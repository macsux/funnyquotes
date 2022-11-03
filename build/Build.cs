using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.CloudFoundry;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NerdbankGitVersioning;
using Nuke.Common.Utilities.Collections;
using Octokit;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.HttpTasks;
using static Nuke.Common.Tools.CloudFoundry.CloudFoundryTasks;
using static Nuke.Common.Tools.Docker.DockerTasks;
using Project = Nuke.Common.ProjectModel.Project;

class Build : NukeBuild
{


    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Solution] readonly Solution Solution;
    ProjectsList Projects;
    [GitRepository] readonly GitRepository GitRepository;
    [Parameter("Cloud Foundry Username")]
    readonly string CfUsername;
    [Parameter("Cloud Foundry Password")]
    readonly string CfPassword;
    [Parameter("Cloud Foundry Endpoint")]
    readonly string CfApiEndpoint;
    [Parameter("Cloud Foundry Org")]
    readonly string CfOrg;
    [Parameter("Cloud Foundry Space")]
    readonly string CfSpace;
    [Parameter("Type of database plan (default: db-small)")]
    readonly string DbPlan = "db-small";
    [NerdbankGitVersioning] readonly NerdbankGitVersioning GitVersion;
    
    [Parameter("Runnable projects selected for action by the invoked target(s). Defaults to all")]
    string[] TargetProjects = new[]
    {
        "FunnyQuotesLegacyService",
        "FunnyQuotesOwinWindowsService",
        "FunnyQuotesUICore",
        "FunnyQuotesUIForms",
    };

    [Parameter("GitHub personal access token with access to the repo")]
    string GitHubToken;
    
    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    string ArchiveName => "funny-quotes.zip";
    
    GitHubClient GitHubClient { get; set; }
    string GitHubOwner { get; set; }
    string GitHubRepo { get; set; }

    protected override void OnBuildInitialized()
    {
        GitHubClient = new GitHubClient(new ProductHeaderValue("nuke-build"));
        if (GitHubToken != null)
        {
            GitHubClient.Credentials = new Credentials(GitHubToken, AuthenticationType.Bearer);
        }

        var gitIdParts = GitRepository.Identifier.Split("/");
        GitHubOwner = gitIdParts[0];
        GitHubRepo = gitIdParts[1];
        Projects = new ProjectsList(this);
    }

    Target Clean => _ => _
        .Before(Restore)
        .Description("Clean out all bin/obj directories + artifacts")
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Unlisted()
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Restore"));
        });

    
    Target Publish => _ => _
        .Description($"Compiles all the apps and publishes them for deployment into Artifacts folder. This target only works on Windows. Alternatively you can use {nameof(DownloadArtifacts)} target to download precompiled artifacts.")
        .DependsOn(Restore)
        .Requires(() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        .Executes(() =>
        {
            EnsureCleanDirectory(ArtifactsDirectory);
            var webProjects = Projects.TargetDeployable
                .Where(x => x == Projects.FunnyQuotesUIForms || x == Projects.FunnyQuotesLegacyService);
            MSBuild(s => s
                .SetTargets("Rebuild")
                .SetConfiguration(Configuration)
                .SetMaxCpuCount(Environment.ProcessorCount)
                .SetNodeReuse(IsLocalBuild)
                .SetVerbosity(MSBuildVerbosity.Minimal)
                .CombineWith(webProjects, (oo, project) => oo
                    .SetProjectFile(project.Path)
                    .SetProperties(new Dictionary<string, object>()
                    {
                        {"DeployOnBuild", true},
                        {"DeployDefaultTarget", "WebPublish"},
                        {"WebPublishMethod", "FileSystem"},
                        {"PackageAsSingleFile", "false"},
                        {"SkipInvalidConfigurations", "true"},
                        {"PublishUrl", ArtifactsDirectory / project.Name},
                    })
                ));
            if(Projects.TargetDeployable.Contains(Projects.FunnyQuotesOwinWindowsService))
                MSBuild(s => s
                    .SetTargets("Rebuild")
                    .SetVerbosity(MSBuildVerbosity.Minimal)
                    .SetConfiguration(Configuration)
                    .SetOutDir(ArtifactsDirectory / Projects.FunnyQuotesOwinWindowsService.Name)
                    .SetProjectFile(Projects.FunnyQuotesOwinWindowsService.Path));
            if(Projects.TargetDeployable.Contains(Projects.FunnyQuotesUICore))
                DotNetPublish(s => s
                    .SetProject(Projects.FunnyQuotesUICore.Path)
                    .SetOutput(ArtifactsDirectory / Projects.FunnyQuotesUICore.Name)
                    .SetConfiguration(Configuration));
            foreach (var project in Solution.Projects)
            {
                DeleteFile(ArtifactsDirectory / project.Name / "appsettings.Development.yaml");
            }
        });

    Target CfLogin => _ => _
        .Requires(() => CfUsername, () => CfPassword, () => CfApiEndpoint)
        .Unlisted()
        .Executes(() =>
        {
            CloudFoundryApi(c => c.SetUrl(CfApiEndpoint));
            CloudFoundryAuth(c => c
                .SetUsername(CfUsername)
                .SetPassword(CfPassword));
        });

    
    Target Release => _ => _
        .Description("Creates a GitHub release (or amends existing) and uploads the artifact")
        //.DependsOn(Publish, Pack)
        .Requires(() => GitHubToken)
        .Executes(async () =>
        {
            if (!GitRepository.IsGitHubRepository())
                Assert.Fail("Only supported when git repo remote is github");
            if(!IsGitPushedToRemote)
                Assert.Fail("Your local git repo has not been pushed to remote. Can't create release until source is upload");

            DeleteFile(ArtifactsDirectory / ArchiveName);
            Compress(ArtifactsDirectory, ArtifactsDirectory / ArchiveName);
            
            var releaseName = $"LATEST";
            Release release;
            try
            {
                release = await GitHubClient.Repository.Release.Get(GitHubOwner, GitHubRepo, releaseName);
            }
            catch (NotFoundException)
            {
                var newRelease = new NewRelease(releaseName)
                {
                    Name = releaseName, 
                    Draft = false, 
                    Prerelease = false
                };
                release = await GitHubClient.Repository.Release.Create(GitHubOwner, GitHubRepo, newRelease);
            }

            var existingAsset = release.Assets.FirstOrDefault(x => x.Name == ArchiveName);
            if (existingAsset != null)
            {
                await GitHubClient.Repository.Release.DeleteAsset(GitHubOwner, GitHubRepo, existingAsset.Id);
            }
            
            var releaseAssetUpload = new ReleaseAssetUpload(ArchiveName, "application/zip", File.OpenRead(ArtifactsDirectory / ArchiveName), null);
            var releaseAsset = await GitHubClient.Repository.Release.UploadAsset(release, releaseAssetUpload);
            
            DeleteFile(ArtifactsDirectory / ArchiveName);
            
            Serilog.Log.Information("{DownloadUrl}", releaseAsset.BrowserDownloadUrl);
        });


    Target DownloadArtifacts => _ => _
        .Description("Downloads precompiled binaries from GitHub releases page")
        .Executes(async () =>
        {
            EnsureCleanDirectory(ArtifactsDirectory);
            try
            {
                var release = await GitHubClient.Repository.Release.Get(GitHubOwner, GitHubRepo, "LATEST");
                var asset = release.Assets
                    .FirstOrDefault(x => x.Name == ArchiveName)
                               ?? throw new Exception($"The release doesn't have asset named {ArchiveName}");
                var zipPath = ArtifactsDirectory / ArchiveName;
                HttpDownloadFile(asset.BrowserDownloadUrl, zipPath);
                UncompressZip(zipPath, ArtifactsDirectory);
            }
            catch (NotFoundException)
            {
                Serilog.Log.Error("There's no release with tag LATEST available for github repo {Url}", GitRepository.HttpsUrl);
            }
        });

    Target SetTargetEnvironment => _ => _
        .Unlisted()
        .After(Publish)
        .DependsOn(CfLogin)
        .Requires(() => CfSpace, () => CfOrg)
        .Executes(() =>
        {
            CloudFoundryCreateSpace(c => c
                .SetOrg(CfOrg)
                .SetSpace(CfSpace));
            CloudFoundryTarget(c => c
                .SetSpace(CfSpace)
                .SetOrg(CfOrg));
        });

    Target DeleteAllApps => _ => _
        .Description("Delete all apps in the target space")
        .Executes(() =>
        {
            CloudFoundryDeleteApplication(c => c
                .CombineWith(Projects.TargetDeployable, (oo, project) => oo
                    .SetAppName(project.Name)), degreeOfParallelism: 5);
        });

    // Target Run => _ => _
    //     .After(Publish)
    //     .Executes(() =>
    //     {
    //         
    //         var isDockerWindows = DockerInfo().EnsureOnlyStd().Select(x => x.Text).Any(x => x.Contains("OSType: windows"));
    //         if (!isDockerWindows)
    //         {
    //             Logger.Error("Docker must be in Windows container mode in order to run");
    //             return;
    //         }
    //         ProcessTasks.StartProcess()
    //         DockerRun(c => c
    //             .SetImage("mcr.microsoft.com/dotnet/framework/aspnet:4.8")
    //             .AddVolume(ArtifactsDirectory / nameof(Projects.FunnyQuotesUIForms), "c:/inetpub/wwwroot")
    //             .AddPublish("49478", "80")
    //             .SetRm(true));
    //     });
    
    Target CreateServices => _ => _
        .DependsOn(SetTargetEnvironment)
        .Description("Creates services the app depends on in target Cloud Foundry environment")
        .Executes(async () =>
        {
           
            var config = TemporaryDirectory / "configserver.json";
            File.WriteAllText(config, JObject.FromObject(new
            {
                git = new {
                    uri = GitRepository.HttpsUrl,
                    searchPaths = "config"
                }
            }).ToString(Formatting.Indented));
            CloudFoundryCreateService(c => c
                    .SetService("p.config-server")
                    .SetPlan("standard")
                    .SetInstanceName("config-server")
                    .SetConfigurationParameters(config));
            
            CloudFoundryCreateService(c => c
                .SetService("p.service-registry")
                .SetPlan("standard")
                .SetInstanceName("eureka"));
 
            CloudFoundryCreateService(c => c
                .SetService("p.mysql")
                .SetPlan(DbPlan)
                .SetInstanceName("mysql"));

            await CloudFoundryEnsureServiceReady("config-server");
            await CloudFoundryEnsureServiceReady("eureka");
            await CloudFoundryEnsureServiceReady("mysql");
        });
    Target Deploy => _ => _
        .DependsOn(SetTargetEnvironment, CreateServices)
        .After(Publish)
        .Description("Deploys to Cloud Foundry, including all required services")
        .Executes(() =>
        {
            CloudFoundryPush(c => c
                .SetRandomRoute(true)
                .SetNoStart(true)
                .CombineWith(Projects.TargetDeployable,(cs,project) => cs
                    .SetPath(ArtifactsDirectory / project.Name)
                    .SetManifest(ArtifactsDirectory / project.Name / "manifest.yml")
                ), 
                degreeOfParallelism: 1);
            
            CloudFoundryBindService(c => c
                .SetServiceInstance("eureka")
                .CombineWith(Projects.TargetDeployable,(cs,v) => cs
                    .SetAppName(v.Name)), degreeOfParallelism: 5);
            CloudFoundryBindService(c => c
                .SetServiceInstance("config-server")
                .CombineWith(Projects.TargetDeployable,(cs,v) => cs
                    .SetAppName(v.Name)), degreeOfParallelism: 5);
            CloudFoundryBindService(c => c
                .SetServiceInstance("mysql")
                .CombineWith(new[]{Projects.FunnyQuotesLegacyService, Projects.FunnyQuotesOwinWindowsService},(cs,v) => cs
                    .SetAppName(v.Name)), degreeOfParallelism: 5);
            CloudFoundryStart(o => o
                .CombineWith(Projects.TargetDeployable, (oo, project) => oo
                    .SetAppName(project.Name)));

        });

    Target CreateConfigServer => _ => _
        .Executes(() =>
        {
            var config = TemporaryDirectory / "configserver.json";
            File.WriteAllText(config, JObject.FromObject(new
            {
                git = new {
                    uri = GitRepository.HttpsUrl,
                    searchPaths = "config"
                }
            }).ToString(Formatting.Indented));
            CloudFoundryCreateService(c => c
                .SetService("p-config-server")
                .SetPlan("standard")
                .SetInstanceName("config-server")
                .SetConfigurationParameters(config));
            DeleteFile(config);
        });
    
    class ProjectsList
    {
        readonly Build _parent;

        public ProjectsList(Build parent)
        {
            _parent = parent;
            TargetDeployable = _parent.TargetProjects?.Select(projectName => _parent.Solution.GetProject(projectName)).ToArray();
        }

        public Project[] TargetDeployable { get; set; }
        public Project FunnyQuotesOwinWindowsService => _parent.Solution.GetProject("FunnyQuotesOwinWindowsService");
        public Project FunnyQuotesLegacyService => _parent.Solution.GetProject("FunnyQuotesLegacyService");
        public Project FunnyQuotesUICore => _parent.Solution.GetProject("FunnyQuotesUICore");
        public Project FunnyQuotesUIForms => _parent.Solution.GetProject("FunnyQuotesUIForms");
    }
    bool IsGitPushedToRemote => GitTasks
        .Git("status")
        .Select(x => x.Text)
        .Count(x => x.Contains("nothing to commit, working tree clean") || x.StartsWith("Your branch is up to date with")) == 2;

    public static async Task CloudFoundryEnsureServiceReady(string serviceInstance) => await CloudFoundryEnsureServiceReady(serviceInstance, TimeSpan.FromSeconds(5));
    public static async Task CloudFoundryEnsureServiceReady(string serviceInstance, TimeSpan checkInterval)
    {
        
        var guid = CloudFoundry($"service {serviceInstance} --guid", logOutput: false, logInvocation: false).First().Text;
        bool IsCreating()
        {
            var jsonString = CloudFoundryCurl(c => c
                    .SetPath($"/v2/service_instances/{guid}")
                    // .DisableProcessLogOutput()
                    // .DisableProcessLogInvocation())
                    )
                .EnsureOnlyStd()
                .Aggregate(new StringBuilder(), (sb, output) => sb.AppendLine(output.Text), sb => sb.ToString());
            var response = JObject.Parse(jsonString);
            if (response.ContainsKey("error_code"))
                throw new Exception($"Service creation failed with \n{response["description"]}");
            return response.SelectToken("entity.last_operation.state")?.ToString() == "in progress";
        }

        Serilog.Log.Debug("Waiting service {ServiceInstance} to finish provisioning", serviceInstance);
        while (IsCreating())
        {
            await Task.Delay(checkInterval);
        }
        Serilog.Log.Debug("Service {ServiceInstance} is finished provisioning", serviceInstance);
    }
    
    // public void CreateConfigServer(string serviceName = "")
}
