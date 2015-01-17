param([switch]$RunTests = $false, [string]$BuildVersion = "0.0.0.0")

$VerbosePreference = "SilentlyContinue"
$DebugPreference = "SilentlyContinue"
$ErrorActionPreference = "Stop"

function Exit-WithCode
{
  param
  (
    $exitcode
  )

  $host.SetShouldExit($exitcode)
  Exit
}
function Get-ScriptDirectory {
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  $Global:ScriptDirectory = (Split-Path ($Invocation.MyCommand.Path))
  $Global:ScriptDirectory
}
[void] (Get-ScriptDirectory)

function Clean {
  [cmdletBinding(SupportsShouldProcess=$false,ConfirmImpact='Low')]
  param(
    [Parameter(Mandatory=$false,ValueFromPipeline=$false)]
    [switch]$CleanStagingOnly = $false
    )  
  Process {
    # Clean it..
    if (Test-Path $global:artefactDir)
    {
      Write-Host "Removing the artefact directory"
      Remove-Item $global:artefactDir -Recurse -Force
    }
    if ( !(Test-Path $global:artefactDir))
    {
      Write-Host "Creating the artefact directory"
      [void] (New-Item $global:artefactDir -Type Directory)
    }
    else
    {
      Throw "Artefact directory was not cleaned"
    }
	}
}

Function Copy-FilesEx($srcPath, $dstPath, $doRecurse,[string]$Exclude = "") {
  Write-Host ("Copying from " + ($srcPath + "\*.*") + " to " + $dstPath)
  
  if (! (Test-Path -Path $dstPath))
  {
    Write-Host ("Creating directory " + $dstPath + "...")
    [void] (New-Item -ItemType Directory -Path $dstPath)
  }
  if ($doRecurse)
  {  
    [void] (Copy-Item -Path ($srcPath + "\*") -Destination $dstPath -Force -Recurse -Exclude $Exclude)
  }
  else
  {
    [void] (Copy-Item -Path ($srcPath + "\*.*") -Destination $dstPath -Exclude $Exclude)
  }
}

try {
  # Setup directories
  $global:rootDir = ($Global:ScriptDirectory) + "\.."
  $global:artefactDir = ($rootDir + "\artefacts")
  
  # Clear the working directory
  Write-Host "Cleaning the build area..."
  Clean
    
  $SolutionFile = ($global:rootDir + "\GlennSarti.Neo4jRepublisher.sln")
  $ConfigurationName = "Release"
  $ConfigDir = "Release"

  # TODO need to add in version numbers into the assemblies

  $nugetEXE = ($Global:ScriptDirectory) + '\nuget.exe'
  if (!(Test-Path -Path $nugetEXE)) {
    Write-Host 'Downloading nuget.exe...'
    Invoke-WebRequest -Uri 'http://nuget.org/nuget.exe' -OutFile $nugetEXE
  }
  if (!(Test-Path -Path $nugetEXE)) { Throw 'Could not get nuget.exe' }

  Write-Host 'Restoring nuget packages...'
  & $nugetEXE restore "`"$SolutionFile`""

  MSBUILD "$SolutionFile" "/t:Clean;Build" "/p:Configuration=$($ConfigurationName)" "/p:VisualStudioVersion=12.0" "/p:DeployOnBuild=true" "/p:publishURL=$($global:artefactDir)" "/p:PublishProfile=$($Global:ScriptDirectory)\FileSystemDeploy.pubxml"
  $msbuildError = $LASTEXITCODE

  if ($msbuildError -ne 0)
  {
    Throw "MSBUILD returned error code $msbuildError"    
  }
  
  # TODO Modify the Web.Config?
  
  Write-Host "Finished"    
  Exit-WithCode 0
}
catch {
  Write-Host ("ERROR " + $_.Exception.ToString())
  Exit-WithCode 255
}
