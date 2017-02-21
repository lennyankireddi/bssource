Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint.Taxonomy")

$outputFile = "$(Get-Location)\terms.txt"
$siteUrl = "http://azjbbfpspdw1.cloudapp.net"
$brandsUrl = "$siteUrl/CONNECT/brands"
$metadataServiceName = "Managed Metadata Service Application"
$termGroupName = "Site Collection - azjbbspextdw1-1"
$categoryTermSetName = "Categories"
$brandTermSetName = "Brands"

# Line up term store references
$site = Get-SPSite $siteUrl
$session = New-Object Microsoft.SharePoint.Taxonomy.TaxonomySession($site)
$termStore = $session.TermStores[$metadataServiceName]
$group = $termStore.Groups[$termGroupName]

$categoryTermSet = $group.TermSets[$categoryTermSetName]
$brandTermSet = $group.TermSets[$brandTermSetName]

# Get a reference to the field to be added
$brandSite = Get-SPWeb $brandsUrl

try {
    if ($null -ne $brandSite) {
        Write-Host "Info: Processing brands site."

        $brandSite.Webs | % {
            $categorySite = $_
            $siteTitle = $categorySite.Title
            Write-Host ""
            Write-Host "Info: Working with Category site - $siteTitle"
            # Check if term for the category exists
            $category = $categoryTermSet.Terms | ? { $_.Name -eq $siteTitle }
            if ($category -eq $null) {
                $dataString = "$siteTitle - UNAVAILABLE"
            }
            else {
                $dataString = "$siteTitle - $($category.Name)"
            }
            Out-File -FilePath $outputFile -InputObject $dataString -Append -Force

            # Iterate through brand sites
            Write-Host "Info: Category site parsed. Now reading brand sites..."
            $categorySite.Webs | % {
                $brandSite = $_
                $brandTitle = $brandSite.Title
                Write-Host "Info: Working with Brand site - $brandTitle"

                # Check if term for brand exists
                $brand = $brandTermSet.Terms | ? { $_.Name -eq $brandTitle }
                if ($brand -eq $null) {
                    $dataString = "$brandTitle - UNAVAILABLE"
                }
                else {
                    $dataString = "$brandTitle - $($brand.Name)"
                }
                Out-File -FilePath $outputFile -InputObject $dataString -Append -Force
            }
            Out-File -FilePath $outputFile -InputObject "" -Append -Force
        }
    }
    else {
        Write-Host "Error: Brands site was not found at $brandsUrl"
    }
}
finally {
    $brandSite.Dispose()
    $site.Dispose()
}