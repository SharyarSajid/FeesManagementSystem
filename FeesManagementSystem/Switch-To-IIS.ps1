$errorActionPreference = "Stop"

# Check for Admin rights
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Warning "Please run this script as Administrator to start/stop services."
    exit
}

# Stop any running console application instances (freeing up Port 80)
Write-Host "Stopping 'FeesManagementSystem' console application..."
Get-Process -Name "FeesManagementSystem" -ErrorAction SilentlyContinue | Stop-Process -Force

# Start IIS (W3SVC)
Write-Host "Starting IIS (World Wide Web Publishing Service)..."
Start-Service W3SVC
Write-Host "IIS Started Successfully." -ForegroundColor Green

Write-Host "`nNow you can browse your site via IIS!"
Write-Host "URL: http://www.APSSchool.com"
