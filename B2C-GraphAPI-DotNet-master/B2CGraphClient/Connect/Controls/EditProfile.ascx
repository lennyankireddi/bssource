<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditProfile.ascx.cs" Inherits="Connect.Controls.EditProfile" %>

<div class="form">
    <div class="row">
        <span class="label">Email Address</span>
        <span class="field">
            <asp:TextBox ID="emailTxt" TextMode="Email" runat="server"></asp:TextBox></span>
        <span class="label">Password</span>
        <span class="field">
            <asp:TextBox ID="passTxt" TextMode="Password" runat="server"></asp:TextBox></span>
    </div>
    <div class="row">
        <span class="label">City</span>
        <span class="field">
            <asp:TextBox ID="cityTxt" runat="server"></asp:TextBox></span>
        <span class="label">Company</span>
        <span class="field">
            <asp:TextBox ID="companyTxt" runat="server"></asp:TextBox></span>
    </div>
    <div class="row">
        <span class="label">Country</span>
        <span class="field">
            <asp:DropDownList ID="countryDDL" runat="server"></asp:DropDownList></span>
        <span class="label">Phone Number</span>
        <span class="field">
            <asp:TextBox ID="phoneTxt" TextMode="Phone" runat="server"></asp:TextBox></span>
    </div>
    <div class="row">
        <span class="label">Full Name</span>
        <span class="field">
            <asp:TextBox ID="nameTxt" runat="server"></asp:TextBox></span>
        <span class="label">Job Title</span>
        <span class="field">
            <asp:TextBox ID="jobTitleTxt" runat="server"></asp:TextBox></span>
    </div>
    <div class="row">
        <span class="label">State</span>
        <span class="field">
            <asp:DropDownList ID="stateDDL" runat="server"></asp:DropDownList></span>
        <span class="label">Street Address</span>
        <span class="field">
            <asp:TextBox ID="addressTxt" runat="server"></asp:TextBox></span>
    </div>
    <div class="row">
        <span class="label">Zip Code</span>
        <span class="field">
            <asp:TextBox ID="zipcodeTxt" runat="server"></asp:TextBox></span>
        <span class="label">Area</span>
        <span class="field">
            <asp:DropDownList ID="areaDDL" runat="server"></asp:DropDownList></span>
    </div>
    <div class="row">
        <span class="label">BeamSuntorySponsor</span>
        <span class="field">
            <asp:TextBox ID="beamSuntorySponsorTxt" runat="server"></asp:TextBox></span>
        <span class="label">Birthdate</span>
        <span class="field">
            <span><asp:TextBox ID="birthdayTxt" TextMode="Date" runat="server"></asp:TextBox></span><span class="toggleCalendar">*</span>
            <span class="calendarContainer"><asp:Calendar ID="birthdayDatePicker" runat="server"></asp:Calendar></span>
        </span>
    </div>
    <div class="row">
        <span class="label">CommercialRegion</span>
        <span class="field">
            <asp:DropDownList ID="commercialRegionDDL" runat="server"></asp:DropDownList></span>
        <span class="label">extensionAttribute1</span>
        <span class="field">
            <asp:TextBox ID="extensionAttribute1Txt" runat="server"></asp:TextBox></span>
    </div>
    <div class="row">
        <span class="label">Employer</span>
        <span class="field">
            <asp:DropDownList ID="employerDDL" runat="server"></asp:DropDownList></span>
        <span class="label">OnOffPremise</span>
        <span class="field">
            <asp:DropDownList ID="onOffPremiseDDL" runat="server"></asp:DropDownList></span>
    </div>
    <div class="row">
        <span class="label">Region</span>
        <span class="field">
            <asp:DropDownList ID="regionDDL" runat="server"></asp:DropDownList></span>
        <span class="label">UserType</span>
        <span class="field">
            <asp:DropDownList ID="userTypeDDL" runat="server"></asp:DropDownList></span>
    </div>
</div>
<div class="footer row">
    <asp:Button ID="updateBtn" CssClass="btn" runat="server" UseSubmitBehavior="true" Text="Update My Details" OnClick="signUpBtn_Click" />
</div>
<style>
    .label {width:15%;min-width:120px;display:inline-block;vertical-align:top;}
    .field {width:33%;display:inline-block;vertical-align:top;}
    .form select, .form input {width:90%;border-radius:0.2em;border:1px solid #ccc;}
    .row {padding:0.5em;vertical-align:top;}
    .footer {text-align:right;margin-right:1em}
    .calendarContainer {float:right;position:absolute;display:none;}
    .toggleCalendar {cursor:pointer;}
</style>
<script>
    jQuery(document).ready(function () {
        jQuery('.toggleCalendar').click(function () {
            jQuery('.calendarContainer').toggle();
        });
    });
</script>