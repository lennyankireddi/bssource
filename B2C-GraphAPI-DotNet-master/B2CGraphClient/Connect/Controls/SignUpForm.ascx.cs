#define PROD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GraphApi;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net.Mime;
using System.Data.SqlClient;

namespace Connect.Controls
{
    public partial class SignUpForm : System.Web.UI.UserControl
    {
        List<Classes.CascadingDropDown> countrycdl = Classes.Helpers.GetCountries();
#if DEV
        private static string tenant = "BSIB2CDev.onmicrosoft.com";
        private static string id = "61eb9d21-9472-4af2-8b39-69647f309aa8";
        private static string secret = "7VNxTdHqru1Uqfyw+Qj0ZzVYQLdYgnNB6C0kt0ZXsn4=";
#endif
#if PROD
        private static string tenant = "extbeamsuntory.onmicrosoft.com";
        private static string id = "46d0355e-649f-4797-8bec-340e124fd149";
        private static string secret = "8NhTs6hDCn5s9VMOvkGrtDi7rHZZPlhNXYGzdlGZGgg=";
#endif
        internal bool TradeUser = true;

        protected void Page_Load(object sender, EventArgs e)
        {
            countryDDL.SelectedIndexChanged += CountryDDL_SelectedIndexChanged;
            stateDDL.SelectedIndexChanged += StateDDL_SelectedIndexChanged;
            //signUpBtn.Click += signUpBtn_Click;
            if (!Page.IsPostBack)
            {
                //this.Controls.Add(new LiteralControl("<script>var tradeEmployers = " + JsonConvert.SerializeObject(Classes.Helpers.GetTradeEmployers()) + ";TradeAutocomplete();</script>"));
                countryDDL.DataSource = countrycdl;
                countryDDL.DataValueField = "SystemValue";
                countryDDL.DataTextField = "Root";
                countryDDL.DataBind();
                employerDDL.DataSource = Classes.Helpers.GetTradeEmployers();
                employerDDL.DataBind();
            }
        }

        private string GetRegion()
        {
            string country = countryDDL.SelectedItem.Value;
            List<string> Americas = new List<string>
            {
                "Argentina",
                "Belize",
                "Bolivia, Plurinational State of",
                "Brazil",
                "Canada",
                "Chile",
                "Colombia",
                "Costa Rica",
                "Ecuador",
                "El Salvador",
                "Falkland Islands (Malvinas)",
                "French Guiana",
                "Guatemala",
                "Guyana",
                "Honduras",
                "Mexica",
                "Nicaragua",
                "Panama",
                "Paraguay",
                "Peru",
                "Suriname",
                "Uruguay",
                "Venezuela, Bolivarian Republic of",
                "South Georgia and the South Sandwich Islands",
                "United States"
            };
            if (Americas.Contains(country))
                return "Americas";
            return "International";
        }

        private string GetCommercialRegion()
        {
            if (TradeUser)
                return null;
            string state = stateDDL.SelectedItem.Value;
            switch(state)
            {
                case "California":
                case "Hawaii":
                case "Alaska":
                case "Arizona":
                case "Nevada":
                case "New Mexico":
                case "Washington":
                    return "SWS Region";
                case "Arkansas":
                case "Kansas":
                case "Louisiana":
                case "Texas":
                    return "Midsouth Region";
                    case "Connecticut":
                case "Massachusetts":
                case "Missouri":
                case "New Jersey":
                case "Rhode Island":
                case "Wisconsin":
                case "Alabama":
                case "Maine":
                case "Mississippi":
                case "New Hampshire":
                case "North Carolina":
                case "Pennsylvania":
                case "Vermont":
                case "Virginia":
                case "West Virginia":
                case "Idaho":
                case "Iowa":
                case "Michigan":
                case "Montana":
                case "Ohio":
                case "Oregon":
                case "Utah":
                case "Wyoming":
                    return "Franchise and Control Region";
                case "Georgia":
                case "Maryland":
                case "District of Columbia":
                case "Nebraska":
                case "Indiana":
                case "Colorado":
                case "North Dakota":
                case "Oklahoma":
                case "South Dakota":
                    return "Central Region";
                default:
                    return null;
            }
        }

