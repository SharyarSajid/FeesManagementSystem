$scriptDir = $PSScriptRoot
Set-Location $scriptDir



$publishDir = ".\publish"

# Check for Admin to stop IIS if needed
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Warning "Please run this script as Administrator to ensure port 80 can be used."
}

# Stop IIS (W3SVC)
$iisService = Get-Service W3SVC -ErrorAction SilentlyContinue
if ($iisService -and $iisService.Status -eq 'Running') {
    Write-Host "Stopping IIS (W3SVC)..."
    Stop-Service W3SVC -Force
}

# Stop any running instances of the app
Write-Host "Stopping existing application instances..."
Get-Process -Name "FeesManagementSystem" -ErrorAction SilentlyContinue | Stop-Process -Force


# Clean previous publish
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

# Publish Release
Write-Host "Publishing Application..."
dotnet publish -c Release -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed!"
    exit
}

# Ensure Database Exists (Run Migrations)


Write-Host "Applying Database Migrations..."
# We temporarily set the environment variable for the migration command to pick up the Production string if it defaults to Development
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet ef database update

# Run
Write-Host "Starting Application..."
Write-Host "Access at: http://www.APSSchool.com"
Set-Location $publishDir
dotnet FeesManagementSystem.dll --environment Production --urls "http://www.APSSchool.com:80"
