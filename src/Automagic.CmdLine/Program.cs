using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Automagic.Core.DataAccess;
using Automagic.Core.MetaModel;
using Automagic.Core;
using SesamNetCoreClient;

namespace Automagic.CmdLine
{

    public class Program
    {
        private static string connStr = "User ID=<some user ID>;;Password=<some password>;Host=localhost;Port=5432;Pooling=true;Database=Adventureworks";
        private static string connStrVisma = "User ID=<some user ID>;Password=<some password>;Server=tcp:sqlprod1.bouvet.no,1433;Initial Catalog=BouvetNorgeASGlobalData";
        private static string connStrCT = "User ID=<some user ID>;;Password=<some password>;Server=tcp:185.13.94.29,1433;Initial Catalog=CT_7";
        
        private static string conn = "User ID=<some user ID>;;Password=<some password>;Host=35.228.48.175;Port=5432;Pooling=true;Database=test_db2";
        private static string connMySql = "server=35.228.197.165;uid=<some user ID>;;pwd=<some password>;database=mysql_VBJMMMDQ";

        static void Main(string[] args)
        {
            // var agent1 = new Agent();
            // agent1.Start();

            // Console.WriteLine("Read to process...");
            // Console.ReadLine();
            try
            {
                
                var db = new MySQLDb(connMySql);
                //var db = new PostgreSqlDb(conn);
                var se = new ModelBuilder(db);
                var m = new Model(new Core.System() {Name = "test"});
                WebClient myWebClient = new WebClient();
                Stream firstnames = myWebClient.OpenRead("https://csbf8630753f67dx4346x85e.blob.core.windows.net/automagic-test/firstnames.csv");
                Stream alllastnames = myWebClient.OpenRead("https://csbf8630753f67dx4346x85e.blob.core.windows.net/automagic-test/alllastnames.csv");

                var refData = new ReferenceDataBlobs(firstnames, alllastnames);
                firstnames.Close();
                alllastnames.Close();

                Console.WriteLine("Populating Model");
                se.PopulateModel(m);

                foreach (var et in m.EntityTypes)
                {
                    Console.WriteLine("\n\tEntity type: " + et.Name);

                    foreach (var pt in et.PropertyTypes)
                    {
                        Console.WriteLine("\t\t " + pt.Name + " : " + pt.DataType);
                    }
                }

                var pdef = new PersonalDataFinder(refData);
                var candidates = pdef.GetPersonalDataRoots(m, db, new List<string>(args));

                //create a system
                var client = new SesamNetCoreClient.Client("<jwt-token>");
                var system = new SesamSystem("mysql-VBJMMMDQ")
                    .OfType(SystemType.MYSQL)
                    .With("host", "35.228.197.165")
                    .With("database", "mysql_VBJMMMDQ")
                    .With("username", "<username>")
                    .With("password", "<password>");
                client.CreateSystem(system);

                Console.WriteLine("Personal Data Candidates\n");

                foreach (var et in candidates)
                {
                    Console.WriteLine("\n\tEntity type: " + et.EntityType.Name + " " + et.TotalScore);
                    //Create a pipe if score is greater than?
                    if (et.TotalScore > 3)
                    {
                        var source = new SqlSource();
                        source.SetTable(et.EntityType.Name.Split('.')[1]);
                        source.SetSystem("mysql-VBJMMMDQ");
                        source.SetType("sql");
                        var pipe = new Pipe(string
                            .Format("automagic-{0}", et.EntityType.Name
                            .Replace(".", "-")
                            .Replace("_", "-"))).WithSource(source);
                        client.CreatePipe(pipe);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error occurred: " + ex.Message + " : " + ex.StackTrace);
            }
        }

    }

}



