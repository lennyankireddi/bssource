#define PROD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facet.Combinatorics;
using Microsoft.SharePoint;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint.Administration.Claims;

namespace ProvisionSecurityGroups
{
    class Program
    {
#if DEV
        static string defaultUser = "idmresc\\spext_setup";
#endif
#if PROD
        static string defaultUser = "beamext\\shpntprd";
#endif
        private static UserProfile currentProfile = null;

        static void Main(string[] args)
        {
            try
            {
                //System.Threading.Thread.Sleep(7000);
                if (args.Length > 1 && args[0] == "PopulateMembers")
                {
                    PopulateSecurityGroups.ScanCsv(args[1]);
                    Console.WriteLine("Completed provisioning group membership");
                    return;
                }
                else
                {
                    List<string> rows = ReadCSV(args[0]);
                    string[] fields = rows[0].Split(',');
                    List<string> updatedRows = new List<string>();
                    for (int i = 1; i < rows.Count; i++)
                    {
                        string row = rows[i].Replace(" ", "_").Replace("/", "").Replace("'", "").Replace(",,", "").Replace("#", "").Replace("&", "_").Replace("%", "").Replace("*", "").Replace(";", "").Replace("@", "").Replace(",", " ").Replace("\"", "");
                        Console.WriteLine(row);
                        updatedRows.Add(row);
                    }

                    VerifyPermissions(updatedRows, args[1]);
                    /*for(int i = 0; i < fields.Count(); i++)
                    {
                        for(int n = 1; n < rows.Count();n++)
                        {
                            string[] currRow = rows[n].Split(',');
                            string currRootVal = currRow[i];
                            List<string> values = GetPossibleCombinations(currRootVal, rows, i);
    #if DEBUG
                            Console.WriteLine("Claim Permutations>");
    #endif
                            List<string> allcombos = new List<string>();
                            for (int x = 0; x < values.Count; x++)
                            {
                                var combis = new Combinations<string>(values, i, GenerateOption.WithoutRepetition);
                                allcombos.AddRange(combis.Select(c => string.Join(" ", c)));
                            }
    #if DEBUG
                            foreach (string c in allcombos)
                            {
                                Console.WriteLine(c);
                            }
    #endif
                            VerifyPermissions(allcombos);
                        }
                    }*/
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:\n" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException + "");
            }
        }

        internal static List<string> GetPossibleCombinations(string baseVal, List<string> rows, int ignore)
        {
            List<string> vals = new List<string>();
            vals.Add(baseVal);
            for(int i = 1; i < rows.Count();i++)
            {
                string[] rowVals = rows[i].Split(',');
                for (int n = 0; n < rowVals.Count(); n++) //format the values returned
                {
                    rowVals[n] = rowVals[n].Replace(" ", "_").Replace("/", "").Replace("#", "").Replace("&", "_").Replace("%", "").Replace("*", "").Replace(";", "").Replace("@", "");
                    if (n != ignore)
                        vals.Add(rowVals[n]);
                }
            }
            return vals;
        }

        internal static List<string> ReadCSV(string filePath)
        {
            return System.IO.File.ReadAllLines(filePath).ToList<string>();
        }

        internal static void VerifyPermissions(List<string> groups, string url)
        {
            //string currUserName = SPContext.Current.Web.CurrentUser.LoginName;
            //string url = SPContext.Current.Site.Url;
            try
            {
                /*SPSecurity.RunWithElevatedPrivileges(delegate ()
                {*/

                SPClaimProviderManager cpm = SPClaimProviderManager.Local;
                SPClaim userClaim = cpm.ConvertIdentifierToClaim(defaultUser, SPIdentifierTypes.WindowsSamAccountName);

                using (SPSite s = new SPSite(url))
                    {
                        using (SPWeb w = s.RootWeb)
                        {
                            w.AllowUnsafeUpdates = true;
                            List<string> missingGroups;
                            List<SPGroup> spgroups = PermutationGroups(groups, w, out missingGroups);
                            if (spgroups.Count != groups.Count)
                            {
                                SPUser admin = w.EnsureUser(userClaim.ToEncodedString());
                                CreateMissingGroups(missingGroups, w, admin);
                            }
                            w.AllowUnsafeUpdates = false;
                        }
                    }
                //});
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:\n" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException);
            }
        }

        internal static List<SPGroup> CreateMissingGroups(List<string> groupNames, SPWeb web, SPUser groupOwner)
        {
            List<SPGroup> missingGroups = new List<SPGroup>();
            foreach (string s in groupNames)
            {
                if (!s.StartsWith("~"))
                    missingGroups.Add(CreateGroup(web, s, groupOwner));
            }
            return missingGroups;
        }

        internal static List<SPGroup> PermutationGroups(List<string> combinations, SPWeb web, out List<string> updatedCombinations)
        {
            char[] splitter = new char[] { ' ' };
            List<SPGroup> groups = new List<SPGroup>();
            foreach (SPGroup g in web.SiteGroups)
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
                        Console.WriteLine("Found existing group: " + g.Name + "");
                    }
                    else if (g.Name.Length == combinations[i].Length)
                    {
                        if (CompareSegments(g.Name.Split(splitter, StringSplitOptions.RemoveEmptyEntries), combinations[i].Split(splitter, StringSplitOptions.RemoveEmptyEntries)))
                        {
                            groups.Add(g);
                            combinations[i] = "~" + combinations[i];
                            Console.WriteLine("Found existing group: " + g.Name + "");
                        }
                    }
                }
                //}
            }
            updatedCombinations = combinations;
            return groups;
        }

        internal static SPGroup CreateGroup(SPWeb Web, string Name, SPUser adminUser)
        {
            Console.WriteLine("Creating group: " + Name + "");
            try
            {
                Web.SiteGroups.Add(Name, adminUser, adminUser, "");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error creating group: " + ex.Message);
                return null;
            }
            return Web.SiteGroups[Name];
        }

        internal static bool CompareSegments(string[] segment1, string[] segment2)
        {
            int matching = 0;
            foreach (string s in segment1)
            {
                foreach (string s2 in segment2)
                {
                    if (s == s2)
                        matching++;
                }
            }
            if (matching == segment1.Length)
                return true;
            return false;
        }

        internal static List<string> GetClaimsForUser(string row)
        {
            string[] fields = row.Split(',');
            for(int i = 0; i < fields.Count(); i++)
            {
                fields[i] = fields[i].Replace(" ", "_").Replace("/", "").Replace("#", "").Replace("&", "_").Replace("%", "").Replace("*", "").Replace(";", "").Replace("@", "");
            }
            return fields.ToList<string>();
        }
    }
}
