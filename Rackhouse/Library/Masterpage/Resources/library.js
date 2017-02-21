function GetFavoritesForUser()
{
	jQuery.ajax({
		url:"/_api/web/lists/GetByTitle('Favorites')/items?$select=ID,URL,Order,Author/Id&$expand=Author&$filter=(Author/Id eq " + _spPageContextInfo.userId + ")&$orderby=Order asc",
		type:"GET",
		headers:{Accept:"application/json;odata=verbose"},
		success:function(data)
		{
			rkhs_favoriteItems = data.d.results;
		},error:function(data) {
			console.log(JSON.stringify(data));
		}
	});
}

function GetFavoriteId(docUrl) {
    for (index = 0; index < rkhs_favoriteItems.length; index++) {
        if (rkhs_favoriteItems[index].URL.Url == docUrl) {
            return rkhs_favoriteItems[index].Id;
        }
    }
    return -1;
}

function AddFavorite(url, title,element)
{
	console.log("AddFavorite (func) - URL: " + url + " Title: " + title + " Element: " + element);
	var d = {
				'__metadata':
				{
					'type':'SP.Data.FavoritesListItem'
				},
				'URL':{
					'__metadata':{'type':'SP.FieldUrlValue'},
					'Description':title,
					'Url':url
				}
			};
	jQuery.ajax({
		url:"/_api/web/lists/GetByTitle('Favorites')/items",
		type:"POST",
		headers:{
			"accept":"application/json;odata=verbose",
            "X-RequestDigest":jQuery("#__REQUESTDIGEST").val(),
			"content-type":"application/json;odata=verbose"
		},
		data: JSON.stringify(d),
		success:function(e) {
			if(typeof(element) != 'undefined')
				jQuery(element).removeClass('fa-star-o').addClass('fa-star');
		},
		error:function(e)
		{
			console.log(JSON.stringify(e));
		}
	});
}

function RemoveFavorite(id, element)
{
    console.log("RemoveFavorite (func) - ID: " + id);
	jQuery.ajax({
		url:"/_api/web/lists/GetByTitle('Favorites')/items(" + id + ")",
		type:"POST",
		headers:{
			"accept":"application/json;odata=verbose",
            "X-HTTP-Method":"DELETE",
            "X-RequestDigest":jQuery("#__REQUESTDIGEST").val(),
			"IF-MATCH":"*"
		},
		success:function(e) {
			if(typeof(element) != undefined)
				jQuery(element).removeClass('fa-star').addClass('fa-star-o');
		},
		error:function(e)
		{
			console.log(JSON.stringify(e));
		}
	});
}

function ToggleFavorite(element, url, title) {
    console.log("ToggleFavorite (func) - URL: " + url + " Title: " + title + " Element: " + element);
    if (jQuery(element).hasClass("fa-star-o")) {
        AddFavorite(url, title, element);
    }
    else if (jQuery(element).hasClass("fa-star")) {
        var favoriteId = GetFavoriteId(url);
        if (favoriteId != -1) {
            RemoveFavorite(favoriteId, element);
        }
    }
}

function GetMonthText(month) {
    switch (month) {
        case 0:
            return "Jan";
        case 1:
            return "Feb";
        case 2:
            return "Mar";
        case 3:
            return "Apr";
        case 4:
            return "May";
        case 5:
            return "Jun";
        case 6:
            return "Jul";
        case 7:
            return "Aug";
        case 8:
            return "Sep";
        case 9:
            return "Oct";
        case 10:
            return "Nov";
        case 11:
            return "Dec";
    }
}

function GetTimeString(date) {
    var hours = date.getHours();
    var ampm = hours > 11 ? "PM" : "AM";
    var minutes = date.getMinutes();
    minutes = minutes.toString().length == 1 ? "0" + minutes.toString() : minutes.toString();
    hours = hours > 12 ? hours - 12 : hours;
    hours = hours == 0 ? 12 : hours;
    return hours + ":" + minutes + " " + ampm;
}

function IsOfficeDocument(extension) {
    switch(extension.toUpperCase()) {
        case "PPT":
            return true;
            break;
        case "PPTX":
            return true;
        case "DOC":
            return true;
            break;
        case "DOCX":
            return true;
        case "XLS":
            return true;
            break;
        case "XLSX":
            return true;
        default:
            return false;
    }
}

function IsVideoFile(extension) {
    switch (extension.toUpperCase()) {
        case "MP4":
            return true;
        case "MOV":
            return true;
        default:
            return false;
    }
}

function PrepURLs(body) {
    while (body.indexOf("<") != -1) {
        start = body.indexOf("<");
        end = body.indexOf(">");
        bracedUrl = body.substr(start, end - start + 1);
        url = body.substr(start + 1, end - start - 1);
        body = body.replace(bracedUrl, encodeURI(url));
    }
    return body;
}

