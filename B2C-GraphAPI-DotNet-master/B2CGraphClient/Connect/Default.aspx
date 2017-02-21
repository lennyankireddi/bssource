<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Connect._Default" %>
<%@ Register Src="~/Controls/SignUpForm.ascx" TagPrefix="uc1" TagName="SignUpForm" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">
    Register for Connect
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <uc1:SignUpForm runat="server" ID="SignUpForm" />
</asp:Content>