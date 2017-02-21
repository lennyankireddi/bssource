Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue

$inputFile = "$(Get-Location)\locations.txt"
$rootWebUrl = "http://azjbbfpspdw1.cloudapp.net/"
$listTitle = "Brand Documents"
$fieldTitle = "Document Type"

# Read input file and process each line
$reader = [System.IO.File]::OpenText($inputFile)

# Get a reference to the field to be added
$rootWeb = Get-SPWeb $rootWebUrl

if ($null -ne $rootWeb) {
    $field = $rootWeb.Fields[$fieldTitle]
    Write-Host "Info: Retrieved field $($field.Title). Parsing locations to add to..."
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
                        Write-Host "Info: List found - processing..."
                        # Check if the field already exists
                        if ($list.Fields[$fieldTitle] -ne $null) {
                            Write-Host "Skipped: The field already exists"
                        }
                        else {
                            # If field doesn't exist, add it
                            Write-Host "Info: Field not found. Adding..." -NoNewline
                            $list.Fields.Add($field)
                            $list.Update()
                            Write-Host -f Green "Done."
                        }
                    }
                    else {
                        Write-Host "Error: List $listTitle could not be found in site at $($web.Url)"
                    }
                }
                else {
                    Write-Host "Error: No site exists at $line. Moving on to next site..."
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
        $rootWeb.Dispose()
    }
}
else {
    Write-Host "Error: Root web could not be found at $rootWebUrl"
}