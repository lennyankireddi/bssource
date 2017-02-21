#define PROD
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Web.UI;
using System.Linq;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using System.Threading;
using Microsoft.IdentityModel.Claims;
using System.Web;
using Facet.Combinatorics;
using Newtonsoft.Json;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint.Administration.Claims;

namespace BEAM.Connect.MapClaimsToGroups
{
    [ToolboxItemAttribute(false)]
    public class MapClaimsToGroups : WebPart
    {
#if DEV
        string defaultUser = "idmresc\\spext_setup";
#endif
#if PROD
        string defaultUser = "beamext\\shpntprd";
#endif
        UserProfile currentProfile = null;
        HttpCookie _claimsCookie = GetClaimsCookie();
        protected override void CreateChildControls()
        {
            try
            {
                //emit claims as JSON
                Dictionary<string, string> dicClaimsJson = GetClaimsForJSON();
                string claimsJson = JsonConvert.SerializeObject(dicClaimsJson);
#if DEBUG
                this.Controls.Add(new LiteralControl("<div>Claims JSON:" + claimsJson + "</div>"));
#endif
                this.Controls.Add(new LiteralControl("<script> var claimsJSONMap = " + claimsJson + " </script>"));

                //Updte permissions and profile if claims changed.
#if DEBUG
                this.Controls.Add(new LiteralControl("<div>Current Claims:</div><ul>"));
#endif
                Dictionary<string, string> claims = GetClaimsForUser(false);
                if (_claimsCookie != null)
                    claims = CompareChanges(claims, JsonConvert.DeserializeObject<Dictionary<string, string>>(_claimsCookie.Value));
                //TODO: Re-enable
                /*if (claims.Count < 1) //ignore users with no claims, and windows users
                    return;*/
                currentProfile = GetCurrentProfile();
                List<string> values = new List<string>();
                string claimUP = "";
                foreach (string k in claims.Keys)
                {
#if DEBUG
                    this.Controls.Add(new LiteralControl("<li>" + k + " - " + claims[k] + "</li>"));
#endif
                    claimUP += k + "-" + claims[k] + "|";
                    values.Add(claims[k]);
                }

                if (ClaimsChanged(claimUP))
                {
#if DEBUG
                    this.Controls.Add(new LiteralControl("</ul><div>Claim Permutations</div><ul>"));
#endif
                    List<string> allcombos = new List<string>();
                    for (int i = 2; i <= values.Count - 1; i++)
                    {
                        var combis = new Combinations<string>(values, i, GenerateOption.WithoutRepetition);
                        allcombos.AddRange(combis.Select(c => string.Join(" ", c)));
                    }
#if DEBUG
                    foreach (string c in allcombos)
                    {
                        this.Controls.Add(new LiteralControl("<li>" + c + "</li>"));
                    }
                    this.Controls.Add(new LiteralControl("</ul>"));
#endif

                    //verify permissions
                    VerifyPermissions(allcombos);
                    UpdateProfile(claimUP);
                }
            }
            catch(Exception ex)
            {
                this.Controls.Add(new LiteralControl("<div class='error'><div>Error in mapping incoming claims to sharepoint groups</div><div class='msg'>" + ex.Message + "</div><div class='trace' style='display:none'>" + ex.StackTrace + "</div><div class='innerException' style='display:none'>" + ex.InnerException + "</div></div>"));
            }
        }

