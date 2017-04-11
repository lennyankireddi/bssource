
// Global Variables
var pa_formMode = "NEW";
var pa_savedData;
var pa_publishedData;
var pa_programId = "";
var pa_programScopes = {};
var pa_programScopesById = {};


// Generate new program ID
function GeneratePeopleId() {
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
function SetPeopleId() {
    var peopleid = getParameterByName("peopleid");
    if (peopleid) {
        jQuery("#peopleIdField span").text(peopleid);
        pa_peopleId = peopleid;
        console.log(peopleid);
    }
    else {
        pa_peopleId = peopleid;
        jQuery("#peopleIdField span").text(GeneratePeopleId());
        console.log(peopleid);
    }
}


function LoadFormData() {
    jQuery.ajax({
        url: "/_api/web/lists/getbytitle('People')/items?$select=PeopleID,Name,Brand,Role,Description,Picture,&$filter=ProgramID eq '" + pa_peopleId + "'",
        type: "GET",
        headers: {
            "accept": "application/json;odata=verbose"
        },
        success: function(data) {
            var person = data.d.results[0];
            pa_savedData = person;
            jQuery("#person-name").val(person.Name);
            jQuery("#person-role").val(person.Role);
            jQuery("#textarea").val(person.Description);
        },
        error: function(err) {
            console.log(JSON.stringify(err));
        }
    });


}   

function GetFormData(listName, action, pageAction) {
    var data = {
        "__metadata": {
            "type": "SP.Data.PeopleListItem"
        },
        "Name": "",
        "Role": "",
        "Brand": "",
        "Description": "",
        "Picture": {
            "__metadata": {
                "type": "SP.FieldUrlValue"
            },
            "Description": "",
            "Url": ""
        }
    }

    //var photoUrl = jQuery("#programPhotoImage").attr("src").replace(_spPageContextInfo.siteAbsoluteUrl, "");
    //var logoUrl = jQuery("#programLogoImage").attr("src").replace(_spPageContextInfo.siteAbsoluteUrl, "");

    if (action == "EDIT") {
        if (pageAction == "SAVE") {
            if (pa_publishedData) {
                data.__metadata.etag = pa_publishedData.__metadata.etag;
            }
        }
        else if (pageAction == "SAVE") {
            data.__metadata.etag = pa_savedData.__metadata.etag;
        }
    }
    data.__metadata.type = "SP.Data." + listName.replace(" ", "_x0020_") + "ListItem";
    data.Name = jQuery("#person-name").val();
    data.Role = jQuery("#person-role").val();
    //data.Brand = jQuery("#person-brand").val();
    data.Description = jQuery("#textarea").val();
    //data.Picture = personPicture;


    return data;
}



//Save Function for New Person
function AddPeople(listName, pageAction) {
    jQuery.ajax({
        url: "/_api/web/lists/GetByTitle('People')/items",
        method: "POST",
        data: JSON.stringify(GetFormData(listName, "NEW", pageAction)),
        headers: {
            "accept": "application/json;odata=verbose",
            "X-RequestDigest": jQuery("#__REQUESTDIGEST").val(),
            "content-type": "application/json;odata=verbose"
        },
        success: function() {
            SetStatus("SUCCESS", "The person has been added.");
        },
        error: function(err) {
            SetStatus("ERROR", "An error occurred while saving the program. Please check the inputs and retry.");
            console.log(JSON.stringify(err));
        }
    });
}


//Edit function for an exisitng Person
function EditPeople(listName, pageAction) {
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
            SetStatus("SUCCESS", "The program has been saved. There are unpublished changes on this program.");  
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
            if (jQuery(".page-status").hasClass("text-warning")) {
                jQuery(".page-status").removeClass("text-warning");
            }
            if (!jQuery(".page-status").hasClass("text-success")) {
                jQuery(".page-status").addClass("text-success");
            }
            break;
        case "ERROR":
            if (jQuery("#pageStatusIcon").hasClass("fa-check")) {
                jQuery("#pageStatusIcon").removeClass("fa-check");
            }
            if (!jQuery("#pageStatusIcon").hasClass("fa-remove")) {
                jQuery("#pageStatusIcon").addClass("fa-remove");
            }
            jQuery("#pageStatusIcon").show();
            if (jQuery(".page-status").hasClass("text-success")) {
                jQuery(".page-status").removeClass("text-success");
            }
            if (!jQuery(".page-status").hasClass("text-warning")) {
                jQuery(".page-status").addClass("text-warning");
            }
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
    if (jQuery("#person-name").val() == "") {
        SetStatus("ERROR", "Person name cannot be empty.");
        return false;
    }

    // Validate that person Role is not blank
    if (jQuery("#person-role").val() == "") {
        SetStatus("ERROR", "Person cannot be empty.");
        return false;
    }
    return true;
}



function SaveFormData(pageAction) {
    if (ValidateInputs()) {
        if (pageAction == "SAVE") {
            if (pa_formMode == "NEW") {
                AddPeople("Programs Saved", "SAVE");
            }
            else if (pa_formMode == "EDIT") {
                EditPeople("Programs Saved", "SAVE");
            }
        }
    }
}


// Add event AddHandlers
function AddHandlers() {

  // Handler to save form data
    jQuery("#btnSave").on("click", function() {
        SaveFormData("SAVE");
    });

}






jQuery(document).ready(function(){

  SetPeopleId();
  AddHandlers();

})
