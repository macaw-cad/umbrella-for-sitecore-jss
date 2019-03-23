using System.Text.RegularExpressions;

/*===============================================
================= HELPER METHODS ================
===============================================*/

public class Configuration
{
    private MSBuildToolVersion _msBuildToolVersion;    

    public string WebsiteRoot {get;set;}
    public string XConnectRoot {get;set;}
    public string XConnectIndexerRoot {get;set;}
	public string XConnectAutomationServiceRoot {get;set;}
    public string InstanceUrl {get;set;}
    public string SolutionName {get;set;}
    public string ProjectFolder {get;set;}
    public string BuildConfiguration {get;set;}
    public string MessageStatisticsApiKey {get;set;}
    public string MarketingDefinitionsApiKey {get;set;}
    public bool RunCleanBuilds {get;set;}
	public int DeployExmTimeout {get;set;}
    public string BuildToolVersions 
    {
        set 
        {
            if(!Enum.TryParse(value, out this._msBuildToolVersion))
            {
                this._msBuildToolVersion = MSBuildToolVersion.Default;
            }
        }
    }

    public string SourceFolder => $"{ProjectFolder}";
    public string ProjectSrcFolder => $"{SourceFolder}";

    public string SolutionFile => $"{ProjectFolder}\\{SolutionName}";
    public MSBuildToolVersion MSBuildToolVersion => this._msBuildToolVersion;
    public string BuildTargets => this.RunCleanBuilds ? "Clean;Build" : "Build";
}

