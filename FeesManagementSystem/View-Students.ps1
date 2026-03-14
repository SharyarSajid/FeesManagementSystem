$connStr = "Server=.\SQLEXPRESS;Database=FeesManagementSystemDb;Trusted_Connection=True;TrustServerCertificate=True"

$query = "
SELECT TOP 50 
    s.StudentId, 
    s.Name, 
    s.RegistrationNo, 
    c.Name AS ClassName, 
    s.FatherName,
    s.IsDeleted
FROM Students s
LEFT JOIN Classes c ON s.ClassId = c.ClassId
ORDER BY s.StudentId DESC;
"

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = $connStr
    $conn.Open()
    
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $query
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataset) | Out-Null
    
    if ($dataset.Tables[0].Rows.Count -gt 0) {
        $dataset.Tables[0] | Format-Table -AutoSize
    }
    else {
        Write-Host "No students found in the database yet." -ForegroundColor Yellow
    }
    
    $conn.Close()
}
catch {
    Write-Host "Error connecting to database:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Gray
}
