Add-PSSnapin Microsoft.SharePoint.PowerShell

$outputFile = "ctusage.txt"
$siteUrl = "http://azjbbfpspdw1.cloudapp.net"
$ctname = "Program"

$site = Get-SPSite $siteUrl
$site.AllWebs | % {
    $web = $_
    $ctype = $web.ContentTypes[$ctname]
    if ($ctype) {
        $ctusage = [Microsoft.SharePoint.SPContentTypeUsage]::GetUsages($ctype)
        $ctusage | % {
            Out-File -FilePath $ctname -InputObject $ctusage.Url -Append -Force
        }
    }
}

$site.Dispose()