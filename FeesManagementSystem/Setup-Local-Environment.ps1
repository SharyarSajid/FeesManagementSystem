# Check if running as Administrator
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Warning "This script must be run as Administrator to modify the hosts file and firewall rules."
    Start-Sleep -Seconds 3
    exit
}

$domain = "www.APSSchool.com"
$ip = "127.0.0.1"
$hostsFile = "$env:SystemRoot\System32\drivers\etc\hosts"

# Add to hosts file
$hostEntry = "$ip $domain"
if (Select-String -Path $hostsFile -Pattern $domain) {
    Write-Host "Domain '$domain' already exists in hosts file."
}
else {
    Add-Content -Path $hostsFile -Value "`r`n$hostEntry"
    Write-Host "Added '$domain' to hosts file."
}

# Add Firewall Rule (Optional, but good for testing access from other devices if binding to 0.0.0.0, here we bind to 127.0.0.1 purely for local simulation)
# For local dev with custom domain, we often don't need a firewall rule if just accessing from the same machine.
# However, if we wanted to allow external access, we would need to bind to specific IP and open firewall.
# For now, we'll skip firewall modification as we are mapping 127.0.0.1 which is local only.

Write-Host "Setup complete. You can now access the site at http://$domain (after starting the app)."
Start-Sleep -Seconds 3
