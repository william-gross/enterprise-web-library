$ErrorActionPreference = "Stop"

function getCurrentPath {
    (Resolve-Path .\).Path
}

echo "Current Directory is $(getCurrentPath)"
cd ..
echo "Current Directory is now $(getCurrentPath)"

$ewlPackageFolderPath = "packages\EWL"
echo "Checking if $ewlPackageFolderPath exists";
if( Test-Path $ewlPackageFolderPath ) { 
    echo "Exists. Removing folder."
	Remove-Item $ewlPackageFolderPath -Recurse 
}

$packageIdFilePath = "Library\Configuration\EWL Package Id.txt"
echo "`$packageIdFilePath=$packageIdFilePath"

$packageId = if( Test-Path $packageIdFilePath ) { 
    Get-Content $packageIdFilePath } else { 
    "Ewl" 
}
echo "`$packageId=$packageId"

$searchTerm = if( $packageId -eq "Ewl" ) { "PackageId:$packageId" } else { $packageId }
echo "`$searchTerm=$searchTerm"

$packageVersion = "2019.1.8.110110";
echo "`$packageVersion=$packageVersion"

echo "Creating a packages.config"
New-Item packages\EWL\packages.config -Force -ItemType file -Value "<?xml version=`"1.0`" encoding=`"utf-8`"?><packages><package id=`"$packageId`" version=`"$packageVersion`" /></packages>" | Out-Null

echo "Running nuget restore"
& "Solution Files\nuget" restore packages\EWL\packages.config -PackagesDirectory packages\EWL -NonInteractive

cd "packages\EWL\Ewl*\Development Utility"
echo "Current Directory is now $(getCurrentPath)"

echo "Running UpdateAllDependentLogic"
& .\EnterpriseWebLibrary.DevelopmentUtility ..\..\..\.. UpdateAllDependentLogic

echo "Done."