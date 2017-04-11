Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue

$site = Get-SPSite https://connect2dev.beamsuntory.com/
$outputFile = "$(Get-Location)/docinfo.csv"

try {
    $site.AllWebs | % {
        $webTitle = ""
        $webUrl = ""

        $web = $_
        $webUrl = $web.Url
        $webTitle = $web.Title
        $web.Lists | % {
            $listTitle = ""

            $list = $_
            if ($list.Title -contains "Documents") {
                $listTitle = $list.Title
                $list.Items | % {
                    $itemTitle = ""
                    $itemName = ""
                    $itemLanguage = ""
                    $outString = ""


                    $item = $_
                    $itemTitle = $item.Title
                    $itemName = $item.Name
                    $itemLanguage = $item["Language"]
                    $outString = "$webTitle|$webUrl|$listTitle|$itemTitle|$itemName|$itemLanguage"
                    Out-File -FilePath $outputFile -InputObject $outString -Append -Force
                }
            }
        }
    }
}
finally {
    $site.Dispose()
}