        internal void VerifyPermissions(List<string> permutations)
        {
            string currUserName = SPContext.Current.Web.CurrentUser.LoginName;
            string url = SPContext.Current.Site.Url;
            try
            {
                
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    // need to convert to claim due to us running against claims enabled web app
                    SPClaimProviderManager cpm = SPClaimProviderManager.Local;
                    SPClaim adminClaim = cpm.ConvertIdentifierToClaim(defaultUser, SPIdentifierTypes.WindowsSamAccountName);

                    using (SPSite s = new SPSite(url))
                    {
                        using (SPWeb w = s.RootWeb)
                        {
                            w.AllowUnsafeUpdates = true;
                            UpdateUserInformationList(w, GetClaimsForUser(true));
                            List<string> missingGroups;
                            List<SPGroup> groups = PermutationGroups(permutations, w, out missingGroups);
                            if (groups.Count != permutations.Count)
                            {
                                SPUser admin = w.EnsureUser(adminClaim.ToEncodedString());
                                groups.AddRange(CreateMissingGroups(missingGroups, w, admin));
                            }
                            SPUser cUser = w.EnsureUser(currUserName);
                            SPGroupCollection gUser = cUser.Groups;
                            foreach (SPGroup g in groups)
                            {
                                bool isAlreadyMember = false;
                                foreach(SPGroup gu in gUser)
                                {
                                    if (gu.Name == g.Name)
                                    {
                                        isAlreadyMember = true;
                                        break;
                                    }
                                }
                                if (!isAlreadyMember)
                                {
                                    g.AddUser(cUser);
#if DEBUG
                                    this.Controls.Add(new LiteralControl("<div>Adding user to group:" + g.Name + "</div>"));
#endif
                                }
#if DEBUG
                                this.Controls.Add(new LiteralControl("<div>Already a member of group: " + g.Name + "</div>"));
#endif
                            }
                            w.AllowUnsafeUpdates = false;
                        }
                    }
                });
            }
            catch(Exception ex)
            {
                this.Controls.Add(new LiteralControl("<div class='Error' style='display:none'>" + ex.Message + "</div><div>" + ex.StackTrace + "</div>"));
            }
        }

        internal List<SPGroup> CreateMissingGroups(List<string> groupNames, SPWeb web, SPUser groupOwner)
        {
            List<SPGroup> missingGroups = new List<SPGroup>();

            //Leave this as manual for now
            /*foreach(string s in groupNames)
            {
                if (!s.StartsWith("~"))
                    missingGroups.Add(CreateGroup(web, s, groupOwner));
            }*/
            return missingGroups;
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

        internal List<SPGroup> PermutationGroups(List<string> combinations, SPWeb web, out List<string> updatedCombinations)
        {
            char[] splitter = new char[] { ' ' };
            List<SPGroup> groups = new List<SPGroup>();
            foreach(SPGroup g in web.SiteGroups)
            {
                /*if (combinations.Contains(g.Name))
                    groups.Add(g);
                else
                {*/
                for (int i = 0; i < combinations.Count; i++)
                {
                    if (g.Name == combinations[i])
                    {
                        groups.Add(g);
                        combinations[i] = "~" + combinations[i];
#if DEBUG
                        this.Controls.Add(new LiteralControl("<div>Found existing group: " + g.Name + "</div>"));
#endif
                    }
                    else if (g.Name.Length == combinations[i].Length)
                    {
                        if (CompareSegments(g.Name.Split(splitter, StringSplitOptions.RemoveEmptyEntries), combinations[i].Split(splitter, StringSplitOptions.RemoveEmptyEntries)))
                        {
                            groups.Add(g);
                            combinations[i] = "~" + combinations[i];
#if DEBUG
                            this.Controls.Add(new LiteralControl("<div>Found existing group: " + g.Name + "</div>"));
#endif
                        }
                    }
                }
                //}
            }
            updatedCombinations = combinations;
            return groups;
        }

        internal SPGroup CreateGroup(SPWeb Web, string Name, SPUser adminUser)
        {
#if DEBUG
            this.Controls.Add(new LiteralControl("<div>Creating group: " + Name + "</div>"));
#endif
            Web.SiteGroups.Add(Name, adminUser, adminUser, "");
            return Web.SiteGroups[Name];
        }

        internal bool CompareSegments(string[] segment1, string[] segment2)
        {
            int matching = 0;
            foreach (string s in segment1)
            {
                foreach(string s2 in segment2)
                {
                    if (s == s2)
                        matching++;
                }
            }
            if (matching == segment1.Length)
                return true;
            return false;
        }

        internal bool ClaimsChanged(string currentClaims)
        {
            //return true; //REMOVE THIS
#if DEBUG
            this.Controls.Add(new LiteralControl("<div>Current Claims - " + currentClaims + "</div>"));
#endif
            try
            {
                HttpCookie current = GetCookie("ConnectClaims");
                if (current != null)
                {
                    if (current.Value == currentClaims)
                        return false;
                }
            }
            catch (Exception) { }
            if(currentProfile != null)
                if (currentProfile["UserClaims"].Value as string == currentClaims)
                    return false;
            return true;
        }

        internal HttpCookie GetCookie(string Name)
        {
            foreach(HttpCookie c in HttpContext.Current.Request.Cookies)
            {
                if (c.Name == Name)
                    return c;
            }
            return null;
        }

        internal void UpdateUserInformationList(SPWeb w, Dictionary<string, string> AllClaims)
        {
            try
            {
                if (AllClaims["identityprovider"] == "windows")
                    return;
                SPList uil = w.SiteUserInfoList;
                SPListItem user = uil.GetItemById(SPContext.Current.Web.CurrentUser.ID);
#if DEBUG
                foreach(SPField f in user.Fields)
                {
                    try
                    {
                        this.Controls.Add(new LiteralControl("<div>" + f.InternalName + " - " + user[f.InternalName].ToString() + "</div>"));
                    }
                    catch(Exception) { }
                }
#endif
                string fullName = "";
                if (AllClaims.ContainsKey("givenname") && AllClaims.ContainsKey("surname"))
                    fullName = AllClaims["givenname"] + " " + AllClaims["surname"];
                else if (AllClaims.ContainsKey("givenname"))
                    fullName = AllClaims["givenname"];
                else if (AllClaims.ContainsKey("surname"))
                    fullName = AllClaims["surname"];
                user["Title"] = fullName;
                if (AllClaims.ContainsKey("division"))
                    user["Department"] = AllClaims["division"];
                if (AllClaims.ContainsKey("extensionattribute6"))
                    user["JobTitle"] = AllClaims["extensionattribute6"];
                user.Update();
            }
            catch(Exception ex)
            {
                string err = ex.Message;
            }
        }

        internal void UpdateProfile(string newClaims)
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie("ConnectClaims")
            {
                Value = newClaims
            });
            try
            {
                Dictionary<string, string> allClaims = GetClaimsForUser(true);
                if (currentProfile != null)
                {
                    SPContext.Current.Web.AllowUnsafeUpdates = true;
                    currentProfile["UserClaims"].Value = newClaims;
                    string fullName = "";
                    if (allClaims.ContainsKey("givenname") && allClaims.ContainsKey("surname"))
                        fullName = allClaims["givenname"] + " " + allClaims["surname"];
                    else if (allClaims.ContainsKey("givenname"))
                        fullName = allClaims["givenname"];
                    else if (allClaims.ContainsKey("surname"))
                        fullName = allClaims["surname"];
                    currentProfile["PreferredName"].Value = fullName;
                    foreach(string k in allClaims.Keys)
                    {
                        if (k == "givenname")
                            currentProfile["FirstName"].Value = allClaims[k];
                        else if(k == "surname")
                            currentProfile["LastName"].Value = allClaims[k];
                        else if (k == "country")
                            currentProfile["Office"].Value = allClaims[k];
                        else if (k == "state")
                            currentProfile["SPS-Location"].Value = allClaims[k];
                    }
                    currentProfile.Commit();
                    SPContext.Current.Web.AllowUnsafeUpdates = false;
                }
            }
            catch(Exception ex)
            {
#if DEBUG
                this.Controls.Add(new LiteralControl("<div>Unable to update profile</div><div>" + ex.Message + "</div><div>" + ex.StackTrace + "</div><div>" + ex.InnerException + "</div>"));
#endif
            }
        }

        internal UserProfile GetCurrentProfile()
        {
            try
            {
                SPServiceContext ctx = SPServiceContext.Current;
                UserProfileManager upm = new UserProfileManager(ctx);
                return upm.GetUserProfile(true);
            }
            catch (Exception) { }
            return null;
        }

        internal Dictionary<string, string> GetClaimsForUser(bool GetAll)
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();
            IClaimsPrincipal cp = (IClaimsPrincipal)Thread.CurrentPrincipal;
            bool ignoreUser = false;
            foreach(var c in cp.Identities[0].Claims)
            {
                string type = c.ClaimType.Substring(c.ClaimType.LastIndexOf('/') + 1).ToLower();
                if (type == "identityprovider" && c.Value == "windows")
                    ignoreUser = true;
                try
                {
                    if(GetAll)
                        claims.Add(type, c.Value.Replace(" ", "_").Replace("/", "").Replace("'", "").Replace(",,", "").Replace("#", "").Replace("&", "_").Replace("%", "").Replace("*", "").Replace(";", "").Replace("@", "").Replace(",", " ").Replace("\"", ""));
                    else if ((type == "extensionattribute5" /* Region */ || 
                        type == "extensionattribute6" /* SuccessFactorsRoleID */ ||
                        type == "employer"  || 
                        type == "country" || 
                        type == "state") && 
                        (c.Value != "NA_NULL" || c.Value != ""))
                        claims.Add(type, c.Value.Replace(" ", "_").Replace("/", "").Replace("'", "").Replace(",,", "").Replace("#", "").Replace("&", "_").Replace("%", "").Replace("*", "").Replace(";", "").Replace("@", "").Replace(",", " ").Replace("\"", ""));
                }
                catch(ArgumentException)
                {
                    //duplicated claimtype from acs and sharepoint
                }
            }
            if (ignoreUser)
                claims.Clear();
            return claims;
        }

        internal Dictionary<string, string> CompareChanges(Dictionary<string, string> Claims, Dictionary<string, string> PendingUpdates)
        {
            for (int i = 0; i < Claims.Count; i++)
            {
                switch (Claims.Keys.ElementAt(i))
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
            foreach (string pk in PendingUpdates.Keys)
            {
                switch (pk)
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

        internal Dictionary<string, string> GetClaimsForJSON()
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();
            IClaimsPrincipal cp = (IClaimsPrincipal)Thread.CurrentPrincipal;
            foreach (var c in cp.Identities[0].Claims)
            {
                string type = c.ClaimType.Substring(c.ClaimType.LastIndexOf('/') + 1).ToLower();
                try
                {
                    if ((type == "emailaddress" ||
                        type == "extensionattribute2" /* CommercialRegion */ ||
                        type == "extensionattribute3" /* Area */ ||
                        type == "extensionattribute4" /* OnOffPremise */ ||
                        type == "extensionattribute5" /* Region */ ||
                        type == "extensionattribute6" /* SuccessFactorsRoleID */ ||
                        type == "extensionattribute8" /* Division */ ||
                        type == "givenname" /* FirstName */||
                        type == "surname" /* LastName */||
                        type == "employer" ||
                        type == "country" ||
                        type == "state") &&
                        (c.Value != "NA_NULL" || c.Value != ""))
                    {
                        if (type == "extensionattribute2") { type = "CommercialRegion"; }
                        if (type == "extensionattribute3") { type = "Area"; }
                        if (type == "extensionattribute4") { type = "OnOffPremise"; }
                        if (type == "extensionattribute5") { type = "Region"; }
                        if (type == "extensionattribute6") { type = "SuccessFactorsRoleID"; }
                        if (type == "extensionattribute8") { type = "Division"; }
                        if (type == "givenname") { type = "FirstName"; }
                        if (type == "surname") { type = "LastName"; }
                        if(c.Value.ToUpper() != "NA_NULL")
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
}
