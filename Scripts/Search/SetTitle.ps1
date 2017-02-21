Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint.Taxonomy")

$inputFile = "$(Get-Location)\locations.txt"

# Read input file and process each line
$reader = [System.IO.File]::OpenText($inputFile)

# Prepare site, list and field strings
$siteUrl = "http://azjbbfpspdw1.cloudapp.net/"
$listTitle = "Brand Documents"

try {
    while ($null -ne ($line = $reader.ReadLine())) {
        try {
            Write-Host ""
            Write-Host "Info: Processing site at $line..."
            # Get the web
            $web = Get-SPWeb $line
            if ($null -ne $web) {

                # Get the list
                $list = $web.Lists[$listTitle]
                if ($null -ne $list) {
                    Write-Host "Info: List found - processing items..."
                    $list.Items | % {
                        $item = $_
                        Write-Host "`t$($item.Name)..." -NoNewline
                        $itemUpdated = $false
                        if ($item.Folder -eq $null) {
                            $spItem = [Microsoft.SharePoint.SPListItem]$item
                            $name = $spItem.Name
                            $newTitle = $name.Substring(0, $name.LastIndexOf('.'))
                            $spItem["Title"] = $newTitle
                            $spItem.SystemUpdate()
                            Write-Host -f Green "Item updated"
                        }
                    }
                }
                else {
                    Write-Host -f Red "Error: List $listTitle could not be found in site at $($web.Url)"
                }
            }
            else {
                Write-Host -f Red "Error: No site exists at $line. Moving on to next site..."
            }
        }
        catch {
            Write-Host -f Red "Error: $($_.Exception.Message)" 
        }
        finally {
            $web.Dispose()
        }
    }
}
finally {
    $reader.Close()
}
