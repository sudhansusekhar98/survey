$conn = New-Object System.Data.SqlClient.SqlConnection("Server=10.0.32.135;Database=VLDev;User Id=adminrole;Password=@dminr0le;")
$conn.Open()

$cmd = New-Object System.Data.SqlClient.SqlCommand("dbo.SpAdminItemMasterType", $conn)
$cmd.CommandType = [System.Data.CommandType]::StoredProcedure
$cmd.Parameters.AddWithValue("@SpType", 10) | Out-Null
$cmd.Parameters.AddWithValue("@TypeId", 109) | Out-Null

$adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$dt = New-Object System.Data.DataTable
$rowCount = $adapter.Fill($dt)

Write-Host "Rows returned: $rowCount"
Write-Host "Columns: $($dt.Columns.Count)"

foreach ($row in $dt.Rows) {
    $itemId = $row["ItemId"]
    $name = $row["ItemName"]
    $isActive = $row["IsActive"]
    $sqNo = $row["SqNo"]
    Write-Host "ItemId=$itemId Name=$name IsActive=$isActive SqNo=$sqNo"
}

$conn.Close()
