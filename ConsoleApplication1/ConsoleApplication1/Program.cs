using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> rows = System.IO.File.ReadAllLines(@"D:\install\maintenance tools\ProvisionSecurityGroups\AdditionalSharePointSecurityGroups_09132016.csv").ToList<string>();
            string[] header = rows[0].Split(',');
            for (int i = 1; i < rows.Count; i++)
            {
                try
                {
                    string row = rows[i];
                    //Dictionary<string, string> claims = GetClaimsForUser(header, row.Split(','));
                    //string userName = row.Split(',')[0];
                    //Console.WriteLine("Processing User: " + userName);
                    //List<string> values = new List<string>();
                    //string claimUP = "";

                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
