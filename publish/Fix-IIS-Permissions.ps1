# Fix-IIS-Permissions.ps1
# Run as Administrator

$ErrorActionPreference = "Stop"
$serverName = ".\SQLEXPRESS"
$databaseName = "FeesManagementSystemDb"
$appPoolName = Read-Host "Enter IIS Application Pool Name"

Write-Host "Granting SQL Permissions for IIS App Pool: $appPoolName" -ForegroundColor Cyan

# SQL Command to Create Login and User
$sqlCmd = @"
USE [master];
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'IIS AppPool\$appPoolName')
BEGIN
    CREATE LOGIN [IIS AppPool\$appPoolName] FROM WINDOWS;
END;

USE [$databaseName];
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'IIS AppPool\$appPoolName')
BEGIN
    CREATE USER [IIS AppPool\$appPoolName] FOR LOGIN [IIS AppPool\$appPoolName];
END;

ALTER ROLE db_owner ADD MEMBER [IIS AppPool\$appPoolName];
"@

try {
    # Execute SQL Command using Invoke-Sqlcmd or sqlcmd.exe
    # We use sqlcmd.exe as it is more likely to be available
    $queryFile = "$PSScriptRoot\temp_permissions.sql"
    $sqlCmd | Out-File -FilePath $queryFile -Encoding ASCII
    
    Write-Host "Executing SQL update..."
    sqlcmd -S $serverName -E -C -i $queryFile
    
    Remove-Item $queryFile
    Write-Host "Success! Permission granted to 'IIS AppPool\$appPoolName'." -ForegroundColor Green
    Write-Host "Try logging in again on http://www.APSSchool.com"
}
catch {
    Write-Error "Failed to update permissions: $_"
    Write-Host "Ensure you are running as Administrator and SQLEXPRESS is running." -ForegroundColor Yellow
}
