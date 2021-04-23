using System;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

class Build : NukeBuild {
    public static int Main() => Execute<Build>(x => x.Pack);

    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath BigNetDirectory => RootDirectory / "Yahi-bigNET";
    AbsolutePath NanoNetDirectory => RootDirectory / "Yahi-nanoNET";
    AbsolutePath NanoNuspecFile => NanoNetDirectory / "DevBot9.NanoFramework.Homie.nuspec";

    AbsolutePath BigNetProjectFile => BigNetDirectory / "Yahi-bigNET.csproj";
    AbsolutePath NanoNetProjectFile => NanoNetDirectory / "Yahi-nanoNET.nfproj";


    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = Configuration.Release;
    [Parameter("Forcing build to x86, because nanoFramework deosn't build with amd64")]
    readonly MSBuildPlatform MsBuildPlatform = MSBuildPlatform.x86;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() => {
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() => {
            MSBuild(s => s.SetTargetPath(BigNetProjectFile).SetTargets("Restore"));
            MSBuild(s => s.SetTargetPath(NanoNetProjectFile).SetTargets("Restore"));
        });

    Target RebuildBigNet => _ => _
        .DependsOn(Restore)
        .Executes(() => {
            var buildSettings = new MSBuildSettings();
            buildSettings = buildSettings.SetTargetPath(BigNetProjectFile);
            buildSettings = buildSettings.SetTargets("Rebuild");
            buildSettings = buildSettings.SetConfiguration(Configuration);
            buildSettings = buildSettings.SetMSBuildPlatform(MsBuildPlatform);
            buildSettings = buildSettings.SetMaxCpuCount(Environment.ProcessorCount);
            buildSettings = buildSettings.SetNodeReuse(IsLocalBuild);
            //             buildSettings=buildSettings.relea
            MSBuild(buildSettings);
        });

    Target RebuildNanoNet => _ => _
        .DependsOn(Restore)
        .Executes(() => {
            var buildSettings = new MSBuildSettings();
            buildSettings = buildSettings.SetTargetPath(NanoNetProjectFile);
            buildSettings = buildSettings.SetTargets("Rebuild");
            buildSettings = buildSettings.SetConfiguration(Configuration);
            buildSettings = buildSettings.SetMSBuildPlatform(MsBuildPlatform);
            buildSettings = buildSettings.SetMaxCpuCount(Environment.ProcessorCount);
            buildSettings = buildSettings.SetNodeReuse(IsLocalBuild);
            MSBuild(buildSettings);
        });

    Target Pack => _ => _
        .DependsOn(RebuildBigNet, RebuildNanoNet)
        .Produces(OutputDirectory / "*.nupkg")
        .Executes(() => {


            var bigNetNugetSettings = new DotNetPackSettings();
            bigNetNugetSettings = bigNetNugetSettings.SetProject(BigNetProjectFile);
            bigNetNugetSettings = bigNetNugetSettings.SetConfiguration(Configuration);
            bigNetNugetSettings = bigNetNugetSettings.SetNoBuild(InvokedTargets.Contains(RebuildBigNet));
            bigNetNugetSettings = bigNetNugetSettings.SetOutputDirectory(OutputDirectory);
            DotNetPack(bigNetNugetSettings);

            var nanoNetNugetSettings = new NuGetPackSettings();
            nanoNetNugetSettings = nanoNetNugetSettings.SetTargetPath(NanoNuspecFile);
            nanoNetNugetSettings = nanoNetNugetSettings.SetConfiguration(Configuration);
            nanoNetNugetSettings = nanoNetNugetSettings.SetOutputDirectory(OutputDirectory);
            nanoNetNugetSettings = nanoNetNugetSettings.SetBasePath(Configuration == Configuration.Release ? NanoNetDirectory / "bin/release" : NanoNetDirectory / "bin/debug");
            NuGetPack(nanoNetNugetSettings);

            ReportSummary(_ => _.AddPair("Packages", OutputDirectory.GlobFiles("*.nupkg").Count.ToString()));
        });

}
