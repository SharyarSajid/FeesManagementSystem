$connStr = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=FeesManagementSystem;Integrated Security=True;TrustServerCertificate=True"
Write-Host "Testing connection to: $connStr"

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = $connStr
    $conn.Open()
    Write-Host "SUCCESS: Connected to database 'FeesManagementSystem'." -ForegroundColor Green
    $conn.Close()
}
catch {
    Write-Host "FAILED: Could not connect." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Gray
}

# Try connecting to master to see if instance is accessible at all
$connStrMaster = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True"
Write-Host "`nTesting connection to 'master'..."

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = $connStrMaster
    $conn.Open()
    Write-Host "SUCCESS: Connected to 'master' DB." -ForegroundColor Green
    $conn.Close()
}
catch {
    Write-Host "FAILED: Could not connect to 'master'." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Gray
}
