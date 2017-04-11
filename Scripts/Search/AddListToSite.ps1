Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue

$inputFile = "$(Get-Location)\locations.txt"
$listTitle = "Brand Documents"
$listDescription = "Document library to hold documents related to the brand this site represents."

# Read input file and process each line
$reader = [System.IO.File]::OpenText($inputFile)

try {
    while ($null -ne ($line = $reader.ReadLine())) {
        try {
            Write-Host ""
            Write-Host "Info: Processing site at $line..."
            # Get the web
            $web = Get-SPWeb $line
            if ($null -ne $web) {
                # Check if list exists
                $list = $web.Lists[$listTitle]
                if ($null -eq $list) {
                    Write-Host "Info: List not found. Adding..."
                    $web.Lists.Add($listTitle, $listDescription, [Microsoft.SharePoint.SPListTemplateType]::DocumentLibrary)
                    Write-Host -f Green "Done."
                }
                else {
                    Write-Host -f Yellow "Warning: List $listTitle already exists. Skipping..."
                }
            }
            else {
                Write-Host -f Red "Error: No site exists at $line. Moving on to next site..."
            }
        }
        catch {
        }
        finally {
            $web.Dispose()
        }
    }
}
finally {
    $reader.Close()
}