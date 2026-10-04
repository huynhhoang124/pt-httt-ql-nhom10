# Chạy CSDL lên SQL Server: schema.sql -> views.sql -> seed.sql -> kiem_tra.sql
# Không cần sqlcmd: dùng System.Data.SqlClient có sẵn trong .NET của Windows.
# Cách dùng:  powershell -ExecutionPolicy Bypass -File csdl/chay.ps1 [-May .\SQLEXPRESS] [-Tep schema.sql,views.sql]
# Thoát với mã 1 nếu có lỗi SQL hoặc có dòng kiểm tra cho kết quả 'LỖI'.
param(
    [string]$May = '.\SQLEXPRESS04',
    [string[]]$Tep = @('schema.sql', 'views.sql', 'seed.sql', 'kiem_tra.sql')
)
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.Encoding]::UTF8
$Tep = $Tep -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ }   # -File truyền "a.sql,b.sql" thành một chuỗi
$thuMuc = Split-Path -Parent $MyInvocation.MyCommand.Path
$cn = New-Object System.Data.SqlClient.SqlConnection "Server=$May;Database=master;Integrated Security=True;TrustServerCertificate=True"
$cn.Open()
$coLoi = $false
try {
    foreach ($t in $Tep) {
        $sql = [IO.File]::ReadAllText((Join-Path $thuMuc $t), [Text.Encoding]::UTF8)
        $loNhom = [regex]::Split($sql, '(?im)^\s*GO\s*$') | Where-Object { $_.Trim() }
        Write-Host "== $t ($($loNhom.Count) lô lệnh)"
        foreach ($lo in $loNhom) {
            $cmd = $cn.CreateCommand()
            $cmd.CommandText = $lo
            $cmd.CommandTimeout = 120
            $da = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
            $ds = New-Object System.Data.DataSet
            try { [void]$da.Fill($ds) }
            catch { Write-Host "LỖI SQL: $($_.Exception.InnerException.Message)`n$($lo.Trim().Substring(0, [Math]::Min(300, $lo.Trim().Length)))"; $coLoi = $true; continue }
            foreach ($bang in $ds.Tables) {
                if ($bang.Rows.Count -eq 0) { continue }
                $bang | Format-Table -AutoSize -Wrap | Out-String -Width 220 | Write-Host
                if ($bang.Columns.Contains('KetQua') -and ($bang.Select("KetQua = 'LỖI'").Count -gt 0)) { $coLoi = $true }
            }
        }
    }
}
finally { $cn.Close() }
if ($coLoi) { Write-Host 'KẾT QUẢ: CÓ LỖI'; exit 1 } else { Write-Host 'KẾT QUẢ: ĐẠT' }
