/* Environment specific variables - CHANGE BEFORE DEPLOYING */
pa_termStoreName = "Managed Metadata Service Application";
pa_scopeTermSetGuid = "25f982c6-18bc-4d08-b107-82582b3df7ac";

// Global Variables
var pa_formMode = "NEW";
var pa_savedData;
var pa_publishedData;
var pa_programId = "";
var pa_programScopes = {};
var pa_programScopesById = {};
var pa_language = "";
var pa_scope = "";
var pa_programPhotoUrl = "";
var pa_programLogoUrl = "";

// Get language for labels
function GetLanguageForLabel(label) {
    switch (label.toLowerCase()) {
        case "connect":
            return "English";
        case "zhhk":
            return "Chinese (Hong Kong S.A.R.)";
        case "zhcn":
            return "Chinese (People's Republic of China)";
        case "fr":
            return "French (France)";
        case "de":
            return "German (Germany)";
        case "ja":
            return "Japanese (Japan)";
        case "pl":
            return "Polish (Poland)";
        case "pt":
            return "Portuguese (Brazil)";
        case "ru":
            return "Russian (Russia)";
        case "es":
            return "Spanish (Spain)";
        default:
            return "";
    }
}

// Generate new program ID
function GenerateProgramId() {
    var date = new Date();
    var month = date.getMonth() + 1;
    month = month.toString().length == 1 ? "0" + month.toString() : month.toString();
    var day = date.getDate();
    day = day.toString().length == 1 ? "0" + day.toString() : day.toString();
    var year = date.getFullYear();
    var hours = date.getHours();
    hours = hours.toString().length == 1 ? "0" + hours.toString() : hours.toString();
    var minutes = date.getMinutes();
    minutes = minutes.toString().length == 1 ? "0" + minutes.toString() : minutes.toString();
    var seconds = date.getSeconds();
    seconds = seconds.toString().length == 1 ? "0" + seconds.toString() : seconds.toString();
    return (month + day + year + hours + minutes + seconds);
}

