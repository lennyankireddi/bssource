using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace BEAM.Connect
{
    public static class Helpers
    {
        internal static string SPURL = SPContext.Current.Site.Url;
        public static List<CascadingDropDown> GetCountries()
        {
            List<CascadingDropDown> cdls = new List<CascadingDropDown>();
            cdls.Add(new CascadingDropDown { Root = "", SystemValue = "" });
            try
            {
                using (SPSite site = new SPSite(SPURL))
                {
                    using (SPWeb web = site.RootWeb)
                    {
                        SPQuery q = new SPQuery();
                        q.Query =
                            "<Where>" +
                                "<Eq>" +
                                    "<FieldRef Name=\"Title\"/>" +
                                    "<Value Type=\"Text\">Country</Value>" +
                                "</Eq>" +
                            "</Where>" +
                            "<OrderBy>" +
                                "<FieldRef Name=\"Parent\" Ascending=\"TRUE\" />" + //get root nodes first
                                "<FieldRef Name=\"Value\" Ascending=\"TRUE\" />" +
                            "</OrderBy>";
                        q.ViewFields =
                            "<FieldRef Name=\"Title\"/>" +
                            "<FieldRef Name=\"Parent\"/>" +
                            "<FieldRef Name=\"Value\"/>" +
                            "<FieldRef Name=\"SystemValue\"/>";
                        CascadingDropDown cdl;
                        foreach (SPListItem i in web.Lists["RegistrationConfiguration"].GetItems(q))
                        {
                            if (!FieldHasValue(i, "Parent"))
                            {
                                cdl = new CascadingDropDown();
                                cdl.Root = i["Value"].ToString();
                                if (FieldHasValue(i, "SystemValue"))
                                    cdl.SystemValue = i["SystemValue"].ToString();
                                else
                                    cdl.SystemValue = cdl.Root;
                                cdls.Add(cdl);
                            }
                            else
                            {
                                string parent = new SPFieldLookupValue(i["Parent"] as string).LookupValue;
                                for (int n = 0; n < cdls.Count; n++)
                                {
                                    if (cdls[n].Root == parent)
                                    {
                                        if (cdls[n].Children.Count == 0)
                                            cdls[n].Children.Add(new CascadingDropDown { Root = "", SystemValue = "" });

                                        cdl = new CascadingDropDown();
                                        cdl.Root = i["Value"].ToString();
                                        if (FieldHasValue(i, "SystemValue"))
                                            cdl.SystemValue = i["SystemValue"].ToString();
                                        else
                                            cdl.SystemValue = cdl.Root;
                                        cdls[n].Children.Add(cdl);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
            return cdls;
        }

        public static List<string> GetEmployers()
        {
            List<string> vals = new List<string>();
            vals.Add("");
            try
            {
                using (SPSite site = new SPSite(SPURL))
                {
                    using (SPWeb web = site.RootWeb)
                    {
                        SPQuery q = new SPQuery();
                        q.Query =
                            "<Where>" +
                                "<Eq>" +
                                    "<FieldRef Name=\"Title\"/>" +
                                    "<Value Type=\"Text\">Employer</Value>" +
                                "</Eq>" +
                            "</Where>" +
                            "<OrderBy>" +
                                "<FieldRef Name=\"Value\" Ascending=\"TRUE\" />" + //get root nodes first
                            "</OrderBy>";
                        q.ViewFields =
                            "<FieldRef Name=\"Title\"/>" +
                            "<FieldRef Name=\"Parent\"/>" +
                            "<FieldRef Name=\"Value\"/>" +
                            "<FieldRef Name=\"SystemValue\"/>";
                        foreach (SPListItem i in web.Lists["RegistrationConfiguration"].GetItems(q))
                        {
                            vals.Add(i["Value"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
            return vals;
        }

        public static List<string> GetTradeEmployers()
        {
            List<string> vals = new List<string>();
            vals.Add("");
            try
            {
                using (SPSite site = new SPSite(SPURL))
                {
                    using (SPWeb web = site.RootWeb)
                    {
                        SPQuery q = new SPQuery();
                        q.Query =
                            "<Where>" +
                                "<Eq>" +
                                    "<FieldRef Name=\"Title\"/>" +
                                    "<Value Type=\"Text\">TradeEmployer</Value>" +
                                "</Eq>" +
                            "</Where>" +
                            "<OrderBy>" +
                                "<FieldRef Name=\"Value\" Ascending=\"TRUE\" />" + //get root nodes first
                            "</OrderBy>";
                        q.ViewFields =
                            "<FieldRef Name=\"Title\"/>" +
                            "<FieldRef Name=\"Parent\"/>" +
                            "<FieldRef Name=\"Value\"/>" +
                            "<FieldRef Name=\"SystemValue\"/>";
                        foreach (SPListItem i in web.Lists["RegistrationConfiguration"].GetItems(q))
                        {
                            vals.Add(i["Value"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
            return vals;
        }

        public static void LogUserChange(bool TradeUser, bool Update, Dictionary<string, string> Details)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                try
                {
                    using (SPSite site = new SPSite(SPURL))
                    {
                        SPWeb w = site.OpenWeb();
                        w.AllowUnsafeUpdates = true;
                        SPListItem item = w.Lists["UserRequests"].AddItem();
                        item["Title"] = Details["preferredName"];
                        item["UserType"] = (TradeUser) ? "Trade" : "Distributor";
                        item["RequestType"] = (Update) ? "Update" : "New User";
                        if (Details.ContainsKey("country"))
                            item["Country"] = Details["country"];
                        if (Details.ContainsKey("state"))
                            item["State"] = Details["state"];
                        if (Details.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_Employer"))
                            item["Employer"] = Details["extension_a67c327875c34024bd4523a3d66619ba_Employer"];
                        if (Details.ContainsKey("extension_be6dc6c96b4c411780751b4231962926_Employer"))
                            item["Employer"] = Details["extension_be6dc6c96b4c411780751b4231962926_Employer"];
                        if (Details.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_Area"))
                            item["Area"] = Details["extension_a67c327875c34024bd4523a3d66619ba_Area"];
                        if (Details.ContainsKey("extension_be6dc6c96b4c411780751b4231962926_Area"))
                            item["Area"] = Details["extension_be6dc6c96b4c411780751b4231962926_Area"];
                        if (Details.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise"))
                            item["OnOffPremise"] = Details["extension_a67c327875c34024bd4523a3d66619ba_OnOffPremise"];
                        if (Details.ContainsKey("extension_be6dc6c96b4c411780751b4231962926_OnOffPremise"))
                            item["OnOffPremise"] = Details["extension_be6dc6c96b4c411780751b4231962926_OnOffPremise"];
                        if (Details.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_Region"))
                            item["Region"] = Details["extension_a67c327875c34024bd4523a3d66619ba_Region"];
                        if (Details.ContainsKey("extension_be6dc6c96b4c411780751b4231962926_Region"))
                            item["Region"] = Details["extension_be6dc6c96b4c411780751b4231962926_Region"];
                        if (Details.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_Division"))
                            item["Division"] = Details["extension_a67c327875c34024bd4523a3d66619ba_Division"];
                        if (Details.ContainsKey("extension_be6dc6c96b4c411780751b4231962926_Division"))
                            item["Division"] = Details["extension_be6dc6c96b4c411780751b4231962926_Division"];
                        if (Details.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_CommercialRegion"))
                            item["CommercialRegion"] = Details["extension_a67c327875c34024bd4523a3d66619ba_CommercialRegion"];
                        if (Details.ContainsKey("extension_be6dc6c96b4c411780751b4231962926_CommercialRegion"))
                            item["CommercialRegion"] = Details["extension_be6dc6c96b4c411780751b4231962926_CommercialRegion"];
                        if (Details.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_Birthdate"))
                            item["Birthdate"] = Details["extension_a67c327875c34024bd4523a3d66619ba_Birthdate"];
                        if (Details.ContainsKey("extension_be6dc6c96b4c411780751b4231962926_Birthdate"))
                            item["Birthdate"] = Details["extension_be6dc6c96b4c411780751b4231962926_Birthdate"];
                        if (Details.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsRoleID"))
                            item["SuccessFactorsRoleID"] = Details["extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsRoleID"];
                        if (Details.ContainsKey("extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsRoleID"))
                            item["SuccessFactorsRoleID"] = Details["extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsRoleID"];
                        if (Details.ContainsKey("extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsID"))
                            item["SuccessFactorsID"] = Details["extension_a67c327875c34024bd4523a3d66619ba_SuccessFactorsID"];
                        if (Details.ContainsKey("extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsID"))
                            item["SuccessFactorsID"] = Details["extension_be6dc6c96b4c411780751b4231962926_SuccessFactorsID"];
                        item.Update();
                        w.AllowUnsafeUpdates = false;
                    }
                }
                catch (Exception ex)
                {

                }
            });
        }

        private static bool FieldHasValue(SPListItem item, string fieldName)
        {
            try
            {
                if (item.Fields.ContainsField(fieldName))
                    if (item[fieldName] != null)
                        if (!string.IsNullOrEmpty(item[fieldName] as string))
                            return true;
            }
            catch (Exception) { }
            return false;
        }
    }

    public class CascadingDropDown
    {
        public string Root { get; set; }
        public string SystemValue { get; set; }
        public List<CascadingDropDown> Children = new List<CascadingDropDown>();
    }
}
