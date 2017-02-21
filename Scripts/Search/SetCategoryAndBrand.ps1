Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint.Taxonomy")

$inputFile = "$(Get-Location)\locations.txt"

# Read input file and process each line
$reader = [System.IO.File]::OpenText($inputFile)

# Prepare site, list and field strings
$siteUrl = "http://azjbbfpspdw1.cloudapp.net/"
$listTitle = "Brand Documents"
$categoryFieldTitle = "BeamConnect Category"
$brandFieldTitle = "Brand"

# Prepare term store strings
$metadataServiceName = "Managed Metadata Service Application"
$termGroupName = "Site Collection - azjbbspextdw1-1"
$categoryTermSetName = "Categories"
$brandTermSetName = "Brands"

# Line up term store references
$site = Get-SPSite $siteUrl

if ($site -ne $null) {
    $session = New-Object Microsoft.SharePoint.Taxonomy.TaxonomySession($site)
    $termStore = $session.TermStores[$metadataServiceName]
    $group = $termStore.Groups[$termGroupName]
    $categoryTermSet = $group.TermSets[$categoryTermSetName]
    $brandTermSet = $group.TermSets[$brandTermSetName]
    try {
        while ($null -ne ($line = $reader.ReadLine())) {
            try {
                Write-Host ""
                Write-Host "Info: Processing site at $line..."
                # Get the web
                $web = Get-SPWeb $line
                if ($null -ne $web) {
                    $categorySite = $web.ParentWeb.Title
                    $brandSite = $web.Title

                    # Get the terms if they exist
                    $categoryTerm = $categoryTermSet.Terms | ? { $_.Name -eq $categorySite }
                    $brandTerm = $brandTermSet.Terms | ? { $_.Name -eq $brandSite }

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
                                $categoryField = [Microsoft.SharePoint.Taxonomy.TaxonomyField]$spItem.Fields[$categoryFieldTitle]
                                if (($categoryField -ne $null) -and ($categoryTerm -ne $null)) {
                                    $categoryField.SetFieldValue($spItem, $categoryTerm)
                                    $itemUpdated = $true
                                }
                                $brandField = [Microsoft.SharePoint.Taxonomy.TaxonomyField]$spItem.Fields[$brandFieldTitle]
                                if (($brandField -ne $null) -and ($brandTerm -ne $null)) {
                                    $brandField.SetFieldValue($spItem, $brandTerm)
                                    $itemUpdated = $true
                                }
                                if ($itemUpdated) {
                                    $spItem.SystemUpdate()
                                    Write-Host -f Green "Item updated"
                                }
                            }
                        }
                        # Check if fields exist
                        $categoryField = [Microsoft.SharePoint.Taxonomy.TaxonomyField]
                        if ($list.Fields[$categoryFieldTitle] -ne $null) {
                            # If it does and if the term is available, add it
                            if ($categoryTerm -ne $null) {
                                
                            }
                        }
                        else {
                            Write-Host -f Red "Error: Field $categoryFieldTitle was not found in $listTitle in $($web.Title) site"
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
        $site.Dispose()
    }
}
else {
    Write-Host "Error: The site at $siteUrl was not found."
}