        internal string GetDivision(string State)
        {
            switch (State)
            {
                case "California":
                case "Hawaii":
                    return "CA/HI Division";
                case "Alaska":
                case "Arizona":
                case "Nevada":
                case "New Mexico":
                case "Washington":
                    return "Mountain Division";
                case "Illinois":
                case "Kentucky":
                case "Minnesota":
                    return "Central Division";
                case "Delaware":
                case "New York":
                case "South Carolina":
                    return "East Coast Division";
                case "Florida":
                    return "Flordia Division";
                case "Connecticut":
                case "Massachusetts":
                case "Missouri":
                case "New Jersey":
                case "Rhode Island":
                case "Wisconsin":
                    return "Franchise Region";
                case "Alabama":
                case "Maine":
                case "Mississippi":
                case "New Hampshire":
                case "North Carolina":
                case "Pennsylvania":
                case "Vermont":
                case "Virginia":
                case "West Virginia":
                    return "Eastern Control Division";
                case "Idaho":
                case "Iowa":
                case "Michigan":
                case "Montana":
                case "Ohio":
                case "Oregon":
                case "Utah":
                case "Wyoming":
                    return "Western Control Division";
                case "Nebraska":
                case "Indiana":
                case "Colorado":
                case "North Dakota":
                case "Oklahoma":
                case "South Dakota":
                    return "Central Control Division";
                default:
                    return null;
            }
        }

