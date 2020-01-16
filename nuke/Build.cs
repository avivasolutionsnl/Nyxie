using System;
using System.Linq;

using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.Xunit;
using Nuke.Common.Utilities.Collections;
using Nuke.GitHub;
using static Nuke.GitHub.GitHubTasks;
using static CourierTasks;

using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Xunit.XunitTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter] string GitHubAuthenticationToken;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath ChangeLogFile => RootDirectory / "CHANGELOG.md";
    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Restore"));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Rebuild")
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetMaxCpuCount(Environment.ProcessorCount)
                .SetNodeReuse(IsLocalBuild));
        });

    Target RunTests => _ => _
       .DependsOn(Compile)
       .Executes(() =>
       {
           var unitTestDlls = SourceDirectory.GlobFiles(
               $"*Tests/bin/{Configuration}/*/*.Tests.dll");

           Xunit2(x => x.AddTargetAssemblies(unitTestDlls.Select(x => x.ToString()))
                                    .SetFramework("net471")
                                    .AddResultReport(Xunit2ResultFormat.Xml, OutputDirectory / "testresults.xml"));
       });

    Target Package => _ => _
       .DependsOn(RunTests)
       .Executes(() =>
       {
           Courier(c => c.SetTargetFolder(RootDirectory / "unicorn")
                         .SetOutputPackage(OutputDirectory / $"Nyxie-{GitVersion.AssemblySemVer}.update")
                         .SetRainbowFormat(true));

           DotNetPack(s => s.SetProject(SourceDirectory / "Nyxie.Plugin.Promotions/Nyxie.Plugin.Promotions.csproj")
                            .SetVersion(GitVersion.AssemblySemVer)
                            .SetOutputDirectory(OutputDirectory));
       });

    Target Publish => _ => _
        .DependsOn(Package)
        .Requires(() => GitHubAuthenticationToken)
        .OnlyWhenStatic(() => GitVersion.BranchName.Equals("master") || GitVersion.BranchName.Equals("origin/master"))
        .Executes(async () =>
        {
            var releaseTag = $"v{GitVersion.MajorMinorPatch}";

            var changeLogSectionEntries = ExtractChangelogSectionNotes(ChangeLogFile);
            var latestChangeLog = changeLogSectionEntries
                .Aggregate((c, n) => c + Environment.NewLine + n);
            var completeChangeLog = $"## {releaseTag}" + Environment.NewLine + latestChangeLog;

            var repositoryInfo = GetGitHubRepositoryInfo(GitRepository);
            var packages = GlobFiles(OutputDirectory, "*.nupkg", "*.update")
                                .NotEmpty("No packages found")
                                .ToArray();

            await PublishRelease(c => c.SetArtifactPaths(packages)
                                 .SetCommitSha(GitVersion.Sha)
                                 .SetReleaseNotes(completeChangeLog)
                                 .SetRepositoryName(repositoryInfo.repositoryName)
                                 .SetRepositoryOwner(repositoryInfo.gitHubOwner)
                                 .SetTag(releaseTag)
                                 .SetToken(GitHubAuthenticationToken));
        });

}
