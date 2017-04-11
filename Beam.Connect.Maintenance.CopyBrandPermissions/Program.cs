using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using static Microsoft.SharePoint.WebPartPages.WebPartToolPart;
using System.IO;

namespace Beam.Connect.Maintenance.CopyBrandPermissions
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = string.Empty;
            string sourceSitePath = string.Empty;
            string copyAction = string.Empty;

            if (args.Length > 0)
            {
                Console.WriteLine("site url: " + args[0].ToString());
                url = args[0].ToString();

                Console.WriteLine("source site url: " + args[1].ToString());
                sourceSitePath = args[1].ToString();

                Console.WriteLine("action: " + args[2].ToString());
                copyAction = args[2].ToString();
            }
            else
            {
                url = "https://connectadmin.beamsuntory.com";
                sourceSitePath = "/connect/brands/cognac-brandy/courvoisier";
                copyAction = "copylanguages";
            }

            using (SPSite site = new SPSite(url))
            {
                Console.WriteLine("successfully connected to " + url);

                using (SPWeb cw = site.AllWebs["CONNECT"])
                {
                    if (copyAction.ToLower() == "copylanguages")
                    {
                        CopyPermissionsRecursive(cw);
                        //CopyPermissionsAcrossLanguages(cw, site);
                    }
                    else
                    {
                        CopyBrandPermsFromSite(cw, site, sourceSitePath);
                    }
                }
            }
        }

        private static void AddPermissions(SPSite site, SPSecurableObject secObj, string groupName, string roleDef)
        {
            //manually add permission for this new group
            SPRoleAssignment roleAssignment = new SPRoleAssignment(site.RootWeb.SiteGroups[groupName]);
            roleAssignment.RoleDefinitionBindings.Add(site.RootWeb.RoleDefinitions[roleDef]);
            secObj.RoleAssignments.Add(roleAssignment);
        }

        private static void CopyPermission(SPSecurableObject secObj, SPRoleAssignment roleAssignment)
        {
            secObj.RoleAssignments.Add(roleAssignment);
        }

        private static void CleanUpExistingGroups(SPWeb productSite)
        {
            #region remove all roleassignments
            for (int i = 0; i < productSite.RoleAssignments.Count; i++)
            {
                productSite.RoleAssignments.Remove(i);
            }
            #endregion
        }

        private static void CopyBrandPermsFromSite(SPWeb cw, SPSite site, string sourceSitePath)
        {
            #region copy category sites
            foreach (SPWeb categorySite in cw.Webs["Brands"].Webs)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" >> working through " + categorySite.Name);

                foreach (SPWeb productSite in categorySite.Webs)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(" >> processing " + productSite.Name);

                    // check to make sure site is not source site
                    if (productSite.ServerRelativeUrl != sourceSitePath)
                    {
                        productSite.AllowUnsafeUpdates = true;
                        productSite.Update();
                        productSite.BreakRoleInheritance(true);
                        productSite.Update();


                        CleanUpExistingGroups(productSite);

                        // TODO: Copy permission courvoisier site
                        using (SPWeb cour = site.OpenWeb(sourceSitePath))
                        {
                            foreach (SPRoleAssignment assignment in cour.RoleAssignments)
                            {
                                CopyPermission(productSite, assignment);
                            }

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("     >> copied permissions");
                        }

                        productSite.AllowUnsafeUpdates = false;
                        productSite.Update();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("     >> site updated");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("     >> this is our source site");
                    }
                }
            }
            #endregion
        }

        private static string[] languages = new string[] { "es", "zhcn", "zhhk", "de", "fr", "ja", "pl", "pt", "ru" };

        private static void CopyPermissionsRecursive(SPWeb sourceWeb)
        {
            foreach(SPWeb web in sourceWeb.Webs)
            {
                Console.WriteLine("Processing: " + web.Url);
                foreach(string lang in languages)
                {
                    CopyPermissionsForLanguage(web, lang, web.Site);
                }
                if (web.Webs.Count > 0)
                    CopyPermissionsRecursive(web);
            }
        }

        private static void CopyPermissionsAcrossLanguages(SPWeb connectEnglish, SPSite site)
        {
            foreach (SPWeb topEnglishSites in connectEnglish.Webs)
            {
                if (topEnglishSites.Webs.Count > 0)
                {
                    foreach (SPWeb categoryWeb in topEnglishSites.Webs)
                    {
                        if (categoryWeb.Webs.Count > 0)
                        {
                            foreach (SPWeb productWeb in categoryWeb.Webs)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine(" >> working through " + productWeb.Url);
                                CopyPermissionsForLanguage(productWeb, "es", site);
                                CopyPermissionsForLanguage(productWeb, "zhcn", site);
                                CopyPermissionsForLanguage(productWeb, "zhhk", site);
                                CopyPermissionsForLanguage(productWeb, "de", site);
                                CopyPermissionsForLanguage(productWeb, "fr", site);
                                CopyPermissionsForLanguage(productWeb, "ja", site);
                                CopyPermissionsForLanguage(productWeb, "pl", site);
                                CopyPermissionsForLanguage(productWeb, "pt", site);
                                CopyPermissionsForLanguage(productWeb, "ru", site);
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(" >> working through " + categoryWeb.Url);
                            CopyPermissionsForLanguage(categoryWeb, "es", site);
                            CopyPermissionsForLanguage(categoryWeb, "zhcn", site);
                            CopyPermissionsForLanguage(categoryWeb, "zhhk", site);
                            CopyPermissionsForLanguage(categoryWeb, "de", site);
                            CopyPermissionsForLanguage(categoryWeb, "fr", site);
                            CopyPermissionsForLanguage(categoryWeb, "ja", site);
                            CopyPermissionsForLanguage(categoryWeb, "pl", site);
                            CopyPermissionsForLanguage(categoryWeb, "pt", site);
                            CopyPermissionsForLanguage(categoryWeb, "ru", site);
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(" >> working through " + topEnglishSites.Url);
                    CopyPermissionsForLanguage(topEnglishSites, "es", site);
                    CopyPermissionsForLanguage(topEnglishSites, "zhcn", site);
                    CopyPermissionsForLanguage(topEnglishSites, "zhhk", site);
                    CopyPermissionsForLanguage(topEnglishSites, "de", site);
                    CopyPermissionsForLanguage(topEnglishSites, "fr", site);
                    CopyPermissionsForLanguage(topEnglishSites, "ja", site);
                    CopyPermissionsForLanguage(topEnglishSites, "pl", site);
                    CopyPermissionsForLanguage(topEnglishSites, "pt", site);
                    CopyPermissionsForLanguage(topEnglishSites, "ru", site);
                }

            }
        }

        private static void CopyPermissionsForLanguage(SPWeb englishSite, string language, SPSite site)
        {
            try
            {
                string languageURL = englishSite.ServerRelativeUrl.Replace("CONNECT", language);
                SPWeb langWeb = SiteExists(site.Url, languageURL);
                if (langWeb != null)
                {
                    using (langWeb)
                    {
                        langWeb.AllowUnsafeUpdates = true;
                        langWeb.BreakRoleInheritance(true);

                        CleanUpExistingGroups(langWeb);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("     >> copying english permissions to: " + langWeb.Url);
                        foreach (SPRoleAssignment assignment in englishSite.RoleAssignments)
                        {
                            CopyPermission(langWeb, assignment);
                        }

                        //langWeb.SiteLogoUrl = site.Url + englishSite.SiteLogoUrl;
                        //langWeb.Update();

                        langWeb.AllowUnsafeUpdates = false;

                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Site does not exist at - " + englishSite.ServerRelativeUrl.Replace("CONNECT", language));
            }
        }

        public static SPWeb SiteExists(string siteUrl, string url)
        {
            try
            {
                using (SPSite site = new SPSite(siteUrl))
                {
                    using (SPWeb web = site.OpenWeb(url, true))
                    {
                        return web;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("site does not exist at - " + url);
                return null;
            }
        }
    }
}

