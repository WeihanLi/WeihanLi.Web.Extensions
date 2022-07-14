///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var apiKey = Argument("apiKey", "");
var stable = Argument("stable", "false");
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionPath = "./WeihanLi.Web.Extensions.sln";
var srcProjects  = GetFiles("./src/**/*.csproj");
var packProjects = GetFiles("./src/**/*.csproj");
var testProjects  = GetFiles("./test/**/*.csproj");

var artifacts = "./artifacts/packages";
var isWindowsAgent = (EnvironmentVariable("Agent_OS") ?? "Windows_NT") == "Windows_NT";
var branchName = EnvironmentVariable("BUILD_SOURCEBRANCHNAME") ?? "local";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
   PrintBuildInfo();
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("clean")
    .Description("Clean")
    .Does(() =>
    {
       var deleteSetting = new DeleteDirectorySettings()
       {
          Force = true,
          Recursive = true
       };
      if (DirectoryExists(artifacts))
      {
         DeleteDirectory(artifacts, deleteSetting);
      }
    });

Task("restore")
    .Description("Restore")
    .Does(() => 
    {
      foreach(var project in srcProjects)
      {
         DotNetRestore(project.FullPath);
      }
    });

Task("build")    
    .Description("Build")
    .IsDependentOn("clean")
    .IsDependentOn("restore")
    .Does(() =>
    {
      var buildSetting = new DotNetBuildSettings{
         NoRestore = true,
         Configuration = configuration
      };
      foreach(var project in srcProjects)
      {
         DotNetBuild(project.FullPath, buildSetting);
      }
    });

Task("test")    
    .Description("Test")
    .IsDependentOn("build")
    .Does(() =>
    {
      var testSettings = new DotNetTestSettings{
         NoRestore = true,
         Configuration = configuration
      };
      foreach(var project in testProjects)
      {
         DotNetTest(project.FullPath, testSettings);
      }
    });

Task("pack")
    .Description("Pack package")
    .IsDependentOn("build")
    .Does((context) =>
    {
      var settings = new DotNetPackSettings
      {
         Configuration = configuration,
         OutputDirectory = artifacts,
         VersionSuffix = "",
         NoRestore = true,
         NoBuild = true
      };
      if(branchName != "master" && stable != "true"){
         settings.VersionSuffix = $"preview-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
      }
      foreach (var project in packProjects)
      {
         DotNetPack(project.FullPath, settings);
      }
      PublishArtifacts(context);
    });

bool PublishArtifacts(ICakeContext context)
{
   if (context.Environment.Platform.IsUnix())
   {
      return false;
   }
   if (string.IsNullOrEmpty(apiKey))
   {
      apiKey = EnvironmentVariable("Nuget__ApiKey");
   }
   if (!string.IsNullOrEmpty(apiKey))
   {
      var pushSetting =new DotNetNuGetPushSettings
      {
         Source = "https://api.nuget.org/v3/index.json",
         ApiKey = apiKey,
         SkipDuplicate = true
      };
      var packages = GetFiles($"{artifacts}/*.nupkg");
      foreach (var package in packages)
      {
         DotNetNuGetPush(package.FullPath, pushSetting);
      }
      return true;
   }
   return false;
}

void PrintBuildInfo(){
   Information($@"branch:{branchName}, agentOs={EnvironmentVariable("Agent_OS")}
   BuildID:{EnvironmentVariable("BUILD_BUILDID")},BuildNumber:{EnvironmentVariable("BUILD_BUILDNUMBER")},BuildReason:{EnvironmentVariable("BUILD_REASON")}
   ");
}

Task("Default")
    .IsDependentOn("pack");

RunTarget(target);