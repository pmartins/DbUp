using DbUp;
using DbUp.Engine;
using DbUp.Support.SybaseAse;
using DbUp.SybaseAse;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SybaseAseSampleApplication
{
    class Program
    {
        static int Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["SampleSybase"].ConnectionString;

            var upgrader =
                DeployChanges.To
                    .SybaseAseDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .WithoutTransaction()
                    .LogToConsole()
                    .Build();

            Console.WriteLine("Is upgrade required: " + upgrader.IsUpgradeRequired());

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();

                WaitIfDebug();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            WaitIfDebug();
            return 0;

        }

        static void Display(string dbType, DatabaseUpgradeResult result, TimeSpan ts)
        {
            // Display the result
            if (result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success!");
                Console.WriteLine("{0} Database Upgrade Runtime: {1}", dbType,
                    String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
                Console.ReadKey();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ReadKey();
                Console.WriteLine("Failed!");
            }
        }

        [Conditional("DEBUG")]
        public static void WaitIfDebug()
        {
            Console.ReadLine();
        }
    }
}