        internal void SendNewUserEmail(B2CUser NewUser)
        {
            try
            {
                MailMessage m = new MailMessage();
                SmtpClient sc = new SmtpClient();

                m.From = new MailAddress("connect@beamsuntory.com", "Beam Suntory Connect");
                m.To.Add(new MailAddress("someone@beamsuntory.com", "Someones Name"));
                m.Subject = "New User Pending Approval in Connect - " + NewUser.mail;
                m.Body = "";
                m.IsBodyHtml = true;
                string html = "";
                AlternateView av = AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html);
                m.AlternateViews.Add(av);

                sc.Host = "smtp.beamsuntory.com";
                sc.Port = 587;
                sc.Credentials = new System.Net.NetworkCredential("from@username.com", "password");
                sc.EnableSsl = true;
                sc.Send(m);
            }
            catch(Exception) {  /* expected until we get full email login details */}
        }

#region Events
        protected void signUpBtn_Click(object sender, EventArgs e)
        {
            Page.Validate("registration");
            if(!Page.IsValid)
                return;
            //if trade user, then accountEnabled is true automatically without approval
            GraphApi.B2CNewUser newUser = new B2CNewUser
            {
                accountEnabled = (distributeChk.Checked) ? false : true,
                signInNames = new SignInName[] { new SignInName
                    {
                        type = "emailAddress",
                        value = emailTxt.Text
                    }
                },
                displayName = firstNameTxt.Text + " " + lastNameTxt.Text,
                creationType = "LocalAccount",
                givenName = firstNameTxt.Text,
                mailNickname = emailTxt.Text.Split('@')[0],
                passwordProfile = new PasswordProfile
                {
                    password = passTxt.Text,
                    forceChangePasswordNextLogin = false
                },
                passwordPolicies = "DisablePasswordExpiration",
                streetAddress = emailTxt.Text,
                surname = lastNameTxt.Text,
                usageLocation = "US",
                userType = "member",
                extension_a67c327875c34024bd4523a3d66619ba_Birthdate = birthdayTxt.Text,
                extension_a67c327875c34024bd4523a3d66619ba_Email = emailTxt.Text,
                extension_a67c327875c34024bd4523a3d66619ba_Employer = (employerDDL.SelectedIndex > 0) ? employerDDL.SelectedItem.Value : null,
                extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsRoleID = (!distributeChk.Checked) ? "Trade_ID" : "Dist_ID",
                extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsID = (Guid.NewGuid()).ToString().Replace("-","").Substring(0, 20) + "_EXT",
                country = (countryDDL.SelectedIndex > 0) ? countryDDL.SelectedItem.Value : null,
                state = (stateDDL.SelectedIndex > 0) ? stateDDL.SelectedItem.Value : null
            };
            if(distributeChk.Checked)
            {
                newUser.extension_a67c327875c34024bd4523a3d66619ba_Region = GetRegion();
                newUser.extension_a67c327875c34024bd4523a3d66619ba_BeamSuntorySponsor = beamSuntorySponsorTxt.Text;
                newUser.extension_a67c327875c34024bd4523a3d66619ba_Area = (areaDDL.SelectedIndex > 0) ? areaDDL.SelectedItem.Value : null;
                newUser.extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise = (onOffPremiseDDL.SelectedIndex > 0) ? onOffPremiseDDL.SelectedItem.Value : null;
                newUser.extension_a67c327875c34024bd4523a3d66619ba_CommercialRegion = GetCommercialRegion();
                newUser.extension_a67c327875c34024bd4523a3d66619ba_Division = (stateDDL.SelectedIndex > 0 && countryDDL.SelectedItem.Value == "United States") ? GetDivision(stateDDL.SelectedItem.Value) : null;
            }
            GraphApi.AADUser aad = new AADUser(tenant, id, secret);
            GraphApi.B2CUser user = null;
            try
            {
                user = aad.CreateUser(newUser);
            }
            catch(Exception ex)
            {
                //signupFormPnl.ContentTemplateContainer.Controls.Add(new LiteralControl("<div>" + ex.Message + "</div><div>" + ex.StackTrace + "</div><div>" + ex.InnerException + "</div>"));
                //signupFormPnl.Update();

                successMsg.Controls.Add(new LiteralControl("<h1>Whoops!</h1><p>We were unable to complete your registration at this time. If you are still experiencing issues, please contact <a href='mailto:connect@beamsuntory.com'>connect@beamsuntory.com</a> for assistance.</p><a class='btn' href='/'>OK</a>" + ex.Message));
                successMsg.Controls.Add(new LiteralControl("<style>.form .row {display:none !important;}.footer .btn {display:none;}</style>"));
                successMsg.Controls.Add(new LiteralControl("<div class='error' style='display:none'><div>New User Details:</div><div>" + Newtonsoft.Json.JsonConvert.SerializeObject(user) + "</div></div>"));
                signupFormPnl.Update();
            }
#if DEBUG
            successMsg.Controls.Add(new LiteralControl("<div style='display:none'><div>New User Details:</div><div>" + Newtonsoft.Json.JsonConvert.SerializeObject(user) + "</div></div>"));
#endif
            if(user != null)
            {
                if (user.userType != "Error")
                {
                    //woo... also indicate to the user that they have been signed up pending approval
                    //send pending user email to admin
                    if (distributeChk.Checked)
                        successMsg.Controls.Add(new LiteralControl("<h1>Registration Complete!</h1><p>You have successfully registered for Beam Suntory Connect. Your registration is pending approval which can take 1 - 2 business days.</p><a class='btn' href='/'>OK</a>"));
                    else
                        successMsg.Controls.Add(new LiteralControl("<h1>Registration Complete!</h1><p>You have successfully registered for Beam Suntory Connect and can use the system immediately. Note that youre learning courses will take 1 -2 business days to be available.</p><a class='btn' href='/'>Login</a>"));
                    bool TradeUser = (distributeChk.Checked) ? false : true;
                    Classes.Helpers.LogUserChange(TradeUser, false, GetUserDictionary(user));
                    successMsg.Controls.Add(new LiteralControl("<style>.form .row {display:none !important;}.footer .btn {display:none;}</style>"));
                    signupFormPnl.Update();
                }
                else
                {
#if PROD
                    string resetUrl = "https://passwordreset.microsoftonline.com/?ru=https%3a%2f%2flogin.microsoftonline.com%2fextbeamsuntory.onmicrosoft.com%2freprocess%3fctx%3drQIIAbVSv4vTcBQnSTt4HCgFQTiQG24S0uSbpL220KG9_rCl_YZrm4vNlqTftN80ybfXfEua4OCqooOIQic5t9vURW_wD7jFcxNHJ3F0ctNWwUXQyeF9eLz3Ph_e4_NucCALSnuWJZkSkk3ezqF9XnFEky8UkMivAzgj2bGBo8wzW1e2P78RMzuN6tPlzt3vW6_erZjraEktZPrhIqBkHmdJ4GN7TkLi0KxN_FOmP6F0FpYEwSNjHGR_d0ng4QBtZgSKhL-rCMRc0IkkbHCOwtlrhrlgmDssc49NAVGWV2waBbzWf8Yyp-w1o9uDN_UjywirpD5LLOdAV6ZRufyRvaxWNjobIHOcoE9_VL6w6Z97fmXTxws0jx9xV8kMBXhkkyBANs3iESVTFKy4-wyK24O-dohV3FY6Okg6OoxMXaS2381DSYuhD13D1-jQNTA8EEVDryedwThnDLoUNrVo6E4ltdmYGrqWtHCEj-peveUSrDYPl119KA2TCoUunKhrLqyN_DU3hrUhhX7bg66tDJMqNtzKshWIL7l_-HDG7YkyyOWkEeJFVCzwShEB3txfZw6QRUsumnkpv3_ObSPfxN7ur6M_cMxFivmWYk7Sa_Prj58_fPvgdu3Fe547eZLJnqeFTk-YjYuJE_eaqpsb3QLuMozCQQGAmNZacmV23OsaSWOAvaislMDZpf_xDT8A0&mkt=en-US&hosted=0&device_platform=Windows+10&nca=1&domain_hint=extbeamsuntory.onmicrosoft.com";
#endif
#if DEV
                    string resetUrl = "https://passwordreset.microsoftonline.com/?ru=https%3a%2f%2flogin.microsoftonline.com%2fbsib2cdev.onmicrosoft.com%2freprocess%3fctx%3drQIIAa1SO2zTUBSV7UQChARCQiJMIHUCOX7v2Wk-UoZ8HOQS28JxEmyxOPZz8kzslyavpPHExAALqjplKowdEUKoYmXJ1BExMIBYEBNiYKQBiaVi63DPcO_R0T3n3lsCzMPKxmCAPIRlT_QLuCgqIfDEUgkD8aRgGMihD0NleuXC5S83V_u5Tnr3xdf39PX3vU9LLlfvaHXUaOJHeZrExJ_SGQ1Z3qfxIWeMGJvMKpI0pkOS5P9NaTImCV5zJIalwYwMkB-cFpCot8NGSFrjFM8mbznumOMe89xTPgOBLC_5LE7EbueA5w75a2V6W5F7buB4eq_kFSwE5sloWK1-5C-ZtbXOGuiUpPjzqc43PvtnxR98dnsHTxd7wlU6wQkJfJok2Gd5EjD6ECdL4RmHF1t2p3uPmGRLafdh2u4bc68PmB_rm06kAzfWFCetMdPuym4HACeyxm17RAykMh1p0G12gWu7sRu1iEbmpKeOVS2ixIk15EStyG06zLB9oDcAMGxr1LaHyOyrzEEqMJp6wUhVxWx2FS0Br4Tcf8M7Eja8YmEAoQJE4CNZVHw5EEtyaVMMoOwPQhmgchGthIs49sj4xl-_HwTuOMP9ynAvsyfXfvLz3O6DaF8_eH79Xd1tvFllpbYlTYblNFxYd8yoENyH0e5sPrNLEC5YU5Nrk21Ld9OWTcbzarECj86f8Q_8Bg2&mkt=en-US&hosted=0&device_platform=Windows+10&nca=1&domain_hint=BSIB2CDev.onmicrosoft.com";
#endif
                    successMsg.Controls.Add(new LiteralControl("<h1>Whoops!</h1><p>We were unable to complete your registration because your account already exists. Click the buttons below to either log in or to reset your password. If you are still experiencing issues, please contact <a href='mailto:connect@beamsuntory.com'>connect@beamsuntory.com</a> for assistance.</p><a class='btn' href='/'>Login</a> <a class='btn' href='" + resetUrl + "'>Reset Password</a>"));
                    successMsg.Controls.Add(new LiteralControl("<style>.form .row {display:none !important;}.footer .btn {display:none;}</style>"));
                    successMsg.Controls.Add(new LiteralControl("<div class='error' style='display:none'><div>New User Details:</div><div>" + Newtonsoft.Json.JsonConvert.SerializeObject(user) + "</div></div>"));
                    signupFormPnl.Update();
                }
            }
        }

