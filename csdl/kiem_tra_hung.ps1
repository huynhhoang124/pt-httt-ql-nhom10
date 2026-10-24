# TC38: thử nâng cấp 002 trong giao dịch rồi hoàn tác, không thay đổi dữ liệu đang dùng.
# Chạy: powershell -ExecutionPolicy Bypass -File csdl/kiem_tra_hung.ps1 [-May .\SQLEXPRESS04]
param(
    [string]$May = '.\SQLEXPRESS04',
    [string]$ChuoiKetNoi = $env:NHOM10_SQL_CONNECTION
)
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.Encoding]::UTF8
if (-not $ChuoiKetNoi) { $ChuoiKetNoi = "Server=$May;Database=TrungTamNgoaiNgu;Integrated Security=True;TrustServerCertificate=True" }
$cn = New-Object System.Data.SqlClient.SqlConnection $ChuoiKetNoi
$cn.Open()
$tx = $cn.BeginTransaction()
try {
    function Sql([string]$s) {
        $c = $cn.CreateCommand()
        $c.Transaction = $tx
        $c.CommandText = $s
        $c.CommandTimeout = 60
        $c.ExecuteScalar()
        $c.Dispose()
    }
    function NangCap {
        $s = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'nang_cap/002_phien_ban_csdl.sql'), [Text.Encoding]::UTF8)
        foreach ($lo in [regex]::Split($s, '(?im)^\s*GO\s*$')) {
            if ($lo.Trim()) { [void](Sql $lo) }
        }
    }
    [void](Sql "DELETE dbo.ThamSo WHERE MaThamSo = 'PHIEN_BAN_CSDL'")
    $truoc = [int](Sql 'SELECT COUNT(*) FROM dbo.ThamSo')
    NangCap
    $sau = [int](Sql 'SELECT COUNT(*) FROM dbo.ThamSo')
    if ($sau -ne ($truoc + 1) -or (Sql "SELECT GiaTri FROM dbo.ThamSo WHERE MaThamSo = 'PHIEN_BAN_CSDL'") -ne 2) { throw '002 không thêm đúng một dòng phiên bản 2.' }
    NangCap
    if ((Sql 'SELECT COUNT(*) FROM dbo.ThamSo') -ne $sau -or (Sql "SELECT GiaTri FROM dbo.ThamSo WHERE MaThamSo = 'PHIEN_BAN_CSDL'") -ne 2) { throw 'Chạy lại 002 không giữ nguyên kết quả.' }
    [void](Sql "UPDATE dbo.ThamSo SET GiaTri = 1 WHERE MaThamSo = 'PHIEN_BAN_CSDL'")
    NangCap
    if ((Sql "SELECT GiaTri FROM dbo.ThamSo WHERE MaThamSo = 'PHIEN_BAN_CSDL'") -ne 2) { throw 'Không nâng phiên bản từ 1 lên 2.' }
    [void](Sql "UPDATE dbo.ThamSo SET GiaTri = 3 WHERE MaThamSo = 'PHIEN_BAN_CSDL'")
    NangCap
    if ((Sql "SELECT GiaTri FROM dbo.ThamSo WHERE MaThamSo = 'PHIEN_BAN_CSDL'") -ne 3) { throw '002 hạ phiên bản của tệp nâng cấp sau.' }
    Write-Host 'TC38 SQL: ĐẠT (thêm mới, chạy lại, nâng 1 lên 2, không hạ phiên bản).'
    Write-Host ('TC37 tổng phải nộp để đối chiếu R3.1: ' + (Sql 'SELECT SUM(d.SoTien) FROM dbo.DotHocPhi d JOIN dbo.HocPhi h ON h.MaHV = d.MaHV AND h.MaLop = d.MaLop'))
}
finally {
    if ($tx.Connection) { $tx.Rollback() }
    $cn.Close()
    $cn.Dispose()
}
