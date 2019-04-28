#addin nuget:?package=Cake.XdtTransform&version=0.16.0
#addin nuget:?package=Cake.Powershell&version=0.4.7
#addin nuget:?package=Cake.Http&version=0.5.0
#addin nuget:?package=Cake.Json&version=3.0.1
#addin nuget:?package=Newtonsoft.Json&version=12.0.1
#addin nuget:?package=Cake.Incubator&version=4.0.1

#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

#load local:?path=CakeScripts/helper-methods.cake


var target = Argument<string>("Target", "Default");

var configuration = new Configuration();
var cakeConsole = new CakeConsole();
var configJsonFile = "cake-config.json";
var unicornSyncScript = $"./scripts/Unicorn/Sync.ps1";

/*===============================================
================ MAIN TASKS =====================
===============================================*/

Setup(context =>
{
	cakeConsole.ForegroundColor = ConsoleColor.Yellow;
	PrintHeader(ConsoleColor.DarkGreen);
	
    var configFile = new FilePath(configJsonFile);
    configuration = DeserializeJsonFromFile<Configuration>(configFile);
});

Task("Build-All-Projects")
.IsDependentOn("Build-Solution");

Task("Default")
.WithCriteria(configuration != null)
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Modify-PublishSettings")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.IsDependentOn("Post-Deploy");

Task("Post-Deploy")
.IsDependentOn("Sync-Unicorn");

Task("Quick-Deploy")
.WithCriteria(configuration != null)
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Modify-PublishSettings")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Modify-Unicorn-Source-Folder");

/*===============================================
================= SUB TASKS =====================
===============================================*/

Task("Copy-Sitecore-Lib")
	.WithCriteria(()=>(configuration.BuildConfiguration == "Local"))
    .Does(()=> {
        var files = GetFiles( 
            $"{configuration.WebsiteRoot}/bin/Sitecore*.dll");
        var destination = "./sc.lib";
        EnsureDirectoryExists(destination);
        CopyFiles(files, destination);
}); 

Task("Publish-All-Projects")
.IsDependentOn("Build-Solution")
.IsDependentOn("Publish-Projects")
.IsDependentOn("Publish-XConnect");


Task("Build-Solution").Does(() => {
    MSBuild(configuration.SolutionFile, cfg => InitializeMSBuildSettings(cfg));
});

Task("Publish-Projects").Does(() => {
    PublishProjects($"{configuration.ProjectFolder}\\src\\Foundation\\Umbrella.Pantau\\website", configuration.WebsiteRoot);
});

Task("Publish-XConnect").Does(()=>{
   DeployFiles(
       $"{configuration.ProjectSrcFolder}\\*.dll",
       $"{configuration.XConnectRoot}\\bin");
});
Task("Modify-Unicorn-Source-Folder").Does(() => {
    var zzzDevSettingsFile = File($"{configuration.WebsiteRoot}/App_config/Include/Sitecore.HabitatHome.Fitness/z.Sitecore.HabitatHome.Fitness.DevSettings.config");
    
	var rootXPath = "configuration/sitecore/sc.variable[@name='{0}']/@value";
    var sourceFolderXPath = string.Format(rootXPath, "fitnessSourceFolder");
    var directoryPath = MakeAbsolute(new DirectoryPath(configuration.SourceFolder)).FullPath;

    var xmlSetting = new XmlPokeSettings {
        Namespaces = new Dictionary<string, string> {
            {"patch", @"http://www.sitecore.net/xmlconfig/"}
        }
    };
    XmlPoke(zzzDevSettingsFile, sourceFolderXPath, directoryPath, xmlSetting);
});

Task("Modify-PublishSettings").Does(() => {
    var publishSettingsOriginal = File($"{configuration.ProjectFolder}/publishsettings.targets");
    var destination = $"{configuration.ProjectFolder}/publishsettings.targets.user";

    CopyFile(publishSettingsOriginal,destination);

	return;

	var importXPath = "/ns:Project/ns:Import";

    var publishUrlPath = "/ns:Project/ns:PropertyGroup/ns:publishUrl";

    var xmlSetting = new XmlPokeSettings {
        Namespaces = new Dictionary<string, string> {
            {"ns", @"http://schemas.microsoft.com/developer/msbuild/2003"}
        }
    };
    XmlPoke(destination,importXPath,null,xmlSetting);
    XmlPoke(destination,publishUrlPath,$"{configuration.InstanceUrl}",xmlSetting);
});

Task("Sync-Unicorn").Does(() => {
    var unicornUrl = configuration.InstanceUrl + "unicorn.aspx";
    Information("Sync Unicorn items from url: " + unicornUrl);

    var authenticationFile = new FilePath($"{configuration.WebsiteRoot}/App_config/Include/Unicorn/Unicorn.SharedSecret.config");
    var xPath = "/configuration/sitecore/unicorn/authenticationProvider/SharedSecret";
    
    string sharedSecret = XmlPeek(authenticationFile, xPath);
 
    
   
    
    StartPowershellFile(unicornSyncScript, new PowershellSettings()
                                                        .SetFormatOutput()
                                                        .SetLogOutput()
                                                        .WithArguments(args => {
                                                            args.Append("secret", sharedSecret)
                                                                .Append("url", unicornUrl);
                                                        }));
});


Task("Apply-Xml-Transform").Does(() => {
	// target website transforms 
	Transform($"{configuration.ProjectSrcFolder}\\Fitness.AppItems", configuration.WebsiteRoot);

	// xconnect transforms
	Transform($"{configuration.ProjectSrcFolder}\\Fitness.Automation\\App_Data\\Config\\sitecore\\MarketingAutomation", $"{configuration.XConnectAutomationServiceRoot}\\App_Data\\Config\\sitecore\\MarketingAutomation");
});

Task("Build-NuGet-Packages")
  //.IsDependentOn("Restore-NuGet-Packages")
  //.IsDependentOn("Build")
    .Does(() =>
  {
    var nuGetPackSettings = new NuGetPackSettings
	  {
		  OutputDirectory = $"{configuration.ProjectSrcFolder}\\NuGetPackages",
		  IncludeReferencedProjects = true,
		  Properties = new Dictionary<string, string>
		  {
			  { "Configuration", "Release" },
        { "version", configuration.NuGetVersion },
		  },
      Files = new [] {
          new NuSpecContent {Source = "bin/Umbrella.PanTau.dll", Target = "lib/net472/"},
          new NuSpecContent {Source = "App_Config/**/*", Target = "."},
        }
	  };

    NuGetPack($"{configuration.ProjectFolder}\\src\\Foundation\\Umbrella.Pantau\\website\\Umbrella.Pantau.nuspec", nuGetPackSettings);

    // Get the path to the package.
    //var package = "";

 // Push the package.
 //NuGetPush(package, new NuGetPushSettings {
 //    Source = "",
 //    ApiKey = "AzureDevOps"
 //});
});

RunTarget(target);