function ReplaceLanguageInUrl(url, lang) {
    var newUrl =  url.replace("/zhcn/", lang).replace("/zhhk/", lang).replace("/de/", lang).replace("/es/", lang).replace("/fr/", lang).replace("/ja/", lang).replace("/pl/", lang).replace("/pt/", lang).replace("/ru/", lang);
    console.log(newUrl);
    return newUrl;
}

function PrintLibraryItems(element){

    jQuery.ajax({
        url: ReplaceLanguageInUrl(_spPageContextInfo.webAbsoluteUrl, "/CONNECT/") + "/_api/web/lists/GetByTitle('Rackhouse Documents')/items?$expand=File&$orderby=Modified desc",
        type:"GET",
        headers:{Accept:"application/json;odata=verbose"},
        success:function(data)
        {
            var category = "";
            var docName = "";
            var docModified = "";
            var date = "";
            var ampm = "";
            var hour = "";
            var extension = "";
            var iconUrl = "";
            var imageUrl = "";
            var owaUrl = "";
            var videoUrl = "";
            var docUrl = "";
            var newLine = "%0D%0A"
            var emailBody = "";
            var mobileEmailBody = "";
            var html = "";
            var featuredDoc = "";
            var ftDocsAdded = 0;
            var keys = [];
            var arrayList = [];
            var docData = "";

            jQuery.each(data.d.results, function(i, n) {
                imageUrl = "";
                // Get category and name of document
                category = n.OData__Category;
                docName = n.File.Name; 

                // Convert date Modified
                date = new Date(n.Modified);
                docModified = date.getDate() + " " + GetMonthText(date.getMonth()) + " " + date.getFullYear() + " " + GetTimeString(date);

                // Get feature image if available
                if (n.Featured_x0020_Image) {
                    imageUrl = n.Featured_x0020_Image.Url;
                }

                // Get extension and retrieve icon
                extension = docName.substr(docName.lastIndexOf('.') + 1);
                iconUrl = GetDocIcon(extension);
                
                // Prepare document and OWA (if office doc) URLs
                docUrl = _spPageContextInfo.siteAbsoluteUrl + n.File.ServerRelativeUrl;
                if (IsVideoFile(extension)) {
                    videoUrl = _spPageContextInfo.siteAbsoluteUrl + "/Pages/VideoPlayer.aspx?videopath=" + n.File.ServerRelativeUrl;
                }
                if (IsOfficeDocument(extension)) {
                    owaUrl = _spPageContextInfo.webAbsoluteUrl + "/_layouts/15/WopiFrame.aspx?sourcedoc=" + n.File.ServerRelativeUrl;
                }
                
                // Prepare email body for mailto tag
                emailBody = "Open:" + newLine + "<" + encodeURI(docUrl) + ">";
                mobileEmailBody = "<a href='" + docUrl + "'>Open</a>";
                if (IsOfficeDocument(extension)) {
                    emailBody += newLine + "Open in Web:" + newLine + "<" + encodeURI(owaUrl) + ">";
                    mobileEmailBody += "<br/><br/><a href='" + owaUrl + "'>Open in web</a>";
                }

                docData = iconUrl + "|" + docName + "|" + docUrl + "|" + docModified + "|" + emailBody + "|" + owaUrl + "|" + videoUrl + "|" + n.Featured + "|" + mobileEmailBody;

                // Check if category was already found
                if (keys.indexOf(category) == -1) {
                    // If not, add it to array of keys
                    index = keys.push(category);
                    // Check if an array exists at that index
                    if (arrayList[index - 1]) {
                        // Array exists (unlikely). Add data to it
                        arrayList[index - 1].push(docData);
                    }
                    else {
                        // Array doesn't exist (more likely for new file type). Add new array at that position
                        arrayList[index - 1] = new Array();
                        // Then add the data
                        arrayList[index - 1].push(docData);
                    }
                }
                else {
                    // If key was found before, find its position
                    index = keys.indexOf(category);
                    // Look for array in the same position
                    if (arrayList[index]) {
                        // If it exists (more likely), add data
                        arrayList[index].push(docData);
                    }
                    else {
                        // If it doesn't exist (unlikely), add new array
                        arrayList[index] = new Array()
                        // Then add data
                        arrayList[index].push(docData);
                    }
                }

                // Add to featured document section if it is one
                if (n.Featured){
                    if (ftDocsAdded < 3) {
                        featuredDoc = 
                        '<div class=\"col-sm-4\" >' +
                            '<div class=\"featured-document-box\" >' +
                                '<img class=\"img-responsive\" src="' + imageUrl + '">' +
                            '</div>' +
                            '<div class=\"featured-document-footer \">';
                        if (IsOfficeDocument(extension)) {
                            featuredDoc += '<a href=\"' + owaUrl + '\"><h3>' + docName + '</h3></a>';
                        }
                        else if (IsVideoFile(extension)) {
                            featuredDoc += '<a href=\"' + videoUrl + '\"><h3>' + docName + '</h3></a>';
                        }
                        else {
                            featuredDoc += '<a href=\"' + docUrl + '\"><h3>' + docName + '</h3></a>';
                        }
                        
                        featuredDoc +=
                                '<img src=\"' + iconUrl + '\" class=\"featured-icon\">' +
                            '</div>' +
                        '</div>';

                        jQuery(featuredDoc).appendTo(".featured-documents");
                        ftDocsAdded++;
                    }
                }
            });

            var docInfo;
            var featuredTag;
            for (i = 0; i < keys.length; i++) {
                // Generate HTML for a new category section
                if (keys.length != 1) {
                    html += '<h3>' + keys[i] + '</h3>';
                }
                html +=
                    '<div class="rackhouse-table">' +
                        '<table class="table docs-table">' +
                        '<thead>' +
                            '<tr>' +
                                '<th class="hidden-xs"></th>' +
                                '<th>Name</th>' +
                                '<th>Modified Date</th>' +
                                '<th class="hidden-xs">Download</th>' +
                                '<th class="hidden-xs">Favorite</th>' +
                                '<th class="hidden-xs">Share</th>' +
                            '</tr>' +
                        '</thead>' +
                        '<tbody class="library-body">';

                
                for (j = 0; j < arrayList[i].length; j++) {
                    docInfo = arrayList[i][j].split('|');
                    extension = docInfo[1].substr(docName.lastIndexOf('.') + 1);
                    if (docInfo[7] == "true") {
                        featuredTag = "<i class='fa fa-thumb-tack icon-featured'></i>";
                    }
                    else {
                        featuredTag = "";
                    }
                    html +=
                        '<tr>' +
                            '<td class="hidden-xs"><img src="' + docInfo[0] + '"></td>';
                    if (IsOfficeDocument(extension)) {
                        html +=
                            '<td><a href="'+ docInfo[5] +'">' + docInfo[1] + '</a>&nbsp;' + featuredTag + '</td>';
                    }
                    else if (IsVideoFile(extension)) {
                        html +=
                            '<td><a href="'+ docInfo[6] +'">' + docInfo[1] + '</a>&nbsp;' + featuredTag + '</td>';
                    }
                    else {
                        html +=
                            '<td><a href="'+ docInfo[2] +'">' + docInfo[1] + '</a>&nbsp;' + featuredTag + '</td>';
                    }
                    html +=
                            '<td>' + docInfo[3] + '</td>' +
                            '<td><a href=\"' + docInfo[2] + '\" class=\"download-link\" download><i class=\"fa icon-download-alt\" aria-hidden=\"true\"></i></a></td>';
                    if (GetFavoriteId(docInfo[2]) != -1) {
                        html +=
                            '<td><a href=\"#\" class=\"favorite-link\"><i class=\"fa fa-star\" aria-hidden=\"true\" onclick=\"ToggleFavorite(this, \'' + docInfo[2] + '\', \'' + docInfo[1] + '\')\"></i></a></td>';
                    }
                    else {
                        html +=
                            '<td><a href=\"#\" class=\"favorite-link\"><i class=\"fa fa-star-o\" aria-hidden=\"true\" onclick=\"ToggleFavorite(this, \'' + docInfo[2] + '\', \'' + docInfo[1] + '\')\"></i></a></td>';
                    }
                    html +=
                            '<td>' +
                            '<a class=\"share-link hidden-xs hidden-sm\" href=\"mailto:?subject=' + docInfo[1] + '&body=' + PrepURLs(docInfo[4]) + '\"><i class=\"fa icon-envelope\" aria-hidden=\"true\"></i></a>' +
                            '<a class=\"share-link hidden-md hidden-lg\" href=\"mailto:?subject=' + docInfo[1] + '&body=' + docInfo[8] + '\"><i class=\"fa icon-envelope\" aria-hidden=\"true\"></i></a>' +
                            '</td>' +
                        '</tr>';
                }

                html +=
                    '</tbody>' +
                    '</table>' +
                '</div>';
            }

            // Set accordion HTML and activate it
            jQuery("#accordion").html(html);
            if (keys.length > 1) {
                jQuery("#accordion").accordion({ heightStyle: "content" });
            }

            jQuery(".docs-table").DataTable({
                "paging": false,
                "searching": false,
                "info": false,
                "ordering": true,
                "order": [[2, "desc"]],
                "columnDefs": [
                    { "orderable": false, "targets": 0 },
                    { "orderable": false, "targets": 3 },
                    { "orderable": false, "targets": 4 },
                    { "orderable": false, "targets": 5 }
                ]
            });
        },
        error:function(d) {
            console.log(JSON.stringify(d));
        }
    });
}

var rkhs_favoriteItems;

jQuery(document).ready(function() {
    GetFavoritesForUser();
    PrintLibraryItems();
});
    