        internal Dictionary<string, string> GetUserDictionary(B2CUser User)
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            details.Add("preferredName", User.displayName);
            details.Add("country", User.country);
            details.Add("state", User.state);
            details.Add("FirstName", firstNameTxt.Text);
            details.Add("LastName", lastNameTxt.Text);
//#if DEV
            if(User.extension_a67c327875c34024bd4523a3d66619ba_Birthdate != null)
                details.Add("extension_a67c327875c34024bd4523a3d66619ba_Birthdate", User.extension_a67c327875c34024bd4523a3d66619ba_Birthdate);
            if(User.extension_a67c327875c34024bd4523a3d66619ba_Email != null)
                details.Add("extension_a67c327875c34024bd4523a3d66619ba_Email", User.extension_a67c327875c34024bd4523a3d66619ba_Email);
            if (User.extension_a67c327875c34024bd4523a3d66619ba_Employer != null)
                details.Add("extension_a67c327875c34024bd4523a3d66619ba_Employer", User.extension_a67c327875c34024bd4523a3d66619ba_Employer);
            if (User.extension_a67c327875c34024bd4523a3d66619ba_TradeEmployer != null)
                details.Add("extension_a67c327875c34024bd4523a3d66619ba_TradeEmployer", User.extension_a67c327875c34024bd4523a3d66619ba_TradeEmployer);
            if (User.extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsRoleID != null)
                details.Add("extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsRoleID", User.extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsRoleID);
            if (User.extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsID != null)
                details.Add("extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsID", User.extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsID);
/*#endif
#if PROD
            if (User.extension_be6dc6c96b4c411780751b4231962926_Birthdate != null)
                details.Add("extension_be6dc6c96b4c411780751b4231962926_Birthdate", User.extension_be6dc6c96b4c411780751b4231962926_Birthdate);
            if (User.extension_be6dc6c96b4c411780751b4231962926_Email != null)
                details.Add("extension_be6dc6c96b4c411780751b4231962926_Email", User.extension_be6dc6c96b4c411780751b4231962926_Email);
            if (User.extension_be6dc6c96b4c411780751b4231962926_Employer != null)
                details.Add("extension_be6dc6c96b4c411780751b4231962926_Employer", User.extension_be6dc6c96b4c411780751b4231962926_Employer);
            if (User.extension_be6dc6c96b4c411780751b4231962926_TradeEmployer != null)
                details.Add("extension_be6dc6c96b4c411780751b4231962926_TradeEmployer", User.extension_be6dc6c96b4c411780751b4231962926_TradeEmployer);
            if (User.extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsRoleID != null)
                details.Add("extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsRoleID", User.extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsRoleID);
            if (User.extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsID != null)
                details.Add("extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsID", User.extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsID);
#endif*/
            if (distributeChk.Checked)
            {
//#if DEV
                if (User.extension_a67c327875c34024bd4523a3d66619ba_Region != null)
                    details.Add("extension_a67c327875c34024bd4523a3d66619ba_Region",User.extension_a67c327875c34024bd4523a3d66619ba_Region);
                if (!String.IsNullOrEmpty(beamSuntorySponsorTxt.Text))
                    details.Add("extension_a67c327875c34024bd4523a3d66619ba_BeamSuntorySponsor", beamSuntorySponsorTxt.Text);
                if (User.extension_a67c327875c34024bd4523a3d66619ba_Area != null)
                    details.Add("extension_a67c327875c34024bd4523a3d66619ba_Area", User.extension_a67c327875c34024bd4523a3d66619ba_Area);
                if (User.extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise != null)
                    details.Add("extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise", User.extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise);
                if (!String.IsNullOrEmpty(GetCommercialRegion()))
                    details.Add("extension_a67c327875c34024bd4523a3d66619ba_CommercialRegion", GetCommercialRegion());
                if (User.extension_a67c327875c34024bd4523a3d66619ba_Division != null)
                    details.Add("extension_a67c327875c34024bd4523a3d66619ba_Division", User.extension_a67c327875c34024bd4523a3d66619ba_Division);
/*#endif
#if PROD
                if (User.extension_be6dc6c96b4c411780751b4231962926_Region != null)
                    details.Add("extension_be6dc6c96b4c411780751b4231962926_Region", User.extension_be6dc6c96b4c411780751b4231962926_Region);
                if (!String.IsNullOrEmpty(beamSuntorySponsorTxt.Text))
                    details.Add("extension_be6dc6c96b4c411780751b4231962926_BeamSuntorySponsor", beamSuntorySponsorTxt.Text);
                if (User.extension_be6dc6c96b4c411780751b4231962926_Area != null)
                    details.Add("extension_be6dc6c96b4c411780751b4231962926_Area", User.extension_be6dc6c96b4c411780751b4231962926_Area);
                if (User.extension_be6dc6c96b4c411780751b4231962926_OnOffPremise != null)
                    details.Add("extension_be6dc6c96b4c411780751b4231962926_OnOffPremise", User.extension_be6dc6c96b4c411780751b4231962926_OnOffPremise);
                if (!String.IsNullOrEmpty(GetCommercialRegion()))
                    details.Add("extension_be6dc6c96b4c411780751b4231962926_CommercialRegion", GetCommercialRegion());
                if (User.extension_be6dc6c96b4c411780751b4231962926_Division != null)
                    details.Add("extension_be6dc6c96b4c411780751b4231962926_Division", User.extension_be6dc6c96b4c411780751b4231962926_Division);
#endif*/
            }
            return details;
        }

        protected void distributeChk_CheckedChanged(object sender, EventArgs e)
        {
            //reverse true/false from the check box
            TradeUser = (distributeChk.Checked) ? false : true;
            distributorPnl.Visible = distributeChk.Checked;
            //employerDDL.Visible = distributeChk.Checked;
            //tradeEmployerTxt.Visible = TradeUser;

            if(TradeUser)
            {
                employerDDL.DataSource = Classes.Helpers.GetTradeEmployers();
                employerDDL.DataBind();                
            }
            else
            {
                employerDDL.DataSource = Classes.Helpers.GetEmployers();
                employerDDL.DataBind();
            }

            signupFormPnl.Update();
        }

        private void CountryDDL_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (countrycdl[countryDDL.SelectedIndex].Children.Count() > 0)
            {
                // populate state values with list items that have parent assigned to them
                statePnl.Visible = true;
                List<Classes.CascadingDropDown> states = countrycdl[countryDDL.SelectedIndex].Children;
                stateDDL.DataSource = states;
                stateDDL.DataValueField = "SystemValue";
                stateDDL.DataTextField = "Root";
                stateDDL.DataBind();
                signupFormPnl.Update();
            }
            else
            {
                // unselect and hide state dropdown if country doesn't have states
                stateDDL.SelectedIndex = -1;   
                statePnl.Visible = false;   
                
                // if area was visible due to certain state selection, hide and unselect
                if(areaPnl.Visible)
                {
                    areaDDL.SelectedIndex = -1;
                    areaPnl.Visible = false;
                }
                                         
                signupFormPnl.Update();
            }
        }

        private void StateDDL_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!distributeChk.Checked)
                return;
            string state = stateDDL.SelectedItem.Text;
            List<string> texas = new List<string> { "", "Dallas/Ft Worth","Austin/San Antonio","Houston/South TX","Chain Sales" };
            List<string> newyork = new List<string> { "", "UNY", "MNY" };
            List<string> florida = new List<string> { "", "NFL","SFL","Chain Sales" };
            List<string> california = new List<string> { "", "NCA", "SCA", "Chain Sales" };

            switch(state)
            {
                case "Texas":
                    areaDDL.DataSource = texas;
                    areaDDL.DataBind();
                    areaPnl.Visible = true;
                    signupFormPnl.Update();
                    break;
                case "New York":
                    areaDDL.DataSource = newyork;
                    areaDDL.DataBind();
                    areaPnl.Visible = true;
                    signupFormPnl.Update();
                    break;
                case "Florida":
                    areaDDL.DataSource = florida;
                    areaDDL.DataBind();
                    areaPnl.Visible = true;
                    signupFormPnl.Update();
                    break;
                case "California":
                    areaDDL.DataSource = california;
                    areaDDL.DataBind();
                    areaPnl.Visible = true;
                    signupFormPnl.Update();
                    break;
                default:
                    areaPnl.Visible = false;
                    signupFormPnl.Update();
                    break;
            }
        }
#endregion

        protected void passTxt_TextChanged(object sender, EventArgs e)
        {
            ViewState["pass"] = passTxt.Text;
        }

        protected void sponserValid_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!distributeChk.Checked)
                args.IsValid = true;
            else if(distributeChk.Checked && !String.IsNullOrEmpty(beamSuntorySponsorTxt.Text))
            {
                if (beamSuntorySponsorTxt.Text.Contains("@"))
                {
                    if (beamSuntorySponsorTxt.Text.Split('@')[0].ToLower() == "beamsuntory.com")
                        args.IsValid = true;
                    else
                        args.IsValid = false;
                }
                else
                    args.IsValid = false;
            }
            else
                args.IsValid = false;
        }

        protected void ageValid_ServerValidate(object source, ServerValidateEventArgs args)
        {
            try
            {
                if (!String.IsNullOrEmpty(birthdayTxt.Text))
                {
                    TimeSpan ts = DateTime.Now - (Convert.ToDateTime(birthdayTxt.Text));
                    if (ts.Days > (365 * 21))
                        args.IsValid = true;
                    else
                        args.IsValid = false;
                }
                else
                    args.IsValid = false;
            }
            catch (Exception) { /* invalid date */
                args.IsValid = false;
            }
        }
    }
}