public void PrintHeader(ConsoleColor foregroundColor)
{
    cakeConsole.ForegroundColor = foregroundColor;

cakeConsole.WriteLine("     "); 
cakeConsole.WriteLine("     ");     
cakeConsole.WriteLine(@""); 
cakeConsole.WriteLine(@"            __.|.__"); 
cakeConsole.WriteLine(@"    .n887.d8`qb`""-."); 
cakeConsole.WriteLine(@"  .d88' .888  q8b. `."); 
cakeConsole.WriteLine(@" d8P'  .8888   .88b. \                                                                                            "); 
cakeConsole.WriteLine(@"d88_._ d8888_.._9888 _\                                                                                           "); 
cakeConsole.WriteLine(@"  '   '    |    '   '    ____  ____  ___      ___  _______    _______    _______  ___      ___            __      "); 
cakeConsole.WriteLine(@"           |            (""  _||_ "" ||""  \    /""  ||   _  ""\  /""      \  /""     ""||""  |    |""  |          /""""\     "); 
cakeConsole.WriteLine(@"           |            |   (  ) : | \   \  //   |(. |_)  :)|:        |(: ______)||  |    ||  |         /    \    "); 
cakeConsole.WriteLine(@"           |            (:  |  | . ) /\\  \/.    ||:     \/ |_____/   ) \/    |  |:  |    |:  |        /' /\  \   "); 
cakeConsole.WriteLine(@"           |             \\ \__/ // |: \.        |(|  _  \\  //      /  // ___)_  \  |___  \  |___    //  __'  \  "); 
cakeConsole.WriteLine(@"           |             /\\ __ //\ |.  \    /:  ||: |_)  :)|:  __   \ (:      ""|( \_|:  \( \_|:  \  /   /  \\  \ "); 
cakeConsole.WriteLine(@"         `='            (__________)|___|\__/|___|(_______/ |__|  \___) \_______) \_______)\_______)(___/    \___)"); 
cakeConsole.WriteLine("     "); 
cakeConsole.WriteLine("     ");
cakeConsole.ResetColor();

                                                                                         
}

public void PublishProjects(string rootFolder, string websiteRoot)
{
	cakeConsole.WriteLine(rootFolder);
	
    Func<IFileSystemInfo, bool> excludedProjects = fileSystemInfo => !fileSystemInfo.Path.FullPath.Contains("Fitness.Automation.Plugins");

    var projects = GetFiles($"{rootFolder}\\**\\*.csproj", excludedProjects);

    foreach (var project in projects)
    {
		Information($"Publishing project {project}");

        MSBuild(project, cfg => InitializeMSBuildSettings(cfg)
                                   .WithTarget(configuration.BuildTargets)
                                   .WithProperty("DeployOnBuild", "true")
                                   .WithProperty("DeployDefaultTarget", "WebPublish")
                                   .WithProperty("WebPublishMethod", "FileSystem")
                                   .WithProperty("DeleteExistingFiles", "false")
                                   .WithProperty("publishUrl", websiteRoot)
                                   .WithProperty("BuildProjectReferences", "false"));
    }
}

public FilePathCollection GetTransformFiles(string rootFolder)
{
    Func<IFileSystemInfo, bool> exclude_obj_bin_folder =fileSystemInfo => !fileSystemInfo.Path.FullPath.Contains("/obj/") || !fileSystemInfo.Path.FullPath.Contains("/bin/");

	Information($"Collecting transforms from: {rootFolder}");
    var xdtFiles = GetFiles($"{rootFolder}\\**\\*.xdt", exclude_obj_bin_folder);

    return xdtFiles;
}

public void Transform(string rootFolder, string destinationRootFolder) {
    var xdtFiles = GetTransformFiles(rootFolder);

    foreach (var file in xdtFiles)
    {
        Information($"Applying configuration transform:{file.FullPath}");
        var fileToTransform = Regex.Replace(file.FullPath, ".+/(.+)/*.xdt", "$1");
        fileToTransform = Regex.Replace(fileToTransform, ".sc-internal", "");
        var sourceTransform = $"{destinationRootFolder}\\{fileToTransform}";
        
        XdtTransformConfig(sourceTransform			                // Source File
                            , file.FullPath			                // Tranforms file (*.xdt)
                            , sourceTransform);		                // Target File
    }
}
public void DeployFiles(string source, string destination){
    var files = GetFiles($"{source}");
        EnsureDirectoryExists(destination);
        CopyFiles(files, destination);
}
public void RebuildIndex(string indexName)
{
    var url = $"{configuration.InstanceUrl}utilities/indexrebuild.aspx?index={indexName}";
    string responseBody = HttpGet(url);
}

public void DeployExmCampaigns()
{
	var url = $"{configuration.InstanceUrl}utilities/deployemailcampaigns.aspx?apiKey={configuration.MessageStatisticsApiKey}";
	var responseBody = HttpGet(url, settings =>
	{
		settings.AppendHeader("Connection", "keep-alive");
	});

    Information(responseBody);
}

public MSBuildSettings InitializeMSBuildSettings(MSBuildSettings settings)
{
    settings.SetConfiguration(configuration.BuildConfiguration)
            .SetVerbosity(Verbosity.Minimal)
            .SetMSBuildPlatform(MSBuildPlatform.Automatic)
            .SetPlatformTarget(PlatformTarget.MSIL)
            .UseToolVersion(configuration.MSBuildToolVersion)
            .WithRestore();
    return settings;
}

public void CreateFolder(string folderPath)
{
    if (!DirectoryExists(folderPath))
    {
        CreateDirectory(folderPath);
    }
}

public void Spam(Action action, int? timeoutMinutes = null)
{
	Exception lastException = null;
	var startTime = DateTime.Now;
	while (timeoutMinutes == null || (DateTime.Now - startTime).TotalMinutes < timeoutMinutes)
	{
		try {
			action();

			Information($"Completed in {(DateTime.Now - startTime).Minutes} min {(DateTime.Now - startTime).Seconds} sec.");
			return;
		} catch (AggregateException aex) {
		    foreach (var x in aex.InnerExceptions)
				Information($"{x.GetType().FullName}: {x.Message}");
			lastException = aex;
		} catch (Exception ex) {
		    Information($"{ex.GetType().FullName}: {ex.Message}");
			lastException = ex;
		}
	}

    throw new TimeoutException($"Unable to complete within {timeoutMinutes} minutes.", lastException);
}

public void WriteError(string errorMessage)
{
    cakeConsole.ForegroundColor = ConsoleColor.Red;
    cakeConsole.WriteError(errorMessage);
    cakeConsole.ResetColor();
}