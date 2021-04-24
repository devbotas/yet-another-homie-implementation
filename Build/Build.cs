using System;
using System.IO;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

class Build : NukeBuild {
    public static int Main() => Execute<Build>(x => x.Pack);

    AbsolutePath OutputDirectory => RootDirectory / "Build" / "output";
    AbsolutePath BigNetDirectory => RootDirectory / "Yahi-bigNET";
    AbsolutePath NanoNetDirectory => RootDirectory / "Yahi-nanoNET";
    AbsolutePath NanoNuspecFile => NanoNetDirectory / "DevBot9.NanoFramework.Homie.nuspec";

    AbsolutePath BigNetProjectFile => BigNetDirectory / "Yahi-bigNET.csproj";
    AbsolutePath NanoNetProjectFile => NanoNetDirectory / "Yahi-nanoNET.nfproj";
    AbsolutePath NanoNetAssemblyInfoFile => NanoNetDirectory / "Properties" / "AssemblyInfo.cs";


    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = Configuration.Release;
    [Parameter("Forcing build to x86, because nanoFramework doesn't build with amd64")]
    readonly MSBuildPlatform MsBuildPlatform = MSBuildPlatform.x86;

    string AssemblyVersion { get; set; }
    string FileVersion { get; set; }
    string PackageVersion { get; set; }
    bool IsPreview { get; set; }


    Target GetInfo => _ => _
        .Before(Clean)
        .Executes(() => {
            Console.WriteLine("Version?");
            var version = Console.ReadLine();

            var currentBranch = GitCurrentBranch();
            if ((currentBranch == "Development") || currentBranch.StartsWith("Features/")) {
                IsPreview = true;
                Logger.Info($"Branch is {currentBranch}, so that's a preview release.");
            }
            else if ((currentBranch == "Release") || currentBranch.StartsWith("Releases/")) {
                IsPreview = false;
                Logger.Info($"Branch is {currentBranch}, so that's NOT a preview release.");
            }
            else {
                ControlFlow.Fail("You're on some illegal branch!");
            }

            var finalVersionString = version;
            if (IsPreview) finalVersionString += "-preview." + DateTime.Now.ToString("yyyyMMdd.HHmm");

            AssemblyVersion = version + ".0";
            FileVersion = version + ".0";
            PackageVersion = finalVersionString;

            ReportSummary(_ => _.AddPair("Version", PackageVersion));
        });

    Target UpdateNanoNetVersions => _ => _
        .DependsOn(GetInfo)
        .Executes(() => {
            var assemblyInfoContent = File.ReadAllText(NanoNetAssemblyInfoFile);

            // Replacing AssemblyVersion.
            var assemblyVersionStartPosition = assemblyInfoContent.IndexOf("[assembly: AssemblyVersion(\"") + "[assembly: AssemblyVersion(\"".Length;
            var assemblyVersionEndPosition = assemblyInfoContent.IndexOf("\")]", assemblyVersionStartPosition);
            assemblyInfoContent = assemblyInfoContent.Remove(assemblyVersionStartPosition, assemblyVersionEndPosition - assemblyVersionStartPosition);
            assemblyInfoContent = assemblyInfoContent.Insert(assemblyVersionStartPosition, AssemblyVersion);

            // Replacing FileVersion.
            var fileVersionStartPosition = assemblyInfoContent.IndexOf("[assembly: AssemblyFileVersion(\"") + "[assembly: AssemblyFileVersion(\"".Length;
            var fileVersionEndPosition = assemblyInfoContent.IndexOf("\")]", fileVersionStartPosition);
            assemblyInfoContent = assemblyInfoContent.Remove(fileVersionStartPosition, fileVersionEndPosition - fileVersionStartPosition);
            assemblyInfoContent = assemblyInfoContent.Insert(fileVersionStartPosition, FileVersion);

            File.WriteAllText(NanoNetAssemblyInfoFile, assemblyInfoContent);
        });

    Target Clean => _ => _
        .Before(Restore)
        .DependsOn(UpdateNanoNetVersions)
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
            buildSettings = buildSettings.SetAssemblyVersion(AssemblyVersion);
            buildSettings = buildSettings.SetFileVersion(FileVersion);
            buildSettings = buildSettings.SetPackageVersion(PackageVersion);
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
            buildSettings = buildSettings.SetAssemblyVersion(AssemblyVersion);
            buildSettings = buildSettings.SetFileVersion(FileVersion);
            buildSettings = buildSettings.SetPackageVersion(PackageVersion);
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
            nanoNetNugetSettings = nanoNetNugetSettings.SetBuild(false);
            nanoNetNugetSettings = nanoNetNugetSettings.SetOutputDirectory(OutputDirectory);
            nanoNetNugetSettings = nanoNetNugetSettings.SetBasePath(Configuration == Configuration.Release ? NanoNetDirectory / "bin/release" : NanoNetDirectory / "bin/debug");
            nanoNetNugetSettings = nanoNetNugetSettings.SetVersion(PackageVersion);
            NuGetPack(nanoNetNugetSettings);

            ReportSummary(_ => _.AddPair("Packages", OutputDirectory.GlobFiles("*.nupkg").Count.ToString()));
        });

}
