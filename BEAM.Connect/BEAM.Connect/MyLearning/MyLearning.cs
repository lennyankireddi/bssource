#define DEV
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using Microsoft.IdentityModel.Claims;
using System.Threading;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace BEAM.Connect.MyLearning
{
    [ToolboxItemAttribute(false)]
    public class MyLearning : WebPart
    {
        Token lmsToken = null;
        string lmsCookie;
        Cookie lmsRequestCookie = null;

        #region root url for lms serive
#if DEV
        string lmsRootUrl = "https://jimbeambr-stage.plateau.com/learning";
#endif
#if PROD
        string lmsRootUrl = "https://jimbeambr.plateau.com/learning";
#endif        
        #endregion

        //Treat-As=WEB
        //https://sso.beamglobal.com/B2E/saml2/lms?target=%2fsf%2flearning%3fdestUrl%3dhttps%3a%2f%2fjimbeambr-stage.plateau.com%2flearning%2fuser%2fcommon%2fviewItemDetails.do%3fcomponentTypeID%3dONLINE%26componentID%3dACS%2520Test%26revisionDate%3d1462998240000%26componentKey%3d2006%26company%3dC0017733204T1

        #region deepLink and destUrl
#if DEV
        string deepLink = "?target=%2fsf%2flearning%3fTreat-As=WEB%26destUrl%3d";
        string destUrl = "https%3a%2f%2fjimbeambr-stage.plateau.com%2flearning%2fuser%2fcommon%2fviewItemDetails.do%3fcomponentTypeID%3d{COMPONENTTYPEID}%26componentID%3d{COMPONENTID}%26revisionDate%3d{REVISIONDATE}%26componentKey%3d{COMPONENTKEY}%26company%3d" + company;
        static string company = "C0017733204T1";
#endif
#if PROD
        string deepLink = "?target=%2fsf%2flearning%3fTreat-As=WEB%26destUrl=";
        string destUrl = "https%3a%2f%2fjimbeambr.plateau.com%2flearning%2fuser%2fcommon%2fviewItemDetails.do%3fcomponentTypeID%3d{COMPONENTTYPEID}%26componentID%3d{COMPONENTID}%26revisionDate%3d{REVISIONDATE}%26componentKey%3d{COMPONENTKEY}%26company%3d" + company;
        static string company = "C0017733204P";
#endif
        #endregion

        string learningPlanApi = "/odatav4/learningPlan/v1/UserTodoLearningItems?$filter=criteria/targetUserID%20eq%20";
        string tokenApi = "/oauth-api/rest/v1/token";
        string companyId = "jimbeambr";
        string clientId = "jimbeambr";
        //protected string clientSecret = "$5$414887a4b55ddf80$93f77b90b0f5dc385567ac3f047639a8694a4fc9974a1140a4740a4904d82778";
        //protected string publicKey = "MIICLTCCAZagAwIBAgIETerWzzANBgkqhkiG9w0BAQUFADBbMQswCQYDVQQGEwJVUzELMAkGA1UECBMCVkExDzANBgNVBAcTBlZpZW5uYTEMMAoGA1UEChMDUFNMMQswCQYDVQQLEwJQRTETMBEGA1UEAxMKdHh1ZS10MzUwMDAeFw0xMTA2MDUwMTA3MjdaFw0yMTA2MDIwMTA3MjdaMFsxCzAJBgNVBAYTAlVTMQswCQYDVQQIEwJWQTEPMA0GA1UEBxMGVmllbm5hMQwwCgYDVQQKEwNQU0wxCzAJBgNVBAsTAlBFMRMwEQYDVQQDEwp0eHVlLXQzNTAwMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDIdVOlRIcSuvEgGkZZnS0SwDPJkGTQuQSQ4tn3NV5dB8XvIRuTQ7a26x9fOC/2jSo3RZVLS5CFgy+17nVyEL01aYzKX/xy6EYANKlo0nAUi2NgFZ46//Rw2dksNpX92NVFuLAQXVv6ROENfd75a0+u3uomxpjhaYIXmjo3ERKNHQIDAQABMA0GCSqGSIb3DQEBBQUAA4GBAJqU1TBQ4WaV87n2Lf/YDHVRXItJW9tTZgesmFlA/rq42aybqgTuNKPizqhJ3nklOIIGIPPAZ7rCVV4TPYz/UH/mzhbs3MAIFsnAfFT37nar1WiYifXBNuGeTAhnZxJD/qHWdZSJmeQXt7OKIHQAYv9DdfcuYBC9PfNERkwK6FPB";

        #region authorization and auth tokens
#if DEV
        protected string authorization = "amltYmVhbWJyOzhkMmIxNDUzZDgwM2NkNmNjMWM1MDQ5ZjI5MzdlMjMzODdkODllZDhiM2E3OWFkMTlhYzlmYmVhZGI1Mjk1Mjk0MGE3MTAzZjBiYjIwYzUwNjkwNWNkOTE0M2M2YTkyZA==";
        private TokenData authJson = new TokenData
        {
            grant_type = "client_credentials",
            scope = new TokenScope
            {
                userId = "WEB_SERVICES",
                companyId = "jimbeambr",
                userType = "admin",
                resourceType = "learning_public_api"
            },
            client_id = "jimbeambr",
            client_secret = "8d2b1453d803cd6cc1c5049f2937e23387d89ed8b3a79ad19ac9fbeadb52952940a7103f0bb20c506905cd9143c6a92d"
        };
#endif
#if PROD
        protected string authorization = "amltYmVhbWJyOzdhODU4OTM2YTg0NjM5ZjI1OTcwZjY0ZDBjM2VmODg1YmFlOTNkZWNmMWQ4NzJkMDFmZGY0OWNmNzVmNzQ2ZGQ0MGE3MTAzZjBiYjIwYzUwNjkwNWNkOTE0M2M2YTkyZA==";
        private TokenData authJson = new TokenData
        {
            grant_type = "client_credentials",
            scope = new TokenScope
            {
                userId = "WEB_SERVICES",
                companyId = "jimbeambr",
                userType = "admin",
                resourceType = "learning_public_api"
            },
            client_id = "jimbeambr",
            client_secret = "7a858936a84639f25970f64d0c3ef885bae93decf1d872d01fdf49cf75f746dd40a7103f0bb20c506905cd9143c6a92d"
        };

        /*{"grant_type":"client_credentials","scope":{"userId":"WEB_SERVICES","companyId":"jimbeambr","userType":"admin","resourceType":"learning_public_api"},
        "client_id":"jimbeambr","client_secret":"8d2b1453d803cd6cc1c5049f2937e23387d89ed8b3a79ad19ac9fbeadb52952940a7103f0bb20c506905cd9143c6a92d"}*/
#endif
        #endregion

        private Dictionary<string, Dictionary<string, string>> Translations = new Dictionary<string, Dictionary<string, string>>
        {
#region Translations
            { "en", new Dictionary<string, string>{ //English
                { "View More", "View More" },
                { "Assigned Courses", "Assigned Courses" },
                { "Suggested Courses", "Suggested Courses" }
                }
            },
            { "de", new Dictionary<string, string>{ //German
                { "View More", "Mehr Anzeigen" },
                { "Assigned Courses", "Zugewiesene Schulungen" },
                { "Suggested Courses", "Vorgeschlagene Schulungen" }
                }
            },
            { "fr", new Dictionary<string, string>{ //french
                { "View More", "Voir Plus" },
                { "Assigned Courses", "Cours Assignés" },
                { "Suggested Courses", "Cours Suggérés" }
                }
            },
            { "pt", new Dictionary<string, string>{ //Portuguese
                { "View More", "Exibir mais" },
                { "Assigned Courses", "Cursos Atribuídos" },
                { "Suggested Courses", "Cursos Sugeridos" }
                }
            },
            { "pt-BR", new Dictionary<string, string>{ //Brazillian Portuguese
                { "View More", "Exibir mais" },
                { "Assigned Courses", "Cursos Atribuídos" },
                { "Suggested Courses", "Cursos Sugeridos" }
                }
            },
            { "zh-HK", new Dictionary<string, string>{ //Mandarin Hong Kong
                { "View More", "查看更多" },
                { "Assigned Courses", "分配的課程" },
                { "Suggested Courses", "推薦課程" }
                }
            },
            { "zh", new Dictionary<string, string>{ //Mandarin
                { "View More", "查看更多" },
                { "Assigned Courses", "分配的課程" },
                { "Suggested Courses", "推薦課程" }
                }
            },
            { "zh-TW", new Dictionary<string, string>{ //Mandarin Taiwan
                { "View More", "進一步查看" },
                { "Assigned Courses", "分配的課程" },
                { "Suggested Courses", "推薦課程" }
                }
            },
            { "ru", new Dictionary<string, string>{ //Russian
                { "View More", "Подробнее" },
                { "Assigned Courses", "Назначенные курсы" },
                { "Suggested Courses", "Рекомендуемые курсы" }
                }
            },
            { "pl", new Dictionary<string, string>{ //Polish
                { "View More", "Zobacz Więcej" },
                { "Assigned Courses", "Przypisane Kursy" },
                { "Suggested Courses", "Sugerowane Kursy" }
                }
            },
            { "ja", new Dictionary<string, string>{ //Japanese
                { "View More", "もっと見る" },
                { "Assigned Courses", "指定コース" },
                { "Suggested Courses", "推奨コース" }
                }
            },
            { "es", new Dictionary<string, string>{ //Spanish
                { "View More", "Ver Más" },
                { "Assigned Courses", "Cursos Asignados" },
                { "Suggested Courses", "Cursos Recomendados" }
                }
            }
#endregion
        };

        protected override void CreateChildControls()
        {
            try
            {
                lmsToken = GetToken();
                if (lmsToken == null)
                    return;
                Dictionary<string, string> claims = GetClaimsForUser();
                string lang = GetCurrentLanguage();
                string roleid = (claims.ContainsKey("extensionAttribute6")) ? claims["extensionAttribute6"] : "";
                if (IsAuthenticated() && claims.ContainsKey("extensionAttribute1"))
                    BuildUI(GetCoursesToDisplay(claims["extensionAttribute1"]), lang, roleid);
                else if (!claims.ContainsKey("extensionAttribute1"))
                    BuildUI(null, "", "", true);
            }
            catch(Exception ex)
            {
                this.Controls.Add(new LiteralControl("<div class='error'><div>Error in LMS web part</div><div class='msg'>" + ex.Message + "</div><div class='trace' style='display:none'>" + ex.StackTrace + "</div><div class='innerException' style='display:none'>" + ex.InnerException + "</div></div>"));
            }
        }

        internal string GetCurrentLanguage()
        {
            return Thread.CurrentThread.CurrentUICulture.Name;
        }

        internal void BuildUI(Courses courses, string Language, string RoleID, bool noUserId = false)
        {
            int recommendFound = 0;
            int requiredFound = 0;

#if DEV
            string sso = "https://sso.beamglobal.com/b2e/saml2/lms";
            if (RoleID != "Internal_USER")
                sso = "https://sso.beamglobal.com/b2c/saml2/lms";
#endif

#if PROD
            string sso = "https://sso.beamglobal.com/apps/b2e/saml2/lms";
            if (RoleID != "Internal_USER")
                sso = "https://sso.beamglobal.com/apps/b2c/saml2/lms";
#endif
            string recommended = "";
            string required = "";
            if (!noUserId)
            {
                foreach (Course c in courses.value)
                {
                    if (recommendFound == 2 && requiredFound == 2)
                        break;
                    if (!string.IsNullOrEmpty(c.requiredDate) && requiredFound < 2)
                    {
                        requiredFound++;
                        required += GetCourseHtml(c, sso);
                    }
                    else if (recommendFound < 2)
                    {
                        recommendFound++;
                        recommended += GetCourseHtml(c, sso);
                    }
                }
            }
            else
                required += "<div class='course'>" +
                    "<h2>Missing LMS User</h2>" +
                "</div>";
            string html =
                @"<div class='myLearning'>";
            //https://sso.beamglobal.com/B2E/saml2/lms?target=%2fsf%2flearning%26company%3dC0017733204T1 
            if (requiredFound > 0)
                html +=
                    @"<div class='assignedCourses'>
		                <h2>" + GetTranslation(Language, "Assigned Courses") + @"</h2>" +
                            required +
                        @"<div class='viewMore course'> 
			                <a target='_blank' href='" + sso + @"?target=%2fsf%2flearning%3fcompany%3d" + company + @"'>
				                <span>" + GetTranslation(Language, "View More") + @"</span>
				                <i class='fa fa-angle-right icon-angle-right'></i>
			                </a>
		                </div>
	                </div>";
            html +=
                @"<div class='trendingCourses'> 
		            <h2>" + GetTranslation(Language, "Suggested Courses") + @"</h2>" +
                    recommended +
                    @"<div class='viewMore course'> 
			            <a target='_blank' href='" + sso + @"?target=%2fsf%2flearning%3fcompany%3d" + company + @"'>
				            <span>" + GetTranslation(Language, "View More") + @"</span>
				            <i class='fa fa-angle-right icon-angle-right'></i>
			            </a>
		            </div> 
	            </div>
            </div>";
            this.Controls.Add(new LiteralControl(html));
        }

        internal string GetTranslation(string Language, string Element)
        {
            string parentlang = (Language.Contains("-")) ? Language.Split(new char[] { '-' })[0] : "";
            Language = (Translations.ContainsKey(Language)) ? Language : (Translations.ContainsKey(parentlang)) ? parentlang : "en"; //check if we have the language with an explicit translation. If not get the root language or default to english if we are missing the translation
            return Translations[Language][Element];
        }

        internal string GetCourseHtml(Course course, string sso)
        {
            return
                "<div class='course'>" +
                    "<h2><a target='_blank' href='" + sso + GetItemUrl(course) + "%26company%3d" + company + "' target='_blank'>" + course.title + "</a></h2>" +
                "</div>";
        }

        internal string GetItemUrl(Course item)
        {
            return deepLink + Uri.EscapeUriString(destUrl.Replace("{COMPONENTTYPEID}", item.componentTypeID).Replace("{COMPONENTID}", item.componentID).Replace("{REVISIONDATE}", item.revisionDate).Replace("{COMPONENTKEY}", item.componentKey));
        }

        internal Token GetToken()
        {
            string response = "";
            try
            {
                WebRequest req = WebRequest.Create(lmsRootUrl + tokenApi);
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true; //ignores ssl errors
                req.Headers.Add("X-PINGARUNER", "pingpong");
                ((HttpWebRequest)req).Accept = "application/json;odata=verbose";
                //req.Headers.Add("Accept", "application/json;odata=verbose");
                req.Headers.Add("Authorization", authorization);
                req.Method = "POST";

                req.ContentType = "application/json;charset=utf-8";
                string authBody = JsonConvert.SerializeObject(authJson);
                byte[] bA = Encoding.UTF8.GetBytes(authBody);
                req.ContentLength = bA.Length;
                Stream s = req.GetRequestStream();
                s.Write(bA, 0, bA.Length);
                s.Close();
                WebResponse res = req.GetResponse();
                s = res.GetResponseStream();
                StreamReader reader = new StreamReader(s);
                response = reader.ReadToEnd();
#if DEBUG
                this.Controls.Add(new LiteralControl("<div style='display:none'><h3>Token JSON</h3><div>" + response + "</div></div>"));
#endif
                lmsCookie = res.Headers["Set-Cookie"];
                lmsRequestCookie = new Cookie("X-CSRF-Token", lmsCookie);
#if DEBUG
                this.Controls.Add(new LiteralControl("<div style='display:none'><h3>Token Cookie</h3><div>" + lmsCookie + "</div></div>"));
#endif
                reader.Close();
                s.Close();
                res.Close();
                return JsonConvert.DeserializeObject<Token>(response);
            }
            catch (Exception ex)
            {
                this.Controls.Add(new LiteralControl("<div>Unable to access learning management system with current users information</div><div>" + ex.Message + "</div><div>" + response + "</div>"));
                return null;
            }
        }

        internal Courses GetCoursesToDisplay(string SuccessFactorsUserID)
        {
            Courses courses = new Courses();
            try
            {
                //https://jimbeambr-stage.plateau.com/learning/odatav4/learningPlan/v1/UserTodoLearningItems?$filter=criteria/targetUserID eq '50098'
                //WebRequest req = WebRequest.Create(lmsRootUrl + learningPlanApi + "'" + SuccessFactorsUserID + "'");
                WebRequest req = WebRequest.Create(lmsRootUrl + "/odatav4/learningPlan/v1/UserTodoLearningItems?$filter=criteria/targetUserID%20eq%20'" + SuccessFactorsUserID + "'");
                req.Headers.Add("Cookie", lmsCookie);
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true; //ignores ssl errors
                //((HttpWebRequest)req).Accept = "application/json;odata=verbose";
                /*if(((HttpWebRequest)req).CookieContainer == null)
                    ((HttpWebRequest)req).CookieContainer = new CookieContainer();
                ((HttpWebRequest)req).CookieContainer.Add(new Cookie("Cookie", lmsCookie));*/
                req.Headers.Add("Authorization", "Bearer " + lmsToken.access_token);
                req.Method = "GET";
                //req.ContentType = "application/json;charset=utf-8";
                WebResponse res = req.GetResponse();
                Stream s = res.GetResponseStream();
                StreamReader reader = new StreamReader(s);
                string response = reader.ReadToEnd();
                reader.Close();
                res.Close();
#if DEBUG
                this.Controls.Add(new LiteralControl("<div style='display:none'><h3>Courses JSON</h3><div>" + response + "</div></div>"));
#endif
                courses = JsonConvert.DeserializeObject<Courses>(response);
                courses.value.Sort(delegate (Course x, Course y)
                {
                    if (string.IsNullOrEmpty(x.requiredDate) && string.IsNullOrEmpty(y.requiredDate)) return 0;
                    else if (string.IsNullOrEmpty(x.requiredDate)) return -1;
                    else if (string.IsNullOrEmpty(y.requiredDate)) return 1;
                    else return x.requiredDate.CompareTo(y.requiredDate);
                });
            }
            catch (WebException wex)
            {
                courses.value = new List<Course>();
#if DEBUG
                this.Controls.Add(new LiteralControl("<div style='display:none'><div>Error: " + wex.Message + "</div><div>Trace: " + wex.StackTrace + "</div><div>URL: " + lmsRootUrl + "/odatav4/learningPlan/v1/UserTodoLearningItems?$filter=criteria/targetUserID eq '" + SuccessFactorsUserID + "'</div><div>Token: " + lmsCookie + "</div><div>Authorization: " + authorization + "</div></div>"));
                this.Controls.Add(new LiteralControl("<div style='display:none'><div>lmsToken.access_token: " + lmsToken.access_token + "</div><div>lmsToken.expiresIn: " + lmsToken.expires_in + "</div><div>lmsToken.token_type: " + lmsToken.token_type + "</div></div>"));
#endif
            }
            return courses;
        }

        internal bool IsAuthenticated()
        {
            if (lmsToken == null || string.IsNullOrEmpty(lmsCookie))
                return false;
            return true;
        }

        internal Dictionary<string, string> GetClaimsForUser()
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();
            IClaimsPrincipal cp = (IClaimsPrincipal)Thread.CurrentPrincipal;
            foreach (var c in cp.Identities[0].Claims)
            {
                string type = c.ClaimType.Substring(c.ClaimType.LastIndexOf('/') + 1);
                try
                {
                    if (type.StartsWith("extensionAttribute") || (type == "identityprovider" && c.Value == "windows"))
                        claims.Add(type, c.Value.Replace(" ", "_"));
                }
                catch (ArgumentException)
                {
                    //duplicated claimtype from acs and sharepoint
                }
            }
            return claims;
        }
    }

    public class TokenData
    {
        public string grant_type { get; set; }
        public string client_id { get; set; }
        public TokenScope scope { get; set; }
        public string client_secret { get; set; }
    }

    public class Courses
    {
        public List<Course> value { get; set; }
    }

    public class TokenScope
    {
        public string userId { get; set; }
        public string companyId { get; set; }
        public string userType { get; set; }
        public string resourceType { get; set; }
    }

    public class Token
    {
        public string issuedAt { get; set; }
        public string expires_in { get; set; }
        public string issuedFor { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
    }

    public class Course
    {
        public string sku { get; set; }
        public string cpnt_classification { get; set; }
        public bool isUserRequestsEnabled { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public bool? status { get; set; }
        public string userID { get; set; }
        public string componentTypeID { get; set; }
        public string componentTypeDesc { get; set; }
        public string componentID { get; set; }
        public string componentKey { get; set; }
        public string componentLength { get; set; }
        public string contactHours { get; set; }
        public string creditHours { get; set; }
        public string cpeHours { get; set; }
        public string revisionDate { get; set; }
        public string assignedDate { get; set; }
        public bool? availableNewRevision { get; set; }
        public string revisionNumber { get; set; }
        public string requiredDate { get; set; }
        public string daysRemaining { get; set; }
        public string addUser { get; set; }
        public string addUserName { get; set; }
        public string addUserTypeLabelID { get; set; }
        public string orderItemID { get; set; }
        public string usedOrderTicketNumber { get; set; }
        public string usedOrderTicketSequence { get; set; }
        public bool? onlineLaunched { get; set; }
        public string origin { get; set; }
        public string cdpGoalID { get; set; }
        public string seqNumber { get; set; }
        public string scheduleID { get; set; }
        public string qualificationID { get; set; }
        public string rootQualificationID { get; set; }
        public string qualTitle { get; set; }
        public bool? isRequired { get; set; }
        public string orderItemStatusTypeID { get; set; }
        public string showInCatalog { get; set; }
        public string requirementTypeDescription { get; set; }
        public string requirementTypeId { get; set; }
        public string hasOnlinePart { get; set; }
        public string criteria { get; set; }
    }
}
