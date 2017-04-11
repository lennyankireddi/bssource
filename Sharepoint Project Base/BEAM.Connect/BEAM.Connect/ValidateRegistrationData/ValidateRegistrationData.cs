#define PROD
using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Threading;
using GraphApi;
using Microsoft.IdentityModel.Claims;
using Newtonsoft.Json;
using Microsoft.Office.Server.UserProfiles;

namespace BEAM.Connect.ValidateRegistrationData
{
    [ToolboxItemAttribute(false)]
    public class ValidateRegistrationData : WebPart
    {
        //UserProfile uP = null;
        bool needProfileUpdate = false;
        Panel content;
        Button submitBtn;
        string upn = "";
        HttpCookie _claimsCookie = GetClaimsCookie();
        protected override void CreateChildControls()
        {
            try
            {
                List<UserMap> map = GetRequiredInformation();
                Dictionary<string, string> claims = GetClaimsForUser();
                if (claims["identityprovider"] == "windows")
                    return;
                if (claims.ContainsKey("extensionAttribute6"))
                    if (claims["extensionAttribute6"] == "Internal_USER")
                        return;
                if (claims.ContainsKey("upn"))
                    upn = claims["upn"];
                //uP = GetCurrentProfile();

                bool showForm = false;

                for (int i = 0; i < map.Count; i++)
                {
                    UserMap n = null;
                    if (NeedValue(map[i], claims, out n))
                        showForm = true;
                    else
                        map[i] = n;
                }

                if (showForm && !HttpContext.Current.Request.Url.AbsolutePath.ToLower().Contains("manageprofile.aspx"))
                {
                    HttpResponse res = HttpContext.Current.Response;
                    res.Redirect(SPContext.Current.Site.Url + "/Pages/ManageProfile.aspx?RequiredFields=true");

                    /*content = new Panel();
                    content.CssClass = "RegistrationOverlay";
                    submitBtn = new Button();
                    submitBtn.CssClass = "btn";
                    submitBtn.Text = "Update";
                    submitBtn.UseSubmitBehavior = true;
                    submitBtn.Click += SubmitBtn_Click;
                    content.Controls.Add(new LiteralControl("<div class='registrationForm'><h1>Complete Registration Process</h1><div class='form'>"));
                    ShowForm(map);
                    content.Controls.Add(submitBtn);
                    content.Controls.Add(new LiteralControl("</div></div>"));
                    this.Controls.Add(content);*/
                }
            }
            catch(Exception ex)
            {
                this.Controls.Add(new LiteralControl("<div class='error'><div>Error in validation of required attributes</div><div class='msg'>" + ex.Message + "</div><div class='trace' style='display:none'>" + ex.StackTrace + "</div><div class='innerException' style='display:none'>" + ex.InnerException + "</div></div>"));
            }
        }

        internal static HttpCookie GetClaimsCookie()
        {
            HttpCookieCollection cookies = HttpContext.Current.Request.Cookies;
            for (int i = 0; i < cookies.Count; i++)
            {
                if (cookies[i].Name == "claims")
                    return cookies[i];
            }
            return null;
        }

        private void SubmitBtn_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> updates = new Dictionary<string, string>();
            TextBox field;
            foreach(Control c in content.Controls)
            {
                if(c is TextBox)
                {
                    field = c as TextBox;
                    //UpdateProfile(field.Attributes["Profile"], field.Text);
                    updates.Add(field.Attributes["B2C"], field.Text);
                }
            }
            UpdateB2C(updates, upn);
        }

        internal void ShowForm(List<UserMap> Mapping)
        {
            TextBox field;
            foreach(UserMap m in Mapping)
            {
                if (!String.IsNullOrEmpty(m.ClaimValue))
                    continue;
                field = new TextBox();
                field.Attributes.Add("B2C", m.B2CField);
                field.Attributes.Add("Profile", m.ProfileProperty);
                field.Attributes.Add("required", "required");
                content.Controls.Add(new LiteralControl("<div class='row'><span class='label'>" + m.DisplayName + "</span><span class='field'>"));
                content.Controls.Add(field);
                content.Controls.Add(new LiteralControl("</span></div>"));
            }
        }

        internal List<UserMap> GetRequiredInformation()
        {
            List<UserMap> mapping = new List<UserMap>();
            foreach(SPListItem i in SPContext.Current.Site.RootWeb.Lists["RequiredInfo"].Items)
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

        internal bool NeedValue(UserMap map, Dictionary<string, string> currentClaims, out UserMap CurrentValue)
        {
            CurrentValue = map;
#if DEBUG
            if (currentClaims.ContainsKey(map.ClaimType))
            {
                this.Controls.Add(new LiteralControl("<div><span>Have data for: " + map.ClaimType + "</span><span>-</span><span>" + currentClaims[map.ClaimType] + "</span></div>"));
            }
            else
            {
                this.Controls.Add(new LiteralControl("<div>Missing data for: " + map.ClaimType + "</div>"));
            }
#endif
            /*if (String.IsNullOrEmpty(uP[map.ProfileProperty].Value as string))
            {*/
            if (currentClaims.ContainsKey(map.ClaimType))
            {
                //UpdateProfile(map.ProfileProperty, currentClaims[map.ClaimType]);
                CurrentValue.ClaimValue = currentClaims[map.ClaimType];
                return false;
            }
            else if (_claimsCookie != null)
            {
                string lower = _claimsCookie.Value.ToLower();
                if (lower.Contains(map.ClaimType.ToLower()) || lower.Contains(map.B2CField.ToLower()) || lower.Contains(map.B2CField.Replace("a67c327875c34024bd4523a3d66619ba", "be6dc6c96b4c411780751b4231962926").ToLower()))
                    return false;
                else
                    return true;
            }
            else
                return true;
            /*}
            else
                CurrentValue.ClaimValue = uP[map.ProfileProperty].Value as string;*/
            return false;
        }

        /*internal void UpdateProfile(string ProfileAttribute, string Value)
        {
            uP[ProfileAttribute].Value = Value;
            needProfileUpdate = true;
        }*/

        internal Dictionary<string, string> GetClaimsForUser()
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();
            IClaimsPrincipal cp = (IClaimsPrincipal)Thread.CurrentPrincipal;
            foreach (var c in cp.Identities[0].Claims)
            {
                string type = c.ClaimType.Substring(c.ClaimType.LastIndexOf('/') + 1);
                try
                {
                    if(!String.IsNullOrEmpty(c.Value))
                        if(c.Value.ToUpper() != "NA_NULL")
                            claims.Add(type, c.Value.Replace(" ", "_"));
                }
                catch (ArgumentException)
                {
                    //duplicated claimtype from acs and sharepoint
                }
            }
            return claims;
        }

        internal UserProfile GetCurrentProfile()
        {
            SPServiceContext ctx = SPServiceContext.Current;
            UserProfileManager upm = new UserProfileManager(ctx);
            return upm.GetUserProfile(true);
        }

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
