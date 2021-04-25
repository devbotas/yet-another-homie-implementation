using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
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
    public static int Main() => Execute<Build>(x => x.Finalize);

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
    string ReleaseNotes { get; set; }


    Target GetVersionInfo => _ => _
        .Executes(() => {
            // Extracting current version. For preview releases, this will save some typing, as the version does not change.
            var currentVersion = ExtractVersion(BigNetProjectFile, "<AssemblyVersion>", "</AssemblyVersion>");
            Console.WriteLine($"Current version is {currentVersion}. Enter another, shall you wish (MAJOR.MINOR.PATCH):");
            var version = Console.ReadLine();
            if (version == "") version = currentVersion.Substring(0, currentVersion.Length - 2); // <-- stripping build number, as we do not operate in those (maybe we should).

            // Figuring out the branch repository is currently on. For non-release branches, like Development or Feature/*, we'll automatically add "preview" suffixes.
            var commitsAheadOfRelease = 0;
            var currentBranch = GitCurrentBranch();
            if ((currentBranch == "Development") || currentBranch.StartsWith("Features/")) {
                IsPreview = true;
                var gitCommandResult = Git($"rev-list --count {GitCurrentBranch()} ^Release");
                // The following line is based on pure faith that nobody changes their underlying implementations...
                commitsAheadOfRelease = int.Parse(((BlockingCollection<Output>)gitCommandResult).Take().Text);
                Logger.Info($"Branch is {currentBranch}, so that's a preview release.");
            }
            else if ((currentBranch == "Release") || currentBranch.StartsWith("Releases/")) {
                IsPreview = false;
                Logger.Info($"Branch is {currentBranch}, so that's NOT a preview release.");
            }
            else {
                ControlFlow.Fail("You're on some illegal branch!");
            }

            // Constructing version strings.
            var finalVersionString = version;
            if (IsPreview) finalVersionString += "-preview." + commitsAheadOfRelease;
            AssemblyVersion = version + ".0";
            FileVersion = version + ".0";
            PackageVersion = finalVersionString;

            ReportSummary(_ => _.AddPair("Version", PackageVersion));
        });

    Target GetReleaseNotes => _ => _
        .DependsOn(GetVersionInfo)
        .Executes(() => {
            Console.WriteLine("Paste (or type) your release notes here. Enter 'q' to finish. Multi-line Markdown is OK. COMMAS ARE NOT OKAY! Don't ask me why.");

            var releaseNotesBuilder = new StringBuilder();
            var continueEntering = true;
            while (continueEntering) {
                var line = Console.ReadLine();
                if (line == "q") { continueEntering = false; }
                else {
                    releaseNotesBuilder.AppendLine(line);
                }
            }

            ReleaseNotes = releaseNotesBuilder.ToString().Trim();

            if (ReleaseNotes.Contains(',')) { ControlFlow.Fail("Commas in release notes break 'dotnet pack' command. I do not know, why. For now, just don't use commas."); }
        });

    Target UpdateVersions => _ => _
        .DependsOn(GetVersionInfo)
        .Executes(() => {
            var csprojContent = File.ReadAllText(BigNetProjectFile);
            ReplaceVersion(ref csprojContent, AssemblyVersion, "<AssemblyVersion>", "</AssemblyVersion>");
            ReplaceVersion(ref csprojContent, FileVersion, "<FileVersion>", "</FileVersion>");
            ReplaceVersion(ref csprojContent, PackageVersion, "<Version>", "</Version>");
            File.WriteAllText(BigNetProjectFile, csprojContent);

            var assemblyInfoContent = File.ReadAllText(NanoNetAssemblyInfoFile);
            ReplaceVersion(ref assemblyInfoContent, AssemblyVersion, "[assembly: AssemblyVersion(\"", "\")]");
            ReplaceVersion(ref assemblyInfoContent, FileVersion, "[assembly: AssemblyFileVersion(\"", "\")]");
            File.WriteAllText(NanoNetAssemblyInfoFile, assemblyInfoContent);
        });

    Target Clean => _ => _
        .DependsOn(UpdateVersions, GetReleaseNotes)
        .Executes(() => {
            EnsureCleanDirectory(OutputDirectory);
        });

    Target RestoreBigNet => _ => _
        .DependsOn(Clean)
        .Executes(() => {
            MSBuild(s => s.SetTargetPath(BigNetProjectFile).SetTargets("Restore"));
        });

    Target RestoreNanoNet => _ => _
        .DependsOn(Clean)
        .Executes(() => {
            MSBuild(s => s.SetTargetPath(NanoNetProjectFile).SetTargets("Restore"));
        });

    Target RebuildBigNet => _ => _
        .DependsOn(RestoreBigNet)
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
        .DependsOn(RestoreNanoNet)
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

    Target PackBigNet => _ => _
        .DependsOn(RebuildBigNet)
        .Produces(OutputDirectory / "*.nupkg")
        .Executes(() => {
            var bigNetNugetSettings = new DotNetPackSettings();
            bigNetNugetSettings = bigNetNugetSettings.SetProject(BigNetProjectFile);
            bigNetNugetSettings = bigNetNugetSettings.SetConfiguration(Configuration);
            bigNetNugetSettings = bigNetNugetSettings.SetNoBuild(InvokedTargets.Contains(RebuildBigNet));
            bigNetNugetSettings = bigNetNugetSettings.SetOutputDirectory(OutputDirectory);
            bigNetNugetSettings = bigNetNugetSettings.SetPackageReleaseNotes(ReleaseNotes);
            DotNetPack(bigNetNugetSettings);

            ReportSummary(_ => _.AddPair("Packages", OutputDirectory.GlobFiles("*.nupkg").Count.ToString()));
        });

    Target PackNanoNet => _ => _
        .DependsOn(RebuildNanoNet)
        .Produces(OutputDirectory / "*.nupkg")
        .Executes(() => {
            var nanoNetNugetSettings = new NuGetPackSettings();
            nanoNetNugetSettings = nanoNetNugetSettings.SetTargetPath(NanoNuspecFile);
            nanoNetNugetSettings = nanoNetNugetSettings.SetConfiguration(Configuration);
            nanoNetNugetSettings = nanoNetNugetSettings.SetBuild(false);
            nanoNetNugetSettings = nanoNetNugetSettings.SetOutputDirectory(OutputDirectory);
            nanoNetNugetSettings = nanoNetNugetSettings.SetProperty("releaseNotes", ReleaseNotes);
            nanoNetNugetSettings = nanoNetNugetSettings.SetBasePath(Configuration == Configuration.Release ? NanoNetDirectory / "bin/release" : NanoNetDirectory / "bin/debug");
            nanoNetNugetSettings = nanoNetNugetSettings.SetVersion(PackageVersion);
            NuGetPack(nanoNetNugetSettings);

            ReportSummary(_ => _.AddPair("Packages", OutputDirectory.GlobFiles("*.nupkg").Count.ToString()));
        });

    Target Finalize => _ => _
        .DependsOn(PackBigNet, PackNanoNet)
        .Executes(() => {

        });

    string ExtractVersion(string file, string startTag, string endTag) {
        var returnVersion = "";

        var content = File.ReadAllText(file);

        var startPosition = content.IndexOf(startTag) + startTag.Length;
        var endPosition = content.IndexOf(endTag, startPosition);

        returnVersion = content.Substring(startPosition, endPosition - startPosition);

        return returnVersion;
    }

    static void ReplaceVersion(ref string content, string versionToInject, string startTag, string endTag) {
        var startPosition = content.IndexOf(startTag) + startTag.Length;
        var endPosition = content.IndexOf(endTag, startPosition);
        content = content.Remove(startPosition, endPosition - startPosition);
        content = content.Insert(startPosition, versionToInject);
    }
}
