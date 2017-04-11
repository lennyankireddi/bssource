Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint.Taxonomy")

$inputFile = "$(Get-Location)\locations.txt"

# Read input file and process each line
$reader = [System.IO.File]::OpenText($inputFile)

# Prepare site, list and field strings
$siteUrl = "http://azjbbfpspdw1.cloudapp.net/"
$listTitle = "Brand Documents"
$fieldTitle = "Document Type"

# Prepare term store strings
$metadataServiceName = "Managed Metadata Service Application"
$termGroupName = "Site Collection - azjbbspextdw1-1"
$documentTypeTermSetName = "Document Type"
$brandLocationTermSetName = "Brand Locations"

# Line up term store references
$site = Get-SPSite $siteUrl

if ($site -ne $null) {
    $session = New-Object Microsoft.SharePoint.Taxonomy.TaxonomySession($site)
    $termStore = $session.TermStores[$metadataServiceName]
    $group = $termStore.Groups[$termGroupName]
    $documentTypeTermSet = $group.TermSets[$documentTypeTermSetName]
    try {
        while ($null -ne ($line = $reader.ReadLine())) {
            try {
                Write-Host ""
                Write-Host "Info: Processing site at $line..."

                $numDocs = 0
                # Get the web
                $web = Get-SPWeb $line
                if ($null -ne $web) {
                    # Get the list
                    $list = $web.Lists[$listTitle]
                    if ($null -ne $list) {
                        Write-Host "Info: List found - processing items..."
                        $documentTypeField = [Microsoft.SharePoint.Taxonomy.TaxonomyField]$list.Fields[$fieldTitle]
                        if ($documentTypeField -ne $null) {
                            $list.Items | % {
                                $item = $_
                                Write-Host "`t$($item.Url)..."
                                $itemUpdated = $false
                                $termValueCollection = New-Object Microsoft.SharePoint.Taxonomy.TaxonomyFieldValueCollection($documentTypeField)

                                if ($item.Folder -eq $null) {
                                    # Get Brand Location terminal value
                                    $valueAdded = $false
                                    $item["BeamConnect Brand Location"] | % {
                                        $brandLocationTerm = $_.Label
                                        if (-not [System.String]::IsNullOrEmpty($brandLocationTerm)) {
                                            $brandLocationTerm = $brandLocationTerm.Substring($brandLocationTerm.LastIndexOf(':') + 1, $brandLocationTerm.Length - $brandLocationTerm.LastIndexOf(':') - 1)
                                            if ($brandLocationTerm.EndsWith('s')) {
                                                $brandLocationTerm = $brandLocationTerm.Substring(0, $brandLocationTerm.Length - 1)
                                            }
                                        }

                                        # Get the terms if they exist
                                        Write-Host "`tLooking for term - $brandLocationTerm..."
                                        $documentTypeTerm = $documentTypeTermSet.Terms | ? { $_.Name -eq $brandLocationTerm }
                                        
                                        if ($documentTypeTerm -eq $null) {
                                            Write-Host -f Yellow "`tTerm not found. Skipping..."
                                        }
                                        else {
                                            $documentTypeFieldValue = New-Object Microsoft.SharePoint.Taxonomy.TaxonomyFieldValue($documentTypeField)
                                            $documentTypeFieldValue.TermGuid = $documentTypeTerm.Id
                                            $documentTypeFieldValue.Label = $documentTypeTerm.Name
                                            $termValueCollection.Add($documentTypeFieldValue)
                                            $valueAdded = $true
                                        }
                                    }
                                    if ($valueAdded) {
                                        Write-Host "`tUpdating item..." -NoNewline
                                        $spItem = [Microsoft.SharePoint.SPListItem]$item
                                        $documentTypeField.SetFieldValue($spItem, $termValueCollection)
                                        $itemUpdated = $true
                                        $spItem.SystemUpdate()
                                        $numDocs++
                                        Write-Host -f Green "Done."
                                    }
                                    else {
                                        Write-Host -f Yellow "`tNo new values found. Skipping..."
                                    }
                                }
                            }
                        }
                        else {
                            Write-Host -f Yellow "List does not contain $fieldType field."
                        }
                    }
                    else {
                        Write-Host -f Red "Error: List $listTitle could not be found in site at $($web.Url)"
                    }
                    Write-Host "$numDocs documents updated."
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