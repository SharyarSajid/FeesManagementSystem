$errorActionPreference = "Stop"

function Test-SqlConn($connStr) {
    try {
        $conn = New-Object System.Data.SqlClient.SqlConnection
        $conn.ConnectionString = $connStr
        $conn.Open()
        Write-Host "SUCCESS: $connStr" -ForegroundColor Green
        $conn.Close()
        return $true
    }
    catch {
        Write-Host "FAILED:  $connStr" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Gray
        return $false
    }
}

Write-Host "Testing SQL Connections..."
Write-Host "----------------------------"

$configs = @(
    "Data Source=.\SQLEXPRESS;Initial Catalog=FeesManagementSystem_Db;Integrated Security=True;TrustServerCertificate=True",
    "Data Source=.;Initial Catalog=FeesManagementSystem_Db;Integrated Security=True;TrustServerCertificate=True",
    "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=FeesManagementSystem_Db;Integrated Security=True;TrustServerCertificate=True",
    "Data Source=.\SQLEXPRESS;Initial Catalog=FeesManagementSystem_Db;User ID=Dbadmin;Password=admin123;TrustServerCertificate=True",
    "Data Source=.;Initial Catalog=FeesManagementSystem_Db;User ID=Dbadmin;Password=admin123;TrustServerCertificate=True"
)

foreach ($c in $configs) {
    Test-SqlConn $c
}
