$devPromptFolder = Get-ChildItem -Path "C:\Program Files (x86)\Microsoft Visual Studio" -Filter VsDevCmd.bat -Recurse -ErrorAction SilentlyContinue -Force | % { $_.Directory.FullName }
pushd $devPromptFolder
cmd /c "VsDevCmd.bat&set" |
foreach {
  if ($_ -match "=") {
    $v = $_.split("="); set-item -force -path "ENV:\$($v[0])"  -value "$($v[1])"
  }
}
popd
Write-Host "`nVisual Studio 2017 Command Prompt variables set." -ForegroundColor Yellow
$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

if(-Not(Test-Path nuget.exe))
{
    wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile nuget.exe
}

.\nuget restore ..\src\PCFDotNetLegacyToCloudNative.sln

msbuild /p:DeployOnBuild=true /p:DeployDefaultTarget=WebPublish /p:WebPublishMethod=FileSystem /p:PackageAsSingleFile=false /p:SkipInvalidConfigurations=true /p:PublishUrl=..\..\publish\FunnyQuotesUIForms  /p:OutputPath=tmp  ..\src\FunnyQuotesUIForms\FunnyQuotesUIForms.csproj
Remove-Item ..\src\FunnyQuotesUIForms\tmp -recurse
msbuild /p:DeployOnBuild=true /p:DeployDefaultTarget=WebPublish /p:WebPublishMethod=FileSystem /p:PackageAsSingleFile=false /p:SkipInvalidConfigurations=true /p:PublishUrl=..\..\publish\FunnyQuotesLegacyService  /p:OutputPath=tmp  ..\src\FunnyQuotesLegacyService\FunnyQuotesLegacyService.csproj
Remove-Item ..\src\FunnyQuotesLegacyService\tmp -recurse
msbuild /p:DeployOnBuild=true /p:DeployDefaultTarget=WebPublish /p:WebPublishMethod=FileSystem /p:PackageAsSingleFile=false /p:SkipInvalidConfigurations=true /p:PublishUrl=..\..\publish\FunnyQuotesServicesOwin  /p:OutputPath=tmp  ..\src\FunnyQuotesServicesOwin\FunnyQuotesServicesOwin.csproj
Remove-Item ..\src\FunnyQuotesServicesOwin\tmp -recurse
#msbuild /p:DeployOnBuild=true /p:DeployDefaultTarget=WebPublish /p:WebPublishMethod=FileSystem /p:PackageAsSingleFile=false /p:SkipInvalidConfigurations=true /p:PublishUrl=..\..\publish\FunnyQuotesUICore  /p:OutputPath=tmp  ..\src\FunnyQuotesUICore\FunnyQuotesUICore.csproj
#Remove-Item ..\src\FunnyQuotesUICore\tmp -recurse
dotnet restore ..\src\FunnyQuotesUICore\FunnyQuotesUICore.csproj -r ubuntu.14.04-x64
dotnet publish ..\src\FunnyQuotesUICore\FunnyQuotesUICore.csproj -o ..\..\publish\FunnyQuotesUICore -r ubuntu.14.04-x64

Copy-Item manifest.yml ..\publish\manifest.yml
Copy-Item create-services.bat ..\publish\create-services.bat
Copy-Item gitconfig.json ..\publish\gitconfig.json