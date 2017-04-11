Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue

$outputFile = "$(Get-Location)\locations.txt"
$startWeb = "http://azjbbfpspdw1.cloudapp.net/connect/brands"

$brands = Get-SPWeb $startWeb
try {
    if ($brands -ne $null) {
        # Iterate through the category sites
        $brands.Webs | % {
            $category = $_
            Write-Host "Info: Parsing site - $($category.Title)..."
            $category.Webs | % {
                Out-File -FilePath $outputFile -InputObject $($_.Url) -Append -Force
            }
        }
    }
    else {
        Write-Host "Error: Brand root site could not be found"
    }
}
finally {
    $brands.Dispose()
}