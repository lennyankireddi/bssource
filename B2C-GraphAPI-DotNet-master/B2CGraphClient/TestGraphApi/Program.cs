#define PROD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using GraphApi;
using System.Reflection;
using System.IO;

namespace TestGraphApi
{
    class Program
    {
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
        private static AADUser aad = new AADUser(tenant, id, secret);
        static void Main(string[] args)
        {
            switch(args[0])
            {
                case "CreateUser":
                    PrintUsers(CreateUsers(args[1]));
                    break;
                case "GetUsers":
                    aad.GetUsers("", true);
                    break;
                case "GetUser":
                    aad.GetUsers(args[1], true);
                    break;
            }
        }

        static void PrintUsers(List<B2CUser> Users)
        {
            foreach (B2CUser u in Users)
                PrintUser(u);
        }

        static void PrintUser(B2CUser User)
        {
            string output = "User Details for: " + User.displayName;
            output += "\nAccountEnabled: " + User.accountEnabled;
            output += "\nCity: " + User.city;
            output += "\ndisplayName: " + User.displayName;
            output += "\ncreationType: " + User.creationType;
            output += "\ndepartment: " + User.department;
            output += "\nobjectType: " + User.objectType;
            output += "\nobjectId: " + User.objectId;
            output += "\nuserPrincipalName: " + User.userPrincipalName;
            output += "\ncountry: " + User.country;
            output += "\nstate: " + User.state;
            if(User.deletionTimestamp.HasValue)
                output += "\ndeletionTimestamp: " + User.deletionTimestamp.Value.ToLongDateString();
            output += "\ndirSyncEnabled: " + User.dirSyncEnabled;
            output += "\nimmutableId: " + User.immutableId;
            output += "\npreferredLanguage: " + User.preferredLanguage;
            output += "\nfacsimileTelephoneNumber: " + User.facsimileTelephoneNumber;
            output += "\ngivenName: " + User.givenName;
            output += "\njobTitle: " + User.jobTitle;
            output += "\nmailNickname: " + User.mailNickname;
            output += "\nmail: " + User.mail;
            output += "\nmobile: " + User.mobile;
            output += "\nPasswordPolicies: " + User.passwordPolicies;
            output += "\nphysicalDeliveryOfficeName: " + User.physicalDeliveryOfficeName;
            output += "\npostalCode: " + User.postalCode;
            output += "\nstreetAddress: " + User.streetAddress;
            output += "\nsurname: " + User.surname;
            output += "\ntelephoneNumber: " + User.telephoneNumber;
            output += "\nusageLocation: " + User.usageLocation;
            output += "\nuserType: " + User.userType;
#if DEV
            output += "\next_Area: " + User.extension_a67c327875c34024bd4523a3d66619ba_Area;
            output += "\next_BeamSuntorySponsor: " + User.extension_a67c327875c34024bd4523a3d66619ba_BeamSuntorySponsor;
            output += "\next_Birthdate: " + User.extension_a67c327875c34024bd4523a3d66619ba_Birthdate;
            output += "\next_CommercialRegion: " + User.extension_a67c327875c34024bd4523a3d66619ba_CommercialRegion;
            output += "\next_ConnectToolboxURL: " + User.extension_a67c327875c34024bd4523a3d66619ba_ConnectToolboxURL;
            output += "\next_Email: " + User.extension_a67c327875c34024bd4523a3d66619ba_Email;
            output += "\next_Employer: " + User.extension_a67c327875c34024bd4523a3d66619ba_Employer;
            output += "\next_OnOffPremise: " + User.extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise;
            output += "\next_Region: " + User.extension_a67c327875c34024bd4523a3d66619ba_Region;
            output += "\next_SuccessFactorsID: " + User.extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsID;
            output += "\next_SuccessFactorsRoleID: " + User.extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsRoleID;
            output += "\next_telephoneNumber: " + User.extension_a67c327875c34024bd4523a3d66619ba_telephoneNumber;
#endif
#if PROD
            output += "\next_Area: " + User.extension_be6dc6c96b4c411780751b4231962926_Area;
            output += "\next_BeamSuntorySponsor: " + User.extension_be6dc6c96b4c411780751b4231962926_BeamSuntorySponsor;
            output += "\next_Birthdate: " + User.extension_be6dc6c96b4c411780751b4231962926_Birthdate;
            output += "\next_CommercialRegion: " + User.extension_be6dc6c96b4c411780751b4231962926_CommercialRegion;
            output += "\next_ConnectToolboxURL: " + User.extension_be6dc6c96b4c411780751b4231962926_ConnectToolboxURL;
            output += "\next_Email: " + User.extension_be6dc6c96b4c411780751b4231962926_Email;
            output += "\next_Employer: " + User.extension_be6dc6c96b4c411780751b4231962926_Employer;
            output += "\next_OnOffPremise: " + User.extension_be6dc6c96b4c411780751b4231962926_OnOffPremise;
            output += "\next_Region: " + User.extension_be6dc6c96b4c411780751b4231962926_Region;
            output += "\next_SuccessFactorsID: " + User.extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsID;
            output += "\next_SuccessFactorsRoleID: " + User.extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsRoleID;
            output += "\next_telephoneNumber: " + User.extension_be6dc6c96b4c411780751b4231962926_telephoneNumber;
#endif
            if (User.signInNames != null)
            {
                foreach (SignInName sin in User.signInNames)
                    output += "\nSignInName: " + sin.value;
            }
            if (User.passwordProfile != null)
                output += "\nForceChangePasswordNextLogin: " + User.passwordProfile.forceChangePasswordNextLogin;

        }

        static List<B2CUser> CreateUsers(string FilePath)
        {
            List<B2CUser> users = new List<B2CUser>();
            string[] rows = System.IO.File.ReadAllLines(FilePath);
            string[] Fields;
            Fields = rows[0].Split(new char[] { ',' });
            int cols = Fields.GetLength(0);
            DataTable dt = new DataTable();
            for(int i = 0; i < cols;i++)
            {
                dt.Columns.Add(Fields[i], typeof(string));
            }
            DataRow Row;
            for(int i = 1; i < rows.GetLength(0);i++)
            {
                Fields = rows[i].Split(new char[] { ',' });
                Row = dt.NewRow();
                for (int n = 0; n < cols; n++)
                    Row[n] = Fields[n];
                dt.Rows.Add(Row);
            }
            List<B2CNewUser> newUsers = dt.DataTableToList<B2CNewUser>();
            for(int i = 0; i < newUsers.Count; i++)
            {
                newUsers[i].signInNames = new SignInName[] { new SignInName { type = "emailAddress", value = dt.Rows[i + 1]["SignInName"].ToString() } };
                newUsers[i].passwordProfile = new PasswordProfile { forceChangePasswordNextLogin = true, password = dt.Rows[i + 1]["Password"].ToString() };
#if DEV
                newUsers[i].extension_a67c327875c34024bd4523a3d66619ba_Email = newUsers[i].signInNames[0].value;
#endif
#if PROD
                newUsers[i].extension_be6dc6c96b4c411780751b4231962926_Email = newUsers[i].signInNames[0].value;
#endif
            }
            foreach (B2CNewUser nu in newUsers)
            {
                users.Add(aad.CreateUser(nu));
#if DEV
                Console.WriteLine("Created User: " + nu.extension_a67c327875c34024bd4523a3d66619ba_Email);
#endif
#if PROD
                Console.WriteLine("Created User: " + nu.extension_be6dc6c96b4c411780751b4231962926_Email);
#endif
            }
            return users;
        }
    }

    public static class Helpers
    {
        public static List<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }
    }
}
