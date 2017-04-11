<%@ Assembly Name="BEAM.Connect, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f2ba3df70af251f1" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManageProfileUserControl.ascx.cs" Inherits="BEAM.Connect.ManageProfile.ManageProfileUserControl" %>

<asp:UpdatePanel ID="signupFormPnl" runat="server" EnableViewState="true" ChildrenAsTriggers="true" UpdateMode="Conditional" ViewStateMode="Enabled">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="countryDDL" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="stateDDL" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="areaDDL" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="employerDDL" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>
        <asp:Panel ID="manageProfilePnl" CssClass="form manageprofile" runat="server">
            <div class="row">
                <span class="label">First Name</span>
                <span class="field">
                    <asp:TextBox ID="firstNameTxt" CssClass="missing" originalvalue="" AutoCompleteType="FirstName" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rvFirstName" runat="server" ControlToValidate="firstNameTxt" ErrorMessage="Please provide us with your first name" Display="Dynamic" CssClass="validationMsg" ValidationGroup="registration" />
                </span>
            </div>
            <div class="row">
                <span class="label">Last Name</span>
                <span class="field">
                    <asp:TextBox ID="lastNameTxt" CssClass="missing" originalvalue="" AutoCompleteType="LastName" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rvLastName" runat="server" ControlToValidate="lastNameTxt" ErrorMessage="Please provide us with your last name" Display="Dynamic" CssClass="validationMsg" ValidationGroup="registration" />
                </span>
            </div>
            <div class="row">
                <span class="label">Birthdate</span>
                <span class="field">
                    <asp:TextBox ID="birthdayTxt" CssClass="missing" originalvalue="" TextMode="Date" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rvBirthday" runat="server" ControlToValidate="birthdayTxt" ErrorMessage="Please provide us with your birthday" Display="Dynamic" CssClass="validationMsg" ValidationGroup="registration" />
                    <asp:CustomValidator ID="ageValid" runat="server" ValidationGroup="registration" ControlToValidate="birthdayTxt" OnServerValidate="ageValid_ServerValidate" ErrorMessage="You must be 21 or older to use Connect" Display="Dynamic" CssClass="validationMsg" />
                </span>
            </div>
            <div class="row">
                <span class="label">Country</span>
                <span class="field">
                    <asp:DropDownList ID="countryDDL" CssClass="missing" originalvalue="" AutoPostBack="true" EnableViewState="true" ViewStateMode="Enabled" runat="server"></asp:DropDownList>
                    <asp:RequiredFieldValidator ID="rvCountry" runat="server" ControlToValidate="countryDDL" InitialValue="" ErrorMessage="Please provide us with your country" Display="Dynamic" CssClass="validationMsg" ValidationGroup="registration" />
                </span>
            </div>
            <div class="row">
                <asp:Panel ID="statePnl" Visible="false" runat="server">
                    <span class="label">State</span>
                    <span class="field">
                        <asp:DropDownList ID="stateDDL" CssClass="missing" AutoPostBack="true" EnableViewState="true" ViewStateMode="Enabled" runat="server" CausesValidation="true"></asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rvRequiredState" runat="server" ControlToValidate="stateDDL" InitialValue="" ErrorMessage="Please provide us with your state" Display="Dynamic" CssClass="validationMsg" ValidationGroup="registration" />
                    </span>
                </asp:Panel>
            </div>
            <div class="row">
                <span class="label">Employer</span>
                <span class="field">
                    <asp:DropDownList ID="employerDDL" CssClass="missing" originalvalue="" AutoPostBack="true" EnableViewState="true" Visible="true" ViewStateMode="Enabled" runat="server"></asp:DropDownList>
                    <asp:RequiredFieldValidator ID="rvEmployer" runat="server" ControlToValidate="employerDDL" InitialValue="" ErrorMessage="Please provide us with your employer" Display="Dynamic" CssClass="validationMsg" ValidationGroup="registration" />
                    <%--<asp:TextBox ID="tradeEmployerTxt" CssClass="missing tradeEmployer" originalvalue="" runat="server"></asp:TextBox>
                    <div class="tradeEmployerAuto" style="display:none"></div>--%>
                </span>
            </div>
            <asp:Panel ID="distributorPnl" runat="server" Visible="false">
                <div class="row">
                    <asp:Panel runat="server" ID="areaPnl" Visible="false">
                        <span class="label">Area</span>
                        <span class="field">
                            <asp:DropDownList ID="areaDDL" CssClass="missing" originalvalue="" AutoPostBack="true" EnableViewState="true" ViewStateMode="Enabled" runat="server"></asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rvArea" runat="server" ControlToValidate="areaDDL" InitialValue="" ErrorMessage="Please provide us with your area" Display="Dynamic" CssClass="validationMsg" ValidationGroup="registration" />
                        </span>
                    </asp:Panel>
                </div>
                <div class="row">
                    <span class="label">On Off Premise</span>
                    <span class="field">
                        <asp:DropDownList ID="onOffPremiseDDL" CssClass="missing" originalvalue="" EnableViewState="true" ViewStateMode="Enabled" runat="server">
                            <asp:ListItem></asp:ListItem>
                            <asp:ListItem>On</asp:ListItem>
                            <asp:ListItem>Off</asp:ListItem>
                            <asp:ListItem>Both</asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rvOnOffPremise" runat="server" ControlToValidate="onOffPremiseDDL" InitialValue="" ErrorMessage="Please let us know if you are on-premise, off-premise, or both" Display="Dynamic" CssClass="validationMsg" ValidationGroup="registration" />
                    </span>
                </div>
            </asp:Panel>
            <div class="row">
                <asp:Button ID="updateBtn" ValidationGroup="registration" CausesValidation="true" CssClass="btn" runat="server" Text="Save Changes" OnClick="updateBtn_Click" />
                <asp:Button ID="cancelBtn" CssClass="btn" runat="server" Text="Cancel" OnClick="cancelBtn_Click" />
            </div>
        </asp:Panel>
        <asp:Panel ID="successMsg" CssClass="successMsg" runat="server"></asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<style>
    .manageprofile {
        font-size: 14px;
    }

        .manageprofile .inlinepanel {
            display: inline-block;
        }

        .manageprofile .label {
            width: 33%;
            min-width: 120px;
            display: inline-block;
            vertical-align: top;
            margin-top: 0.75em;
        }

        .manageprofile .field {
            width: 66%;
            display: inline-block;
            vertical-align: top;
        }

        .manageprofile select, .manageprofile input {
            width: 90%;
            border-radius: 0.2em;
            border: 1px solid #ccc;
            padding: 0.75em 0.5em;
        }

        .manageprofile .row {
            padding: 0.5em;
            vertical-align: top;
            text-align: left;
            max-width: 600px;
            margin: 0 auto;
        }

            .manageprofile .row .btn {
                float: right;
                width: auto;
            }

            .manageprofile .row .check > input {
                text-align: left;
                width: auto;
                margin-top: 1.5em;
            }

    .successMsg {
        top: -7.3em;
        background: #fff;
        position: relative;
        text-align: center;
    }

        .successMsg .btn {
            color: #333;
            border: 1px solid #ccc;
            border-radius: 0.2em;
            padding: 0.5em 0.75em;
            display: inline-block;
        }

            .successMsg .btn:hover {
                text-decoration: none;
                border-color: #000;
            }

        .successMsg p {
            text-align: center !important;
            margin-top: 0.5em;
        }

    #DeltaPlaceHolderMain .ms-rtestate-field h1 {
        font-weight: 300;
        cursor: default;
        text-align: center;
        font-size: 2.8em;
        margin-top: 0.75em;
    }

    div#contentBox.centered-content > div#sideNavBox {
        display: none !important;
    }

    .manageprofile select.missing, .manageprofile input.missing {
        border: 1px solid red;
    }

    div#contentBox.centered-content > #DeltaPlaceHolderMain {
        width: 100% !important;
        margin-top: -5em;
    }

    .validationMsg {
        color: #CC0A0A;
    }
</style>

<script type="text/javascript">
    jQuery(document).ready(function () {
    });

    function TradeAutocomplete() {
        if (typeof (tradeEmployers) != 'undefined') {
            var html = "<ul>";
            jQuery.each(tradeEmployers, function (i, n) {
                html += "<li class='employer'>" + n + "</li>";
            });
            html == "</ul>";
            jQuery('.tradeEmployerAuto').append(html);
            jQuery('.tradeEmployer').click(function () {
                jQuery('.tradeEmployerAuto').css('display', 'block');
                jQuery(document).click(function () {
                    if (jQuery(this).attr('class') != "employer")
                        jQuery('.tradeEmployerAuto').css('display', 'none');
                });
            });
            jQuery('.tradeEmployerAuto li').click(function () {
                jQuery('.tradeEmployer').val(jQuery(this).text());
                jQuery('.tradeEmployerAuto').css('display', 'none');
            });
        }
    }
</script>
<style>
    .tradeEmployerAuto ul {
        list-style-type: none;
        padding-left: 0;
    }
</style>
