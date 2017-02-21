#define PROD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Facet.Combinatorics;
using System.IO;
using Microsoft.SharePoint.Administration.Claims;

namespace ProvisionSecurityGroups
{
    class PopulateSecurityGroups
    {
        public static void ScanCsv(string CsvPath)
        {
            try
            {
                //System.Threading.Thread.Sleep(10000);
                Console.WriteLine("Current Claims:");
                List<string> rows = System.IO.File.ReadAllLines(CsvPath).ToList<string>();
                string[] header = rows[0].Split(',');
                for (int i = 1; i < rows.Count; i++)
                {
                    try
                    {
                        string row = rows[i];
                        Dictionary<string, string> claims = GetClaimsForUser(header, row.Split(','));
                        string userName = row.Split(',')[0];
                        Console.WriteLine("Processing User: " + userName);
                        List<string> values = new List<string>();
                        string claimUP = "";
                        foreach (string k in claims.Keys)
                        {
                            Console.WriteLine(k + " - " + claims[k]);
                            claimUP += k + "-" + claims[k] + "|";
                            values.Add(claims[k]);
                        }
                        Console.WriteLine("Claim Permutations");
                        List<string> allcombos = new List<string>();
                        for (int n = 2; n <= values.Count - 1; n++)
                        {
                            var combis = new Combinations<string>(values, n, GenerateOption.WithoutRepetition);
                            allcombos.AddRange(combis.Select(c => string.Join(" ", c)));
                        }
                        foreach (string c in allcombos)
                        {
                            Console.WriteLine(c);
                        }
                        //verify permissions
                        VerifyPermissions(userName, allcombos);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Error processing user:" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing CSV:" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException);
            }
        }

        internal static void VerifyPermissions(string userName, List<string> permutations)
        {
#if DEV
            string url = "https://connect2dev.beamsuntory.com";
#endif
#if PROD
            string url = "https://connect.beamsuntory.com";
#endif
            try
            {
#if DEV
                string userClaim = "i:0e.t|acs dev|" + userName;
#endif
#if PROD
                string userClaim = "i:0e.t|beam suntory acs|" + userName;
#endif
                using (SPSite s = new SPSite(url))
                {
                    using (SPWeb w = s.RootWeb)
                    {
                        w.AllowUnsafeUpdates = true;
                        List<string> missingGroups;
                        List<SPGroup> groups = PermutationGroups(permutations, w, out missingGroups);
                        SPUser cUser = w.EnsureUser(userClaim);
                        SPGroupCollection gUser = cUser.Groups;
                        foreach (SPGroup g in groups)
                        {
                            bool isAlreadyMember = false;
                            foreach (SPGroup gu in gUser)
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
                                Console.WriteLine("Adding user to group:" + g.Name);
#endif
                            }
#if DEBUG
                            Console.WriteLine("Already a member of group: " + g.Name);
#endif
                        }
                        w.AllowUnsafeUpdates = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Verifying Permissions:" + ex.Message + "\n" + ex.StackTrace);
            }
        }

        internal static List<SPGroup> PermutationGroups(List<string> combinations, SPWeb web, out List<string> updatedCombinations)
        {
            char[] splitter = new char[] { ' ' };
            List<SPGroup> groups = new List<SPGroup>();
            foreach (SPGroup g in web.SiteGroups)
            {
                for (int i = 0; i < combinations.Count; i++)
                {
                    if (g.Name == combinations[i])
                    {
                        groups.Add(g);
                        combinations[i] = "~" + combinations[i];
                        Console.WriteLine("Found existing group: " + g.Name);
                    }
                    else if (g.Name.Length == combinations[i].Length)
                    {
                        if (CompareSegments(g.Name.Split(splitter, StringSplitOptions.RemoveEmptyEntries), combinations[i].Split(splitter, StringSplitOptions.RemoveEmptyEntries)))
                        {
                            groups.Add(g);
                            combinations[i] = "~" + combinations[i];
                            Console.WriteLine("Found existing group: " + g.Name);
                        }
                    }
                }
            }
            updatedCombinations = combinations;
            return groups;
        }

        internal static SPGroup CreateGroup(SPWeb Web, string Name, SPUser adminUser)
        {
#if DEBUG
            Console.WriteLine("Creating group: " + Name);
#endif
            Web.SiteGroups.Add(Name, adminUser, adminUser, "");
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

        internal static Dictionary<string, string> GetClaimsForUser(string[] headers, string[] row)
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();
            for(int i = 1; i < headers.Count(); i++)
            {
                string header = headers[i];
                try
                {
                    if ((header == "extensionAttribute5" /* Region */ ||
                        header == "extensionAttribute6" /* SuccessFactorsRoleID */ ||
                        header == "employer" ||
                        header == "country" ||
                        header == "state") &&
                        !string.IsNullOrEmpty(row[i]))
                        claims.Add(header, row[i].Replace(" ", "_").Replace("/", "").Replace("'", "").Replace(",,", "").Replace("#", "").Replace("&", "_").Replace("%", "").Replace("*", "").Replace(";", "").Replace("@", "").Replace(",", " ").Replace("\"", ""));
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
