function PrintLibraryItems(element){

    jQuery.ajax({
        url:"/_api/web/lists/GetByTitle('people')/items",
        type:"GET",
        headers:{Accept:"application/json;odata=verbose"},
        success:function(data)
        {
            console.log('success');

            var peopleName = "";
            var peopleRole = "";
            var peopleBrand = "";
            var peopleDescription = "";
            var peoplePhoto = "";

            jQuery.each(data.d.results, function(i, n) {

                peopleName = n.Name; 
                peopleRole = n.Role; 
                peopleBrand = n.Brand; 
                peopleDescription = n.Description; 
                peoplePhoto = n.Picture.Url; 
                var nameID = peopleName.replace(/\s/g, '');

                console.log(peopleName);


                var primaryHTML = "<li class=\"people-grid-person ui-state-default\"> <a href=\"#\" data-toggle=\"modal\" data-target=\"#" + nameID + "\">" +
                    "<img src=\"" + peoplePhoto + "\" class=\"img-responsive\"/>" +
                    "<div class=\"people-grid-overlay\">" +
                        "<h2 class=\"people-name\">" +
                            peopleName +
                        "</h2>" +
                        "<span class=\"people-description\">"+
                          peopleRole +
                        "</span>" +
                    "</div>" +
                    "<div class=\"person-move\">" +
                        "<i class=\"icon-move\"></i>" +
                    "</div>" +
                  "</a></li>";


              var modalHTML =  "<div class=\"modal fade people-modal\" id=\"" + nameID +"\" tabindex=\"-1\" role=\"dialog\" aria-labelledby=\"myModalLabel\">" +
"<div class=\"modal-dialog modal-lg\" role=\"document\">" +
   " <div class=\"modal-content\">" +
     " <div class=\"modal-header\">" +
        "<button type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-label=\"Close\"><span aria-hidden=\"true\">&times;</span></button>" +
     "</div>" +
     "<div class=\"modal-body\">" +
        "<div class=\"row\">" +
            "<div class=\"col-sm-3\">" +
                "<img src=\"" + peoplePhoto+ "\"> " +
            "</div>" +
            "<div class=\"col-sm-5\" id=\"people-modal-info\">" +
               "<h2 class=\"modal-title\" id=\"myModalLabel\">" +
                peopleName +
               "</h2>" +
               "<h3" +
                    peopleRole +
               "</h3>" +
               "<h3>" +
                    peopleBrand +
               "</h3>" +
                    "<p>" +
                        peopleDescription +
                    "</p>" +
                    "</div>" +
                    "<div class=\"col-sm-4\">" +
                        "<button type=\"button\" class=\"btn btn-primary btn-lg\" data-toggle=\"modal\" data-target=\"#myModal\">" +
                     "Request Market Visit" +
                    "</button>" +
                    "<div class=\"people-twitter\"><i class=\"icon-twitter\"></i> <span>@twitterhandle</span></div>" +
                    "<div class=\"media\">" +
                    "</div>" +
            "</div>"
       "</div>"+
     "</div>"+
    "</div>"+
 "</div>"+
"</div>";


 ;


                $("ul.people-grid-list").append(primaryHTML);
                $(".people-grid").append(modalHTML);


            });

        },
        error:function(d) {
            console.log(JSON.stringify(d));
        }
    });
}

jQuery( function() {
      jQuery( ".people-grid-list" ).sortable();
      jQuery( ".people-grid-list" ).disableSelection();
  } );


jQuery(document).ready(function(){
  PrintLibraryItems();
})



