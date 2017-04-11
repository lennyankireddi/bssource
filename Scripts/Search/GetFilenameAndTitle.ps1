Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint.Taxonomy")

$inputFile = "$(Get-Location)\locations.txt"
$outputFile = "$(Get-Location)\filenametitle.txt"
$listTitle = "Brand Documents"

# Read input file and process each line
$reader = [System.IO.File]::OpenText($inputFile)

# Prepare output file
Out-File -FilePath $outputFile -InputObject "ID,Name,File Name,Title" -Append -Force

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
                    $outputString = "$($_.Id),$($_.Name),$($_.File.Name),$($_.Title)"
                    Out-File -FilePath $outputFile -InputObject $outputString -Append -Force
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