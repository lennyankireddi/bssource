#define PROD
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Web.UI;
using System.Linq;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using GraphApi;
using System.Threading;
using Microsoft.IdentityModel.Claims;
using System.Web;
using Facet.Combinatorics;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Microsoft.Office.Server.UserProfiles;

namespace BEAM.Connect.ManageProfile
{
    public partial class ManageProfileUserControl : UserControl
    {
        Dictionary<string, string> claims = GetClaimsForUser();
        List<CascadingDropDown> countrycdl = Helpers.GetCountries();
        HttpCookie _claimsCookie = GetClaimsCookie();
        
        string upn = "";
        bool TradeUser = true;
        List<UserMap> mapping = GetRequiredInformation();
        bool RequiredFields = (HttpContext.Current.Request.QueryString.AllKeys.Contains("RequiredFields")) ? true : false;
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

        protected void Page_Load(object sender, EventArgs e)
        {
            //ignore windows and internal users
            if (claims["identityprovider"] == "windows")
            {
                signupFormPnl.Visible = false;
                return;
            }
            else if (claims.ContainsKey("extensionAttribute6"))
            {
                if (claims["extensionAttribute6"] == "Internal_USER")
                {
                    signupFormPnl.Visible = false;
                    return;
                }
                else if (claims["extensionAttribute6"] == "Dist_ID")
                    TradeUser = false;
            }
            if (!claims.ContainsKey("objectidentifier"))
            {
                //cant do anything with a user without upn - passed as emailaddress
                signupFormPnl.Visible = false;
                return;
            }
            else
            {
                upn = claims["objectidentifier"];
                countryDDL.SelectedIndexChanged += CountryDDL_SelectedIndexChanged;
                stateDDL.SelectedIndexChanged += StateDDL_SelectedIndexChanged;
                if (!Page.IsPostBack)
                {
                    if(RequiredFields)
                        manageProfilePnl.CssClass = "registrationform form manageprofile";
                    //this.Controls.Add(new LiteralControl("<script>var tradeEmployers = " + JsonConvert.SerializeObject(Helpers.GetTradeEmployers()) + ";TradeAutocomplete();</script>"));
                    countryDDL.DataSource = countrycdl;
                    countryDDL.DataValueField = "SystemValue";
                    countryDDL.DataTextField = "Root";
                    countryDDL.DataBind();

                    if (TradeUser)
                    {
                        employerDDL.DataSource = Helpers.GetTradeEmployers();
                    }
                    else
                    {
                        employerDDL.DataSource = Helpers.GetEmployers();
                    }
                    
                    employerDDL.DataBind();
                    if (_claimsCookie != null)
                        claims = CompareChanges(claims, JsonConvert.DeserializeObject<Dictionary<string, string>>(_claimsCookie.Value));
                    PopulateCurrentValues();
                }
            }
        }

        #region Event Handlers
        private void StateDDL_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TradeUser)
                return;
            string state = stateDDL.SelectedItem.Text;

