#define PROD
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Net;

namespace GraphApi
{
    public class AADUser
    {
        private string Tenant { get; set; }
        private string ClientID { get; set; }
        private string ClientSecret { get; set; }
        private B2CGraphClient Client = null;
        public AADUser(string tenant, string clientid, string clientsecret)
        {
            this.Tenant = tenant;
            this.ClientID = clientid;
            this.ClientSecret = clientsecret;
            Client = new B2CGraphClient(ClientID, ClientSecret, Tenant);
        }
        
        public B2CUsers GetUsers(string Filter = "", bool AllUsers = false)
        {
            Guid temp;
            B2CUsers users = new B2CUsers();
            string results;
            if (AllUsers)
                results = Client.GetAllUsers(null).Result;
            else if (Guid.TryParse(Filter, out temp))
                results = Client.GetUserByObjectId(Filter).Result;
            else
                results = Client.GetAllUsers(Filter).Result;
//#if DEBUG
  //          Console.WriteLine(results);
            /* Example Response: is in AllUsers.json */
//#endif
            if (!Guid.TryParse(Filter, out temp))
                return JsonConvert.DeserializeObject<B2CUsers>(results);
            else
            {
                users.value = new List<B2CUser>();
                users.value.Add(JsonConvert.DeserializeObject<B2CUser>(results));
                return users;
            }
        }

        public B2CUser CreateUser(B2CNewUser NewUser)
        {
/*#if DEBUG
            Console.WriteLine(JsonConvert.SerializeObject(NewUser));
#endif*/
            WebRequest req = WebRequest.Create(Globals.aadGraphEndpoint + Tenant + "/users?" + Globals.aadGraphVersion);
            req.Headers.Add("Authorization", "Bearer " + Client.authContext.AcquireToken(Globals.aadGraphResourceId, Client.credential).AccessToken);
/*#if DEBUG
            string val = req.Headers["Authorization"];
            var blah = "";
#endif*/
            req.ContentType = "application/json";
            req.Method = "POST";
            string body = JsonConvert.SerializeObject(NewUser, Formatting.None,new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
#if PROD
            body = body.Replace("a67c327875c34024bd4523a3d66619ba", "be6dc6c96b4c411780751b4231962926");
#endif
            byte[] bA = Encoding.UTF8.GetBytes(body);
            req.ContentLength = bA.Length;
            try
            {
                Stream s = req.GetRequestStream();
                s.Write(bA, 0, bA.Length);
                s.Close();
                WebResponse res = req.GetResponse();
                s = res.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                string response = sr.ReadToEnd(); 
                return JsonConvert.DeserializeObject<B2CUser>(response.Replace("be6dc6c96b4c411780751b4231962926", "a67c327875c34024bd4523a3d66619ba"));
            }
            catch(Exception ex)
            {
                B2CUser u = new B2CUser
                {
                    userType = "Error", mail = body, displayName = ex.Message, city = ex.StackTrace
                };
                return u;
            }
            //return JsonConvert.DeserializeObject<B2CUser>(Client.CreateUser(JsonConvert.SerializeObject(NewUser)).Result);
        }
    }

    public class B2CAttribute
    {
        public string DisplayName { get; set; }
        public string GraphName { get; set; }
        public object Value { get; set; }
        public string DisplayType { get; set; }
    }

    public class SignInName
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class PasswordProfile
    {
        public string password { get; set; }
        public bool forceChangePasswordNextLogin = false;
    }

    public class B2CUsers
    {
        public string odatametadata { get; set; }
        public List<B2CUser> value { get; set; }
    }

    public class B2CNewUser
    {
        public bool accountEnabled { get; set; } //false
        public SignInName[] signInNames { get; set; }
        public string displayName { get; set; }
        public string city { get; set; }
        public string creationType { get; set; } //LocalAccount
        public string department { get; set; }
        public string facsimileTelephoneNumber { get; set; }
        public string givenName { get; set; }
        public string jobTitle { get; set; }
        public string mailNickname { get; set; }
        public string mobile { get; set; }
        public string passwordPolicies { get; set; }
        public PasswordProfile passwordProfile { get; set; }
        public string physicalDeliveryOfficeName { get; set; }
        public string postalCode { get; set; }
        public string streetAddress { get; set; }
        public string surname { get; set; }
        public string telephoneNumber { get; set; }
        public string usageLocation { get; set; }
        public string companyName { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string userType { get; set; } //member
//#if DEV
        public string extension_a67c327875c34024bd4523a3d66619ba_telephoneNumber { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_Area { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_BeamSuntorySponsor { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_Birthdate { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_CommercialRegion { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_ConnectToolboxURL { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_Email { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_Employer { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_Region { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsID { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsRoleID { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_UserType { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_Division { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_State { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_Country { get; set; }
        public string extension_a67c327875c34024bd4523a3d66619ba_TradeEmployer { get; set; }
//#endif
/*#if PROD
        public string extension_be6dc6c96b4c411780751b4231962926_telephoneNumber { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_Area { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_BeamSuntorySponsor { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_Birthdate { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_CommercialRegion { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_ConnectToolboxURL { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_Email { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_Employer { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_OnOffPremise { get; set; } 
        public string extension_be6dc6c96b4c411780751b4231962926_Region { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsID { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsRoleID { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_Division { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_State { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_Country { get; set; }
        public string extension_be6dc6c96b4c411780751b4231962926_TradeEmployer { get; set; }
#endif*/
    }

    public class B2CUser : B2CNewUser
    {
        public string objectType { get; set; }
        public string objectId { get; set; }
        public string mail { get; set; }
        public string userPrincipalName { get; set; }
        public DateTime? deletionTimestamp { get; set; }
        public string[] assignedLicenses { get; set; }
        public string[] assignedPlans { get; set; }
        public string dirSyncEnabled { get; set; }
        public string immutableId { get; set; }
        public string lastDirSyncTime { get; set; }
        public string onPremisesSecurityIdentifier { get; set; }
        public string[] otherMails { get; set; }
        public string preferredLanguage { get; set; }
        public string[] provisionedPlans { get; set; }
        public string[] provisioningErrors { get; set; }
        public string[] proxyAddresses { get; set; }
    }
}
