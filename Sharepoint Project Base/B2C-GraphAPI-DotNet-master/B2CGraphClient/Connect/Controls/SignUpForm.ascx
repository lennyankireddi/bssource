<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SignUpForm.ascx.cs" Inherits="Connect.Controls.SignUpForm" %>

<asp:UpdatePanel ID="headerFieldsPnl" runat="server" EnableViewState="true" ChildrenAsTriggers="true" UpdateMode="Conditional" ViewStateMode="Enabled">
    <Triggers>

    </Triggers>
    <ContentTemplate>
        <div class="form">
            <div class="row">
                <span class="label">Email Address</span>
                <span class="field">
                    <asp:TextBox ID="emailTxt" TextMode="Email" AutoCompleteType="Email" runat="server"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="signInNameValid" runat="server" ValidationGroup="registration" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ControlToValidate="emailTxt" ErrorMessage="Please enter a valid email address" Display="Dynamic" CssClass="validationMsg" />
                </span>
            </div>
            <div class="row">
                <span class="label">Password</span>
                <span class="field">
                    <asp:TextBox ID="passTxt" TextMode="Password" CssClass="pass" runat="server"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="passValid" runat="server" ValidationGroup="registration" ControlToValidate="passTxt" ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,}" ErrorMessage="Password must be 8 characters or longer and include at least one capital letter, one number, and any of these valid special characters (!,@,$,%,&,*)" Display="Dynamic" CssClass="validationMsg" />
                </span>
            </div>
            <div class="row">
                <span class="label">Verify Password</span>
                <span class="field">
                    <asp:TextBox ID="passVerifyTxt" CssClass="passVerify" TextMode="Password" runat="server"></asp:TextBox>
                    <div id="passwordMatch" class="validationMsg"></div>
                </span>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:UpdatePanel ID="signupFormPnl" runat="server" EnableViewState="true" ChildrenAsTriggers="true" UpdateMode="Conditional" ViewStateMode="Enabled">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="distributeChk" EventName="CheckedChanged" />
        <asp:AsyncPostBackTrigger ControlID="countryDDL" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="stateDDL" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="areaDDL" EventName="SelectedIndexChanged" />
        <asp:AsyncPostBackTrigger ControlID="employerDDL" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>
        <div class="form">
            <div class="row">
                <span class="label">I am a Beam Suntory Distributor Partner</span>
                <span class="field">
                    <asp:CheckBox ID="distributeChk" CssClass="check" AutoPostBack="true" EnableViewState="true" ViewStateMode="Enabled" runat="server" OnCheckedChanged="distributeChk_CheckedChanged" /></span>
            </div>
            <div class="row">
                <span class="label">First Name</span>
                <span class="field">
                    <asp:TextBox ID="firstNameTxt" AutoCompleteType="FirstName" runat="server"></asp:TextBox></span>
            </div>
            <div class="row">
                <span class="label">Last Name</span>
                <span class="field">
                    <asp:TextBox ID="lastNameTxt" AutoCompleteType="LastName" runat="server"></asp:TextBox></span>
            </div>
            <div class="row">
                <span class="label">Birthdate</span>
                    <span class="field">
                        <asp:TextBox ID="birthdayTxt" TextMode="Date" runat="server"></asp:TextBox>
                        <asp:CustomValidator ID="ageValid" runat="server" ValidationGroup="registration" ControlToValidate="birthdayTxt" OnServerValidate="ageValid_ServerValidate" ErrorMessage="You must be 21 or older to use Connect" Display="Dynamic" CssClass="validationMsg" />
                    </span>
            </div>
            <div class="row">
                <span class="label">Country</span>
                <span class="field">
                    <asp:DropDownList ID="countryDDL" AutoPostBack="true" EnableViewState="true" ViewStateMode="Enabled" runat="server"></asp:DropDownList></span>
            </div>
            <div class="row">
                <asp:Panel ID="statePnl" Visible="false" runat="server">
                    <span class="label">State</span>
                    <span class="field">
                        <asp:DropDownList ID="stateDDL" AutoPostBack="true" EnableViewState="true" ViewStateMode="Enabled" runat="server"></asp:DropDownList></span>
                </asp:Panel>
            </div>
            <div class="row">
                <span class="label">Employer</span>
                <span class="field">
                    <asp:DropDownList ID="employerDDL"  AutoPostBack="true" EnableViewState="true" Visible="true" ViewStateMode="Enabled" runat="server"></asp:DropDownList>
                    <%--<asp:TextBox ID="tradeEmployerTxt" CssClass="tradeEmployer" runat="server"></asp:TextBox>
                    <div class="tradeEmployerAuto"></div>--%>
                </span>
            </div>
            <asp:Panel ID="distributorPnl" runat="server" Visible="false">
                <div class="row">
                    <asp:Panel runat="server" ID="areaPnl" Visible="false">
                        <span class="label">Area</span>
                        <span class="field">
                            <asp:DropDownList ID="areaDDL" AutoPostBack="true" EnableViewState="true" ViewStateMode="Enabled" runat="server"></asp:DropDownList></span>
                    </asp:Panel>
                </div>
                <div class="row">
                    <span class="label">Beam Suntory Sponsor</span>
                    <span class="field">
                        <asp:TextBox ID="beamSuntorySponsorTxt" AutoCompleteType="Email" TextMode="Email" runat="server"></asp:TextBox>
                        <asp:CustomValidator ID="sponserValid" runat="server" ValidationGroup="registration" ControlToValidate="beamSuntorySponsorTxt" Enabled="false" OnServerValidate="sponserValid_ServerValidate" ErrorMessage="Please enter a Beam Suntory email address for your sponsor." Display="Dynamic" CssClass="validationMsg" />
                    </span>
                </div>
                <div class="row">
                    <span class="label">On Off Premise</span>
                    <span class="field">
                        <asp:DropDownList ID="onOffPremiseDDL" EnableViewState="true" ViewStateMode="Enabled" runat="server">
                            <asp:ListItem></asp:ListItem>
                            <asp:ListItem>On</asp:ListItem>
                            <asp:ListItem>Off</asp:ListItem>
                            <asp:ListItem>Both</asp:ListItem>
                    </asp:DropDownList></span>
                </div>
            </asp:Panel>
            <div class="row">
                <asp:Button ID="signUpBtn" CssClass="btn" UseSubmitBehavior="true" runat="server" Text="Sign Up" OnClick="signUpBtn_Click" />
            </div>
        </div>
        <asp:Panel ID="successMsg" CssClass="successMsg" runat="server"></asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<style>
    .inlinepanel {display:inline-block;}
    .row .btn {float:right;width:auto;}
    .row .check > input {text-align:left;width:auto;margin-top:1.5em;}
    .successMsg {top:-4.3em;background:#fff;position:relative;}
    .successMsg .btn {color: #333;}
    .validationMsg{color:#CC0A0A;}
</style>

<script type="text/javascript">
    jQuery(document).ready(function () {
        PasswordComparison();
    });

    function TradeAutocomplete()
    {
        if(typeof(tradeEmployers) != 'undefined')
        {
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

    function PasswordComparison()
    {
        jQuery('.passVerify').keyup(function () {
            if (jQuery(this).val() != jQuery('.pass').val())
                jQuery('#passwordMatch').text("Passwords do not match")
            else
                jQuery('#passwordMatch').text("");
        });
    }
</script>