function getParameterByName(name, url) {
    if (!url) {
        url = window.location.href;
    }
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

// Set the program ID hidden field
function SetProgramId() {
    var progid = getParameterByName("progid");
    if (progid) {
        jQuery("#programIdField").val(progid);
        pa_programId = progid;
        pa_formMode = "EDIT";
    }
    else {
        pa_programId = GenerateProgramId();
        jQuery("#programIdField").val(pa_programId);
    }
}

// Construct the language selection HTML for each language
function LoadLanguages(languages) {
    var langlist = "";
    for (i = 0; i < languages.length; i++) {
        langlist +=
            '<a value="' + languages[i] + '" class="dropdown-program-link" href="javascript:{}">' + languages[i] + '</a>';
    }
    jQuery("#ddLanguage").html(langlist);
    
}

function LoadImages(sourceUri, container, image) {
    return jQuery.ajax({
        url: sourceUri,
        type: "GET",
        headers: { "accept": "application/json;odata=verbose" },
        success: function(data) {
            var html = "";
            html += "<ul class='image-listing'";
            for (i = 0; i < data.d.results.length; i++) {
                html +=
                    "<li class='image-tile'>" +
                        "<img class='tile-picture " + image + "-picture' src='" + data.d.results[i].ServerRelativeUrl + "' onclick='pa_programPhotoUrl = jQuery(this).attr(\"src\"); jQuery(\"#" + image + "ImageUrl\").val(jQuery(this).attr(\"src\"))'>" +
                        "<div class='tile-caption'>" + data.d.results[i].Name + "</div>" +
                    "</li>";
            }
            html += "</ul>";
            container.html(html);
        },
        error: function(error) {
            console.log(JSON.stringify(error));
        }
    });
}

// Load the image picker dialogs with images availabe in assets folders
function LoadImagePickers() {
    photoSource = "/SiteAssets/Program_Photos";
    logoSource = "/SiteAssets/Program_Logos";

    var photoUri = _spPageContextInfo.siteAbsoluteUrl + "/_api/web/getfolderbyserverrelativeurl('" + photoSource + "')/files";
    LoadImages(photoUri, jQuery("#photoImageList"), "photo");

    var logoUri = _spPageContextInfo.siteAbsoluteUrl + "/_api/web/getfolderbyserverrelativeurl('" + logoSource + "')/files";
    LoadImages(logoUri, jQuery("#logoImageList"), "logo");
}

// Add allowed values to the Languages drop down
function SetAvailableLanguages() {
    var languages = [];
    var fallbacks = ["English", "Chinese (Hong Kong S.A.R.)", "Chinese (People's Republic of China)", "French (France)", "German (Germany)", "Japanese (Japan)", "Polish (Poland)", "Portuguese (Brazil)", "Russian (Russia)", "Spanish (Spain)"];
    url = "/_api/web/lists/getbytitle('Variation Labels')/items?$select=Title";
    // Get available variation labels
    jQuery.ajax({
        url: url,
        method: "GET",
        headers: { 
            "Accept": "application/json;odata=verbose"
        },
        success: function(data) {
            var result;
            for (i = 0; i < data.d.results.length; i++) {
                result = data.d.results[i];
                languages.push(GetLanguageForLabel(result.Title));
            }
            LoadLanguages(languages.sort());
        },
        error: function(err) {
            LoadLanguages(fallbacks);
            console.log(JSON.stringify(err))
        }
    });
}

function LoadScopeTerms() {
    var context = SP.ClientContext.get_current();
    var taxSession = SP.Taxonomy.TaxonomySession.getTaxonomySession(context);
    var termStores = taxSession.get_termStores();

    // Get the Connect site collection term store
    var termStore = termStores.getByName(pa_termStoreName);
    // Retrieve the "Program Scope" term set
    var termSet = termStore.getTermSet(pa_scopeTermSetGuid);
    // Get all the terms
    var terms = termSet.getAllTerms();
    context.load(terms);
    context.executeQueryAsync(function() {
        // Enumerate through the terms and add
        var termEnumerator = terms.getEnumerator();
        var currentTerm;
        var name;
        var label;
        var guid;
        var scopeChoice = "";
        while(termEnumerator.moveNext()) {
            currentTerm = termEnumerator.get_current();
            // Add the term name and ID to a hash for easy access later
            pa_programScopes[currentTerm.get_name()] = currentTerm.get_id();
            pa_programScopesById[currentTerm.get_id()] = currentTerm.get_name();
            // Construct a choice html for the scope
            scopeChoice =
                '<a value="' + currentTerm.get_name() + '" class="dropdown-program-link" href="javascript:{}">' + currentTerm.get_name() + '</a>';
            jQuery("#ddScope").append(scopeChoice);
        } 
        if (pa_formMode == "EDIT") {
            LoadFormData(pa_programId);
        }
    }, function(sender,args) {                  // If the call fails
          console.log(args.get_message());
    });
}

// Upload file procedure
function UploadFile(fileInput, img, serverRelativeUrlToFolder) {

    // Initiate method calls using jQuery promises.
    // Get the local file as an array buffer.
    var getFile = getFileBuffer();
    getFile.done(function (arrayBuffer) {

        // Add the file to the SharePoint folder.
        var addFile = addFileToFolder(arrayBuffer);
        addFile.done(function (file, status, xhr) {
            var getItem = getListItem(file.d.ListItemAllFields.__deferred.uri);
            getItem.done(function (listItem, status, xhr) {
                img.attr("src", listItem.d.ServerRelativeUrl);
                fileInput.val("");
            });
            getItem.fail(UploadError);
        });
        addFile.fail(UploadError);
    });
    getFile.fail(UploadError);

    // Get the local file as an array buffer.
    function getFileBuffer() {
        var deferred = jQuery.Deferred();
        var reader = new FileReader();
        reader.onloadend = function (e) {
            deferred.resolve(e.target.result);
        }
        reader.onerror = function (e) {
            deferred.reject(e.target.error);
        }
        reader.readAsArrayBuffer(fileInput[0].files[0]);
        return deferred.promise();
    }

    // Add the file to the file collection in the Shared Documents folder.
    function addFileToFolder(arrayBuffer) {

        // Get the file name from the file input control on the page.
        var parts = fileInput[0].value.split('\\');
        var fileName = parts[parts.length - 1];

        // Construct the endpoint.
        var fileCollectionEndpoint = String.format(
            "{0}/_api/web/getfolderbyserverrelativeurl('{1}')/files" +
            "/add(overwrite=true, url='{2}')",
            _spPageContextInfo.siteAbsoluteUrl, serverRelativeUrlToFolder, fileName);

        // Send the request and return the response.
        // This call returns the SharePoint file.
        return jQuery.ajax({
            url: fileCollectionEndpoint,
            type: "POST",
            data: arrayBuffer,
            processData: false,
            headers: {
                "accept": "application/json;odata=verbose",
                "X-RequestDigest": jQuery("#__REQUESTDIGEST").val()
            }
        });
    }

    // Get the list item that corresponds to the file by calling the file's ListItemAllFields property.
    function getListItem(fileListItemUri) {
        // Send the request and return the response.
        fileListItemUri = fileListItemUri.replace("/ListItemAllFields","");
        return jQuery.ajax({
            url: fileListItemUri,
            type: "GET",
            headers: { "accept": "application/json;odata=verbose" }
        });
    }
}

function UploadError(error) {
    console.log(error.responseText);
}

function GetShortDate(dateString) {
    var date = new Date(dateString);
    var month = date.getMonth() + 1;
    month = month.toString().length == 1 ? "0" + month.toString() : month.toString();
    var day = date.getDate();
    day = day.toString().length == 1 ? "0" + day.toString() : day.toString();
    var year = date.getFullYear();
    return (month + "/" + day + "/" + year);
}

function GetZuluDate(dateString) {
    var date = new Date(dateString);
    var month = date.getMonth() + 1;
    month = month.toString().length == 1 ? "0" + month.toString() : month.toString();
    var day = date.getDate();
    day = day.toString().length == 1 ? "0" + day.toString() : day.toString();
    var year = date.getFullYear();
    return (year + "-" + month + "-" + day + "T00:00:00Z");
}

function LoadFormData() {
    var programsUri = "/_api/web/lists/getbytitle('Programs')/items?$select=ProgramID,Title,Language,Program_x0020_Scope,Run_x0020_Time,StartDate,OData__EndDate,BeamConnectDescription,Program_x0020_Photo,Program_x0020_Logo&$filter=ProgramID eq '" + pa_programId + "'";
    jQuery.ajax({
        url: programsUri,
        type: "GET",
        headers: {
            "accept": "application/json;odata=verbose"
        },
        success: function(data) {
            pa_publishedData = data.d.results[0];
        },
        error: function(err) {
            console.log(JSON.stringify(err));
        }
    });
    
    var savedProgramsUri = "/_api/web/lists/getbytitle('Programs Saved')/items?$select=ProgramID,Title,Language,Program_x0020_Scope,Run_x0020_Time,StartDate,OData__EndDate,BeamConnectDescription,Program_x0020_Photo,Program_x0020_Logo&$filter=ProgramID eq '" + pa_programId + "'";
    jQuery.ajax({
        url: savedProgramsUri,
        type: "GET",
        headers: {
            "accept": "application/json;odata=verbose"
        },
        success: function(data) {
            var prog = data.d.results[0];
            pa_savedData = prog;
            jQuery("#program-name").val(prog.Title);
            jQuery("#ddbtnLanguage").find("span").text(prog.Language);
            jQuery("#ddbtnScope").find("span").text(pa_programScopesById[prog.Program_x0020_Scope.TermGuid]);
            if (prog.Run_x0020_Time == "Fixed") {
                jQuery("#radio-fixed").attr("checked", true);
                jQuery("#radio-open").attr("checked", false);
            }
            else {
                jQuery("#radio-fixed").attr("checked", false);
                jQuery("#radio-open").attr("checked", true);
            }
            jQuery("#datepicker-start").val(GetShortDate(prog.StartDate));
            jQuery("#datepicker-end").val(GetShortDate(prog.OData__EndDate));
            jQuery("#textarea").val(prog.BeamConnectDescription);
            jQuery("#programPhotoImage").attr("src", prog.Program_x0020_Photo.Url);
            jQuery("#programLogoImage").attr("src", prog.Program_x0020_Logo.Url);
            pa_programPhotoUrl = prog.Program_x0020_Photo.Url;
            pa_programLogoUrl = prog.Program_x0020_Logo.Url;
        },
        error: function(err) {
            console.log(JSON.stringify(err));
        }
    });
}

function GetFormData(listName, action, pageAction) {
    var data = {
        "__metadata": {
            "type": "SP.Data.ProgramsListItem"
        },
        "Title": "",
        "Language": "English",
        "StartDate": "",
        "OData__EndDate": "",
        "Program_x0020_Logo": {
            "__metadata": {
                "type": "SP.FieldUrlValue"
            },
            "Description": "/SiteAssets/Program_Logos/bourbonlegends.png",
            "Url": "/SiteAssets/Program_Logos/bourbonlegends.png"
        },
        "Program_x0020_Photo": {
            "__metadata": {
                "type": "SP.FieldUrlValue"
            },
            "Description": "/SiteAssets/Program_Photos/bottles.png",
            "Url": "/SiteAssets/Program_Photos/bottles.png"
        },
        "Program_x0020_Scope": {
            "__metadata": {
                "type": "SP.Taxonomy.TaxonomyFieldValue"
            },
            "Label": "2454",
            "TermGuid": "87c2063b-5792-4e52-9282-81309a4e8a13",
            "WssId": 2454
        },
        "Run_x0020_Time": "Fixed",
        "ProgramID": "",
        "BeamConnectDescription": ""
    }

    var photoUrl = jQuery("#programPhotoImage").attr("src").replace(_spPageContextInfo.siteAbsoluteUrl, "");
    var logoUrl = jQuery("#programLogoImage").attr("src").replace(_spPageContextInfo.siteAbsoluteUrl, "");

    if (action == "EDIT") {
        if (pageAction == "PUBLISH") {
            data.__metadata.etag = pa_publishedData.__metadata.etag;
        }
        else if (pageAction == "SAVE") {
            data.__metadata.etag = pa_savedData.__metadata.etag;
        }
    }
    data.__metadata.type = "SP.Data." + listName.replace(" ", "_x0020_") + "ListItem";
    data.ProgramID = pa_programId;
    data.Title = jQuery("#program-name").val();
    data.Language = jQuery("#ddbtnLanguage").find("span").text();
    if (jQuery("#ddbtnScope").find("span").text() == "Local") {
        data.Program_x0020_Scope.Label = "2454";
        data.Program_x0020_Scope.WssId = 2454;
    }
    else {
        data.Program_x0020_Scope.Label = "2453";
        data.Program_x0020_Scope.WssId = 2453;
    }
    data.Program_x0020_Scope.TermGuid = pa_programScopes[jQuery("#ddbtnScope").find("span").text()].toString();
    data.Run_x0020_Time = jQuery("input:radio[name=runtime]:checked").val();
    data.StartDate = GetZuluDate(jQuery("#datepicker-start").val());
    data.OData__EndDate = GetZuluDate(jQuery("#datepicker-end").val());
    data.BeamConnectDescription = jQuery("#textarea").val();
    data.Program_x0020_Photo.Description = photoUrl;
    data.Program_x0020_Photo.Url = photoUrl;
    data.Program_x0020_Logo.Description = logoUrl;
    data.Program_x0020_Logo.Url = logoUrl;

    return data;
}

function AddProgram(listName, pageAction) {
    var url = "/_api/web/lists/getbytitle('" + listName + "')/items";
    jQuery.ajax({
        url: url,
        method: "POST",
        data: JSON.stringify(GetFormData(listName, "NEW", pageAction)),
        headers: {
            "accept": "application/json;odata=verbose",
            "X-RequestDigest": jQuery("#__REQUESTDIGEST").val(),
            "content-type": "application/json;odata=verbose"
        },
        success: function() {
            switch (pageAction) {
                case "SAVE":
                    SetStatus("SUCCESS", "The program has been saved.");
                    break;
                case "PUBLISH":
                    SetStatus("SUCCESS", "The program has been published.");
                    break;
                default:
                    break;
            }
            
        },
        error: function(err) {
            SetStatus("ERROR", "An error occurred while saving the program. Please check the inputs and retry.");
            console.log(JSON.stringify(err));
        }
    });
}

function EditProgram(listName, pageAction) {
    var url;
    var match;
    if (pageAction == "PUBLISH") {
        url = pa_publishedData.__metadata.uri;
        match = pa_publishedData.__metadata.etag;
    }
    else {
        url = pa_savedData.__metadata.uri;
        match = pa_savedData.__metadata.etag;
    }
    jQuery.ajax({
        url: url,
        method: "POST",
        data: JSON.stringify(GetFormData(listName, "EDIT", pageAction)),
        headers: {
            "accept": "application/json;odata=verbose",
            "X-RequestDigest": jQuery("#__REQUESTDIGEST").val(),
            "content-type": "application/json;odata=verbose",
            "X-HTTP-Method": "MERGE",
            "If-Match": match
        },
        success: function() {
            switch (pageAction) {
                case "SAVE":
                    SetStatus("SUCCESS", "The program has been saved. There are unpublished changes on this program.");
                    break;
                case "PUBLISH":
                    SetStatus("SUCCESS", "The program has been published.");
                    break;
                default:
                    break;
            }
            
        },
        error: function(err) {
            SetStatus("ERROR", "An error occurred while saving the program. Please check the inputs and retry.");
            console.log(JSON.stringify(err));
        }
    });
}

function SetStatus(type, message) {
    switch(type) {
        case "SUCCESS":
            if (jQuery("#pageStatusIcon").hasClass("fa-remove")) {
                jQuery("#pageStatusIcon").removeClass("fa-remove");
            }
            if (!jQuery("#pageStatusIcon").hasClass("fa-check")) {
                jQuery("#pageStatusIcon").addClass("fa-check");
            }
            jQuery("#pageStatusIcon").show();
            break;
        case "ERROR":
            if (jQuery("#pageStatusIcon").hasClass("fa-check")) {
                jQuery("#pageStatusIcon").removeClass("fa-check");
            }
            if (!jQuery("#pageStatusIcon").hasClass("fa-remove")) {
                jQuery("#pageStatusIcon").addClass("fa-remove");
            }
            jQuery("#pageStatusIcon").show();
            break;
        default:
            jQuery("#pageStatusIcon").hide();
    }

    jQuery("#pageStatusText").text(message);
    jQuery(".page-status").show();
}

function ClearStatus() {
    jQuery(".page-status").hide();
}

function ValidateInputs() {
    // Validate that program name is not blank
    if (jQuery("#program-name").val() == "") {
        SetStatus("ERROR", "Program name cannot be empty.");
        return false;
    }
    
    // Validate that a language has been chosen
    if (jQuery("#ddbtnLanguage").find("span").text() == "Select Language") {
        SetStatus("ERROR", "Please select a language for the program.");
        return false;
    }

    // Validate that a scope has been picked
    if (jQuery("#ddbtnScope").find("span").text() == "Select Scope") {
        SetStatus("ERROR", "Please select the scope of the program.");
        return false;
    }

    // Validate dates
    switch(jQuery("input:radio[name=runtime]:checked").val()) {
        case "Fixed":
            if (jQuery("#datepicker-start").val() == "" || jQuery("#datepicker-end").val() == "") {
                SetStatus("ERROR", "Start and end date must be specified for a fixed run time program.");
                return false;
            }
            var startDate = new Date(jQuery("#datepicker-start").val());
            var endDate = new Date(jQuery("#datepicker-end").val());
            if (startDate > endDate) {
                SetStatus("ERROR", "Program end date precedes start date.");
                return false;
            }
            break;
        case "Open":
            if (jQuery("#datepicker-start").val() == "") {
                SetStatus("ERROR", "Start date must be specified.");
                return false;
            }
            break;
    }

    // Check if a program photo was selected
    if (jQuery("#programPhotoImage").attr("src") == "/_catalogs/masterpage/Resources/img-upload.png") {
        SetStatus("ERROR", "A program photo must be chosen.");
        return false;
    }

    // Check if a program logo was selected
    if (jQuery("#programLogoImage").attr("src") == "/_catalogs/masterpage/Resources/img-upload.png") {
        SetStatus("ERROR", "A program logo must be chosen.");
        return false;
    }
    return true;
}

function SaveFormData(pageAction) {
    if (ValidateInputs()) {
        if (pageAction == "SAVE") {
            if (pa_formMode == "NEW") {
                AddProgram("Programs Saved", "SAVE");
            }
            else if (pa_formMode == "EDIT") {
                EditProgram("Programs Saved", "SAVE");
            }
        }
        else if (pageAction == "PUBLISH") {
            if (pa_formMode == "NEW") {
                AddProgram("Programs Saved", "");
                AddPrograms("Programs", "PUBLISH");
            }
            else if (pa_formMode == "EDIT") {
                EditProgram("Programs Saved", "");
                EditProgram("Programs", "PUBLISH");
            }
        }
    }
}

// Add event AddHandlers
function AddHandlers() {
    // Set language
    jQuery("#ddLanguage").click(function(e) {
        var choice = jQuery(e.target);
        jQuery(this).parent().find("span").text(choice.text());
        jQuery(this).find("a").removeClass("active-filter");
        choice.addClass("active-filter");
        pa_language = choice.text();
    });

    // Set scope
    jQuery("#ddScope").click(function(e) {
        var choice = jQuery(e.target);
        jQuery(this).parent().find("span").text(choice.text());
        jQuery(this).find("a").removeClass("active-filter");
        choice.addClass("active-filter");
        pa_scope = choice.text();
    });

    // Handler to upload program photo
    jQuery("#uploadProgramPhoto").on("click", function() {
        if (jQuery("#programPhotoFile").val() != "") {
             UploadFile(jQuery("#programPhotoFile"), jQuery("#programPhotoImage"), "/SiteAssets/Program_Photos");
        }
    });

    // Handler to upload program logo
    jQuery("#uploadProgramLogo").on("click", function() {
        if (jQuery("#programLogoFile").val() != "") {
             UploadFile(jQuery("#programLogoFile"), jQuery("#programLogoImage"), "SiteAssets/Program_Logos");
        }
    });

    // Handler to apply existing photo
    jQuery("#btnApplyPhoto").on("click", function() {
        jQuery("#programPhotoImage").attr("src", jQuery("#photoImageUrl").val());
    });

    // Handler to apply existing photo
    jQuery("#btnApplyLogo").on("click", function() {
        jQuery("#programLogoImage").attr("src", jQuery("#logoImageUrl").val());
    });

    // Handler to save form data
    jQuery("#btnSave").on("click", function() {
        SaveFormData("SAVE");
    });

    // Handler to publish form data
    jQuery("#btnPublish").on("click", function() {
        SaveFormData("PUBLISH");
    });

    // Go back to home page on cancel
    jQuery("#btnCancel").on("click", function(){
        window.location.href = "/";
    });
}

jQuery(document).ready(function() {
    SetProgramId();
    // Load sp.runtime.js, sp.js and sp.taxonomy.js for working with term store in jsom
    var scriptbase = _spPageContextInfo.webServerRelativeUrl + "_layouts/15/";
    jQuery.getScript(scriptbase + "SP.Runtime.js",
        function () {
            jQuery.getScript(scriptbase + "SP.js", function(){
                jQuery.getScript(scriptbase + "SP.Taxonomy.js", LoadScopeTerms);
            });
        }
    );
    LoadImagePickers();
    SetAvailableLanguages();
    AddHandlers();
});