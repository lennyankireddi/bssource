Add-PSSnapin Microsoft.SharePoint.PowerShell

$outputFile = "ctusage.txt"
$siteUrl = "http://azjbbfpspdw1.cloudapp.net"
$ctname = "Program"

$site = Get-SPSite $siteUrl
$site.AllWebs | % {
    Write-Host "Emptying recycle bin at $($_.Url)"
    $_.RecycleBin.MoveAllToSecondStage()
}
Write-Host "Emptying site recycle bin"
$site.RecycleBin.DeleteAll()

$site.Dispose()