            UpdateAreaDDL(state);
        }        

        private void CountryDDL_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (countrycdl[countryDDL.SelectedIndex].Children.Count() > 0)
            {
                // populate state values with list items that have parent assigned to them
                statePnl.Visible = true;
                List<CascadingDropDown> states = countrycdl[countryDDL.SelectedIndex].Children;
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
                if (areaPnl.Visible)
                {
                    areaDDL.SelectedIndex = -1;
                    areaPnl.Visible = false;
                }

                signupFormPnl.Update();
            }
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
            catch (Exception)
            { /* invalid date */
                args.IsValid = false;
            }
        }

        protected void updateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Dictionary<string, string> updates = GetUpdatedValues();
                if (updates.Count > 0)
                {
                    updateBtn.Text = "";
                    updateBtn.CssClass = "btn icon-spin icon-refresh";
                    updateBtn.Enabled = false;
                    signupFormPnl.Update();
                    if (UpdateB2C(updates, upn))
                    {
                        HttpResponse res = HttpContext.Current.Response;
                        if (_claimsCookie == null)
                        {
                            _claimsCookie = new HttpCookie("claims", JsonConvert.SerializeObject(updates));
                            res.Cookies.Add(_claimsCookie);
                        }
                        else
                            res.Cookies["claims"].Value = JsonConvert.SerializeObject(updates);
                        updates.Add("preferredName", firstNameTxt.Text + " " + lastNameTxt.Text);
                        Helpers.LogUserChange(TradeUser, true, updates);
                        successMsg.Controls.Add(new LiteralControl("<h3>Update Complete!</h3><p>Your profile has been updated. Note that some changes may not be visible until you log out and log back in.<p><a class='btn' href='/'>OK</a>"));
                    }
                    else
                    {
                        successMsg.Controls.Add(new LiteralControl("<h3>Whoops!</h3><p>Unable to update your profile at this time. Please try again later.</p><p>If you are still experiencing issues, please contact <a href='mailto:connect@beamsuntory.com'>connect@beamsuntory.com</a> for assistance.</p><a class='btn' href='/'>OK</a>"));
                    }
                    successMsg.Controls.Add(new LiteralControl("<style>.form .row {display:none !important;}.footer .btn {display:none;}</style>"));
                    signupFormPnl.Update();
                }
                else
                {
                    successMsg.Controls.Add(new LiteralControl("<h2>Profile Not Updated!</h2><p>Your profile was not updated because you did not submit any new information. Click OK to return to CONNECT.<p><a class='btn' href='/'>OK</a>"));
                    successMsg.Controls.Add(new LiteralControl("<style>.form .row {display:none !important;}.footer .btn {display:none;}</style>"));
                    signupFormPnl.Update();
                }
            }
            catch(Exception ex)
            {
                successMsg.Controls.Add(new LiteralControl("<h2>Something went wrong!</h2><p>Unable to update profile at this time. Please try again later.</p><p>If you are still experiencing issues, please contact <a href='mailto:connect@beamsuntory.com'>connect@beamsuntory.com</a> for assistance.</p><p>" + ex.StackTrace +"</p><div class='error' style='display:none'>" + ex.StackTrace + "</div><a class='btn' href='/'>OK</a>"));
                successMsg.Controls.Add(new LiteralControl("<style>.form .row {display:none !important;}.footer .btn {display:none;}</style>"));
                signupFormPnl.Update();
            }
        }

        protected void cancelBtn_Click(object sender, EventArgs e)
        {
            HttpResponse res = HttpContext.Current.Response;
            res.Redirect(SPContext.Current.Site.Url);
        }
        #endregion

        private void UpdateStateDDL()
        {
            if (countrycdl[countryDDL.SelectedIndex].Children.Count() > 0)
            {
                statePnl.Visible = true;
                List<CascadingDropDown> states = countrycdl[countryDDL.SelectedIndex].Children;
                stateDDL.DataSource = states;
                stateDDL.DataValueField = "SystemValue";
                stateDDL.DataTextField = "Root";
                stateDDL.CssClass = "";
                stateDDL.DataBind();
                signupFormPnl.Update();
            }
        }

        private void UpdateAreaDDL(string state)
        {
            List<string> texas = new List<string> { "", "Dallas/Ft Worth", "Austin/San Antonio", "Houston/South TX", "Chain Sales" };
            List<string> newyork = new List<string> { "", "UNY", "MNY" };
            List<string> florida = new List<string> { "", "NFL", "SFL", "Chain Sales" };
            List<string> california = new List<string> { "", "NCA", "SCA", "Chain Sales" };

            switch (state)
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
                    areaDDL.SelectedIndex = -1;
                    signupFormPnl.Update();
                    break;
            }
        }

        internal static HttpCookie GetClaimsCookie()
        {
            HttpCookieCollection cookies = HttpContext.Current.Request.Cookies;
            for(int i = 0; i < cookies.Count;i++ )
            {
                if (cookies[i].Name == "claims")
                    return cookies[i];
            }
            return null;
        }

        internal Dictionary<string, string> CompareChanges(Dictionary<string, string> Claims, Dictionary<string, string> PendingUpdates)
        {
            for(int i = 0;i < Claims.Count; i++)
            {
                switch(Claims.Keys.ElementAt(i))
                {
                    case "givenname":
                        if (PendingUpdates.ContainsKey("givenName"))
                        {
                            Claims["givenname"] = PendingUpdates["givenname"];
                            PendingUpdates.Remove("givenname");
                        }
                        break;
                    case "surname":
                        if (PendingUpdates.ContainsKey("surname"))
                        {
                            Claims["surname"] = PendingUpdates["surname"];
                                PendingUpdates.Remove("surname");
                        }
                        break;
                    case "Birthdate":
                        if (PendingUpdates.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_Birthdate"))
                        {
                            Claims["Birthdate"] = PendingUpdates["extension_a67c327875c34024bd4523a3d66619ba_Birthdate"];
                            PendingUpdates.Remove("extension_a67c327875c34024bd4523a3d66619ba_Birthdate");
                        }
                        break;
                    case "country":
                        if (PendingUpdates.ContainsKey("country"))
                        {
                            Claims["country"] = PendingUpdates["country"];
                            PendingUpdates.Remove("country");
                        }
                        break;
                    case "state":
                        if (PendingUpdates.ContainsKey("state"))
                        {
                            Claims["state"] = PendingUpdates["state"];
                            PendingUpdates.Remove("state");
                        }
                        break;
                    case "employer":
                        if (PendingUpdates.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_Employer"))
                        {
                            Claims["employer"] = PendingUpdates["extension_a67c327875c34024bd4523a3d66619ba_Employer"];
                                PendingUpdates.Remove("extension_a67c327875c34024bd4523a3d66619ba_Employer");
                        }
                        break;
                    case "extensionAttribute3"://"area":
                        if (PendingUpdates.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_Area"))
                    {
                        Claims["extensionAttribute3"] = PendingUpdates["extension_a67c327875c34024bd4523a3d66619ba_Area"];
                            PendingUpdates.Remove("extension_a67c327875c34024bd4523a3d66619ba_Area");
                        }
                        break;
                    case "extensionAttribute4": //onoffpremise
                        if (PendingUpdates.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise"))
                    {
                        Claims["extensionAttribute4"] = PendingUpdates["extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise"];
                            PendingUpdates.Remove("extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise");
                        }
                        break;
                }
            }
            foreach(string pk in PendingUpdates.Keys)
            {
                switch(pk)
                {
                    case "extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise":
                        Claims.Add("extensionAttribute4", PendingUpdates[pk]);
                        break;
                    case "givenname":
                        Claims.Add("givenname", PendingUpdates[pk]);
                        break;
                    case "surname":
                        Claims.Add("surname", PendingUpdates[pk]);
                        break;
                    case "extension_a67c327875c34024bd4523a3d66619ba_Area":
                        Claims.Add("extensionAttribute3", PendingUpdates[pk]);
                        break;
                    case "extension_a67c327875c34024bd4523a3d66619ba_Birthdate":
                        Claims.Add("Birthdate", PendingUpdates[pk]);
                        break;
                    case "extension_a67c327875c34024bd4523a3d66619ba_Employer":
                        Claims.Add("employer", PendingUpdates[pk]);
                        break;
                    case "country":
                        Claims.Add("country", PendingUpdates[pk]);
                        break;
                    case "state":
                        Claims.Add("state", PendingUpdates[pk]);
                        break;
                }
            }
            return Claims;
        }

        internal void PopulateCurrentValues()
        {
            foreach(string k in claims.Keys)
            {
                switch(k)
                {
                    case "givenname":
                        firstNameTxt.Text = claims[k];
                        firstNameTxt.Attributes.Add("originalvalue", claims[k]);
                        firstNameTxt.Attributes.Add("claimtype", k);
                        firstNameTxt.CssClass = "";
                        break;
                    case "surname":
                        lastNameTxt.Text = claims[k];
                        lastNameTxt.Attributes.Add("originalvalue", claims[k]);
                        lastNameTxt.Attributes.Add("claimtype", k);
                        lastNameTxt.CssClass = "";
                        break;
                    case "Birthdate":
                        birthdayTxt.Attributes.Add("originalvalue", claims[k]);
                        birthdayTxt.Attributes.Add("claimtype", k);
                        birthdayTxt.Text = claims[k];
                        birthdayTxt.CssClass = "";
                        break;
                    case "country":
                        countryDDL.Attributes.Add("originalvalue", claims[k]);
                        countryDDL.Attributes.Add("claimtype", k);
                        countryDDL.SelectedValue = claims[k];
                        UpdateStateDDL();
                        countryDDL.CssClass = "";
                        break;
                    case "state":
                        stateDDL.Attributes.Add("originalvalue", claims[k]);
                        stateDDL.Attributes.Add("claimtype", k);
                        stateDDL.SelectedValue = claims[k];
                        stateDDL.Visible = true;
                        if (stateDDL.SelectedIndex > 0)
                            UpdateAreaDDL(stateDDL.SelectedItem.Text);
                        stateDDL.CssClass = "";
                        break;
                    case "employer":
                        // commented out since both types of users now share the same dropdown
                        //if (TradeUser)
                        //{
                        //    tradeEmployerTxt.Attributes.Add("originalvalue", claims[k]);
                        //    tradeEmployerTxt.Attributes.Add("claimtype", k);
                        //    tradeEmployerTxt.Text = claims[k];
                        //    tradeEmployerTxt.CssClass = "tradeEmployer";
                        //}
                        //else
                        //{
                            employerDDL.Attributes.Add("originalvalue", claims[k]);
                            employerDDL.Attributes.Add("claimtype", k);
                            employerDDL.SelectedValue = claims[k].ToUpper();
                            employerDDL.CssClass = "";
                        //}
                        break;
                    case "extensionAttribute3"://"area":                                                
                        areaDDL.Attributes.Add("originalvalue", claims[k]);
                        areaDDL.Attributes.Add("claimtype", k);
                        areaDDL.SelectedValue = claims[k];
                        areaDDL.CssClass = "";
                        break;
                    case "extensionAttribute4": //onoffpremise
                        onOffPremiseDDL.Attributes.Add("originalvalue", claims[k]);
                        onOffPremiseDDL.Attributes.Add("claimtype", k);
                        onOffPremiseDDL.SelectedValue = claims[k];
                        onOffPremiseDDL.CssClass = "";
                        break;
                }
            }
            if (!TradeUser)
            {
                //tradeEmployerTxt.Visible = false;
                //employerDDL.Visible = true;
                distributorPnl.Visible = true;
            }
            signupFormPnl.Update();
        }

        internal Dictionary<string, string> GetUpdatedValues()
        {
            Dictionary<string, string> vals = new Dictionary<string, string>();
            if (firstNameTxt.Text != firstNameTxt.Attributes["originalvalue"])
            {
                vals.Add("givenName", firstNameTxt.Text);
                vals.Add("displayName", firstNameTxt.Text + " " + lastNameTxt.Text);
            }

            if(lastNameTxt.Text != lastNameTxt.Attributes["originalvalue"])
            {
                vals.Add("surname", lastNameTxt.Text);
                vals.Add("displayName", firstNameTxt.Text + " " + lastNameTxt.Text);
            }

            try
            {
                if (countryDDL.SelectedIndex > 0)
                {
                    if (countryDDL.SelectedItem.Value != countryDDL.Attributes["originalvalue"])
                        vals.Add("country", countryDDL.SelectedItem.Value);
                }
                else
                {
                    vals.Add("country", "NA_NULL");
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            try
            {
                if(stateDDL.SelectedIndex > 0)
                {
                    if (stateDDL.SelectedItem.Value != stateDDL.Attributes["originalvalue"])
                        vals.Add("state", stateDDL.SelectedItem.Value);
                }
                else
                {
                    vals.Add("state", "NA_NULL");
                }
                
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            if(birthdayTxt.Text != birthdayTxt.Attributes["originalvalue"])
            {
                vals.Add("extension_a67c327875c34024bd4523a3d66619ba_Birthdate", birthdayTxt.Text);
            }
            if(TradeUser)
            {
                // commented out because now both share employerddl to populate
                //if(tradeEmployerTxt.Text != tradeEmployerTxt.Attributes["originalvalue"])
                //{
                //    vals.Add("extension_a67c327875c34024bd4523a3d66619ba_Employer", tradeEmployerTxt.Text);
                //}

                if (employerDDL.SelectedItem.Value != employerDDL.Attributes["originalvalue"])
                {
                    vals.Add("extension_a67c327875c34024bd4523a3d66619ba_Employer", employerDDL.SelectedItem.Value);
                }
            }
            else //distributor
            {
                if(employerDDL.SelectedItem.Value != employerDDL.Attributes["originalvalue"])
                {
                    vals.Add("extension_a67c327875c34024bd4523a3d66619ba_Employer", employerDDL.SelectedItem.Value);
                }
                //the nested panels sometimes cause issues here if they're not visible
                try
                {
                    if (areaDDL.SelectedIndex > 0)
                    {
                        if (areaDDL.SelectedItem.Value != areaDDL.Attributes["originalvalue"])
                            vals.Add("extension_a67c327875c34024bd4523a3d66619ba_Area", areaDDL.SelectedItem.Value);
                    }
                    else
                    {
                        vals.Add("extension_a67c327875c34024bd4523a3d66619ba_Area", "NA_NULL");
                    }                        
                }
                catch (Exception) { }
                try { 
                if (onOffPremiseDDL.SelectedItem.Value != onOffPremiseDDL.Attributes["originalvalue"])
                    vals.Add("extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise", onOffPremiseDDL.SelectedItem.Value);
                }
                catch (Exception) { }

                if (countryDDL.SelectedItem.Value != countryDDL.Attributes["originalvalue"])
                {
                    vals.Add("extension_a67c327875c34024bd4523a3d66619ba_Region", GetRegion());
                }
                if ((stateDDL.SelectedIndex > 0 && countryDDL.SelectedItem.Value == "United States") && stateDDL.SelectedItem.Value != stateDDL.Attributes["originalvalue"])
                {
                    vals.Add("extension_a67c327875c34024bd4523a3d66619ba_Division", GetDivision(stateDDL.SelectedItem.Value));
                    vals.Add("extension_a67c327875c34024bd4523a3d66619ba_CommercialRegion", GetCommercialRegion());
                }
            }
            return vals;
        }

        internal bool UpdateB2C(Dictionary<string, string> updates, string userId)
        {
            if (String.IsNullOrEmpty(userId))
                return false;
            AADUser aad = new AADUser(tenant, id, secret);
            B2CGraphClient b2c = new B2CGraphClient(id, secret, tenant);
            string result = b2c.UpdateUser(userId, JsonConvert.SerializeObject(updates)).Result;
            if (!result.Contains("Error"))
                return true;
            return false;
        }

        internal static List<UserMap> GetRequiredInformation()
        {
            List<UserMap> mapping = new List<UserMap>();
            foreach (SPListItem i in SPContext.Current.Site.RootWeb.Lists["RequiredInfo"].Items)
            {
                mapping.Add(new UserMap
                {
                    ProfileProperty = i["ProfileProperty"].ToString(),
                    ClaimType = i["Claim"].ToString(),
                    DisplayName = i["Title"].ToString(),
                    B2CField = i["B2CField"].ToString()
                });
            }
            return mapping;
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
            switch (state)
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

        internal static Dictionary<string, string> GetClaimsForUser()
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();
            IClaimsPrincipal cp = (IClaimsPrincipal)Thread.CurrentPrincipal;
            foreach (var c in cp.Identities[0].Claims)
            {
                string type = c.ClaimType.Substring(c.ClaimType.LastIndexOf('/') + 1);
                try
                {
                    if (!String.IsNullOrEmpty(c.Value))
                    {
                        if(c.Value != "NA_NULL")
                            claims.Add(type, c.Value);
                    }
                }
                catch (ArgumentException)
                {
                    //duplicated claimtype from acs and sharepoint
                }
            }
            return claims;
        }        
    }

    public class UserMap
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string ProfileProperty { get; set; }

        public string DisplayName { get; set; }
        public string B2CField { get; set; }
    }
}
