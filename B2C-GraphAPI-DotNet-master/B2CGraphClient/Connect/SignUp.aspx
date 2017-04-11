<%@ Page Title="Sign Up For Connect" Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="SignUp.aspx.cs" Inherits="Connect.SignUp" %>

<%@ Register Src="~/Controls/SignUpForm.ascx" TagPrefix="uc1" TagName="SignUpForm" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">
    Register for Connect
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc1:SignUpForm runat="server" ID="SignUpForm" />
</asp:Content>

