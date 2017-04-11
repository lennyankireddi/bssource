Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue

$inputFile = "$(Get-Location)\webs.txt"

# Read input file and process each line
$reader = [System.IO.File]::OpenText($inputFile)
try {
    while ($null -ne ($line = $reader.ReadLine())) {
        try {
            $parts = $line.Split(",")
            $webTitle = $parts[0]
            $webUrl = $parts[1]
            $webTemplate = $parts[2]

            # Create the web
            New-SPWeb -Name $webTitle -Url $webUrl -Template $webTemplate

            Write-Host -f Green "Created web at $webUrl"
        }
        catch {
            Write-Host "Failed to create web at $webUrl"
            Write-Host -f Yellow $_.Exception.Message
        }
    }
}
finally {
    $reader.Close()
}