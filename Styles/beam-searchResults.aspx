<%-- SPG:

This HTML file has been associated with a SharePoint Page Layout (.aspx file) carrying the same name.  While the files remain associated, you will not be allowed to edit the .aspx file, and any rename, move, or deletion operations will be reciprocated.

To build the page layout directly from this HTML file, simply fill in the contents of content placeholders.  Use the Snippet Generator at http://connect.beamsuntory.com/search/_layouts/15/ComponentHome.aspx?Url=http%3A%2F%2Fconnect%2Ebeamsuntory%2Ecom%2Fsearch%2F%5Fcatalogs%2Fmasterpage%2Fbeam%2DsearchResults%2Easpx to create and customize additional content placeholders and other useful SharePoint entities, then copy and paste them as HTML snippets into your HTML code.   All updates to this file within content placeholders will automatically sync to the associated page layout.

 --%>
<%@Page language="C#" Inherits="Microsoft.SharePoint.Publishing.PublishingLayoutPage, Microsoft.SharePoint.Publishing, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<%@Register TagPrefix="PageFieldFieldValue" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<%@Register TagPrefix="Publishing" Namespace="Microsoft.SharePoint.Publishing.WebControls" Assembly="Microsoft.SharePoint.Publishing, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<%@Register TagPrefix="SearchWC" Namespace="Microsoft.Office.Server.Search.WebControls" Assembly="Microsoft.Office.Server.Search, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<%@Register TagPrefix="PageFieldTextField" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<%@Register TagPrefix="PageFieldRichImageField" Namespace="Microsoft.SharePoint.Publishing.WebControls" Assembly="Microsoft.SharePoint.Publishing, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<%@Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<asp:Content runat="server" ContentPlaceHolderID="PlaceHolderPageTitle">
            <SharePoint:ProjectProperty Property="Title" runat="server" />
            
            
            <PageFieldFieldValue:FieldValue FieldName="fa564e0f-0c70-4ab9-b863-0177e6ddd247" runat="server">
            </PageFieldFieldValue:FieldValue>
            
        </asp:Content><asp:Content runat="server" ContentPlaceHolderID="PlaceHolderAdditionalPageHead">
            
            
            
            <Publishing:EditModePanel runat="server" id="editmodestyles">
                <SharePoint:CssRegistration name="&lt;% $SPUrl:~sitecollection/Style Library/~language/Themable/Core Styles/editmode15.css %&gt;" After="&lt;% $SPUrl:~sitecollection/Style Library/~language/Themable/Core Styles/pagelayouts15.css %&gt;" runat="server">
                </SharePoint:CssRegistration>
            </Publishing:EditModePanel>
            
            <link href="http://azjbbfpspdw1.cloudapp.net/search/_catalogs/masterpage/Resources/search-styles.css" rel="stylesheet" type="text/css" />
        </asp:Content><asp:Content runat="server" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea">
            
            
            <PageFieldFieldValue:FieldValue FieldName="fa564e0f-0c70-4ab9-b863-0177e6ddd247" runat="server">
            </PageFieldFieldValue:FieldValue>
            
        </asp:Content><asp:Content runat="server" ContentPlaceHolderID="PlaceHolderMain">
            <div class="connectHeader ms-dialogHidden connectBrand">
                <h1>
                    
                    
                    
                    <PageFieldTextField:TextField FieldName="fa564e0f-0c70-4ab9-b863-0177e6ddd247" runat="server">
                    </PageFieldTextField:TextField>
                    
                </h1>
                <h2>
                    
                    
                    <PageFieldTextField:TextField FieldName="d3429cc9-adc4-439b-84a8-5679070f84cb" runat="server">
                        
                    </PageFieldTextField:TextField>
                    
                </h2>
                <div class="pageImage">
                    <div data-name="EditModePanelShowInEdit">
                        
                        
                        <Publishing:EditModePanel runat="server" CssClass="edit-mode-panel">
                            
                            <div class="DefaultContentBlock">
                                <style>//<![CDATA[
							.pageImage {display:block;}
							div #sideNavBox {
							display:none !important;
							}

							#contentBox {
							margin-left:0px !important;
							display:inline-block;
							vertical-align:top;
							}

							div#contentRow {
							height:auto !important;
							}

							#footer {
							clear:both;
							}

							.ms-breadcrumb-top {
							background-color:none;
							}

							div.centered-content#contentBox > #DeltaPlaceHolderMain {
							width:auto !important;
							}

						
                                    
                                    
                                    
                                    
                                //]]></style>
                            </div>
                            
                        </Publishing:EditModePanel>
                        
                    </div>
                    <div data-name="EditModePanelShowInRead">
                        
                        
                        <Publishing:EditModePanel runat="server" PageDisplayMode="Display">
                            
                            <div class="DefaultContentBlock">
                                <style>//<![CDATA[
							.pageImage {display:none;}
							div #sideNavBox {
							display:none !important;
							}

							#contentBox {
							margin-left:0px !important;
							display:inline-block;
							vertical-align:top;
							}

							div#contentRow {
							height:auto !important;
							}

							#footer {
							clear:both;
							}

							.ms-breadcrumb-top {
							background-color:none;
							}

							div.centered-content#contentBox > #DeltaPlaceHolderMain {
							width:auto !important;
							}
						
                                    
                                    
                                    
                                    
                                //]]></style>
                            </div>
                            
                        </Publishing:EditModePanel>
                        
                    </div>
                    
                    
                    <PageFieldRichImageField:RichImageField FieldName="3de94b06-4120-41a5-b907-88773e493458" runat="server">
                        
                    </PageFieldRichImageField:RichImageField>
                    
                </div>
            </div>
            <div class="connectBreadcrumb ms-dialogHidden">
                <div class="ms-PlaceHolderTitleBreadcrumb">
                    <asp:sitemappath runat="server" PathSeperator=" &gt; " sitemapproviders="SPContentMapProvider,SPSiteMapProvider,SPXmlContentMapProvider" rendercurrentnodeaslink="true" hideinteriorrootnodes="false" ParentLevelsDisplayed="2">
                    <PathSeparatorStyle CssClass="seperator" />
                    <NodeStyle CssClass="node" />
                    </asp:sitemappath>
                </div>
            </div>
            <div class="search-body">
                <div class="left-column search-column">
                    <i class="fa fa-filter icon-filter">
                    </i>
                    <h2 class="left-column-header">Refiners
                    </h2>
                    <div data-name="WebPartZone">
                        
                        
                        <div>
                            <WebPartPages:WebPartZone runat="server" AllowPersonalization="false" ID="xa64f45dd0bb845e5aa3b64564e70b356" FrameType="TitleBarOnly" Orientation="Vertical">
                                <ZoneTemplate>
                                    
                                </ZoneTemplate>
                            </WebPartPages:WebPartZone>
                        </div>
                        
                    </div>
                </div>
                <div class="main-content search-column">
                    <div data-name="WebPartZone">
                        
                        
                        <div>
                            <WebPartPages:WebPartZone runat="server" AllowPersonalization="false" ID="xe286af1f254d4f42b8719f89fc4e6e60" FrameType="TitleBarOnly" Orientation="Vertical">
                                <ZoneTemplate>
                                    
                                </ZoneTemplate>
                            </WebPartPages:WebPartZone>
                        </div>
                        
                    </div>
                </div>
            </div>
        </asp:Content>