using System;
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Automagic.Service.Model;
using Microsoft.AspNetCore.Mvc;
using Automagic.Core;
using Automagic.Core.DataAccess;
using FKeyMapping;
using FKeyMappingPostgreSQL;
using PostgreSQLIndexMapping;
using IndexRefMappingPostgreSQL;
using IndexMapping;
using GetAllTablesAndColumns;
using IndexRefMapping;
using FKeyRefQueries;
using SesamNetCoreClient;
using System.Net.Http;

namespace Automagic.Service.Controllers
{
    [Produces("application/json")]
    [Route("api/v2")]
    
    public class AutomagicV2Controller : Controller
    {
        static string tables = "";
        static string User = "";
        static string Dbase = "";
        static string Namedb = "";
        static string Host = "";
        static string Port = "";
        static string Password = "";
        static string Mapping = "";
        static string SesamJWT = "";
        static string SesamSubID = "";

        //Setting index mapping NI
        static List<string> concatPairedlist = new List<string>();

        //Setting fkey mapping NI
        static List<string> concatlist = new List<string>();

        
        [ProducesResponseType(typeof(ServiceV2Description), 200)]
        [HttpGet]
        public ServiceV2Description Get()
        {
            return new ServiceV2Description();
        }
        
        [ProducesResponseType(typeof(JobRequestDescription), 200)]
        [HttpPost("job/create/")]
        public IActionResult Post([FromBody] JobRequestDescription request)
        {
            try
            {
                // Setting variables
                User = request.dbUser.ToString();
                Dbase = request.Dbase.ToString();
                Namedb = request.dbName.ToString();
                Host = request.dbHost.ToString();
                Port = request.dbPort.ToString();
                Password = request.dbPassword.ToString();
                Mapping = request.MappingChoice.ToString();
    
                using (var file = System.IO.File.OpenText("Properties/launchSettings.json"))
                {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);

                var variables = jObject
                    .GetValue("profiles")
                    //select a proper profile here
                    .SelectMany(profiles => profiles.Children())
                    .SelectMany(profile => profile.Children<JProperty>())
                    .Where(prop => prop.Name == "environmentVariables")
                    .SelectMany(prop => prop.Value.Children<JProperty>())
                    .ToList();

                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                }
                }
                
                // Making connection to different databases dynamic - add databases here when needed
                string connSql= "";
                Db db = null;   
                if (Dbase.ToLower().Contains("mysql")) 
                {
                    connSql = string.Format("server={0};uid={1};pwd={2};database={3}", Host, User, Password, Namedb);
                    db = new MySQLDb(connSql);
                }
                else if (Dbase.ToLower().Contains("postgresql"))
                {
                    connSql = string.Format("User ID={0};Password={1};Host={2};Port={3};Pooling=true;Database={4}", User, Password, Host, Port, Namedb);
                    db = new PostgreSqlDb(connSql);
                }
                
                var se = new ModelBuilder(db);
                var m = new Core.MetaModel.Model(new Core.System() {Name = "test"});
                WebClient myWebClient = new WebClient();
                Stream firstnames = myWebClient.OpenRead("https://csbf8630753f67dx4346x85e.blob.core.windows.net/automagic-test/firstnames.csv");
                Stream alllastnames = myWebClient.OpenRead("https://csbf8630753f67dx4346x85e.blob.core.windows.net/automagic-test/alllastnames.csv");

                var refData = new ReferenceDataBlobs(firstnames, alllastnames);
                firstnames.Close();
                alllastnames.Close();

                //Console.WriteLine("--------------------------------------------------------------------------------");
                //Console.WriteLine("Type one of the following values for automagic to undertake a specific config setup of pipes in Sesam");
                //Console.WriteLine("Type '1', if you want Sesam to make pipes in accordance to index mapping in your database or '2' if you want automagic to undertake Fkey mapping in your database instead.");
                //Console.WriteLine("Enter your automagic choice: ");
                //int input = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("Populating Model");
                se.PopulateModel(m);

                var pdef = new PersonalDataFinder(refData);
                var candidates = pdef.GetPersonalDataRoots(m, db, new List<string>(Array.Empty<string>()));


                //StringBuilder sesamTables = new StringBuilder();

                // Evaluating input from console prompt
                if (Mapping.ToLower().Contains("index"))
                {
                    // Logic here for finding and naming Pii tables for index mapping ".last()"
                    StringBuilder piiIndxTables = new StringBuilder();
                    foreach (var et in candidates)
                    {   
                        if (et.TotalScore >= 1)
                        {
                            piiIndxTables.AppendLine(et.EntityType.Name.ToString().TrimEnd().Split('.').Last());
                        }
                    }

                    //Setting Index table and column variables from table schema
                    StringBuilder indexColumns = new StringBuilder();
                    StringBuilder indexTables = new StringBuilder();

                    //Setting all columns and tables variables from column schema 
                    StringBuilder allColumns = new StringBuilder();
                    StringBuilder allTables = new StringBuilder();

                    //Setting index ref tables and columns
                    StringBuilder tablesWithIndxRefs = new StringBuilder();
                    StringBuilder pairingIndxTables = new StringBuilder();
                    StringBuilder pairingIndxColumns = new StringBuilder();
                    StringBuilder columnsWithIndxRefs = new StringBuilder();

                    if (Dbase.ToLower().Contains("mysql"))
                    {
                        IndexQuery index = new IndexQuery(indexColumns, indexTables, db, Namedb); 
                        //---------------------------------
                        
                        GetAllQuery queryAll = new GetAllQuery(allColumns, allTables, db, Namedb); 
                        //---------------------------------
                        
                        IndexRefQuery indexRef = new IndexRefQuery(allTables, piiIndxTables, indexColumns, allColumns, tablesWithIndxRefs, pairingIndxTables, pairingIndxColumns, columnsWithIndxRefs, db, Namedb);
                        //---------------------------------
                    
                        // Logic here for correctly creating NI in a concatenated list for index mapping
                        var tablesWithIndxRefsArray = tablesWithIndxRefs.ToString().TrimEnd().Split("\n");
                        var columnsWithIndxRefsArray = columnsWithIndxRefs.ToString().TrimEnd().Split("\n");
                        var pairingIndxTablesArray = pairingIndxTables.ToString().TrimEnd().Split("\n");
                        var pairingIndxColumnsArray = pairingIndxColumns.ToString().TrimEnd().Split("\n");
                        int dk = 0;
                        foreach(string e in pairingIndxTablesArray)
                        {
                            concatPairedlist.Add(tablesWithIndxRefsArray[dk] + ";" + e + ";" + pairingIndxColumnsArray[dk] + ";" + columnsWithIndxRefsArray[dk]);
                            dk++;   
                        }
                    }

                    else if (Dbase.ToLower().Contains("postgresql"))
                    {
                        IndexQueryPostgreSQL index = new IndexQueryPostgreSQL(indexTables, indexColumns, db);
                        tablesWithIndxRefs = indexTables;
                        //---------------------------------

                        GetAllQueryPostGreSQL queryAll = new GetAllQueryPostGreSQL(allColumns, allTables, db, Namedb);
                        string id = "id"; // Making sure id is included
                        if (allColumns.ToString().Contains("id") == false)
                        {   
                            allColumns.AppendLine(id);
                        }
                        //---------------------------------

                        //Getting matched values
                        StringBuilder concatenatedList = new StringBuilder();
                        IndexRefQueryPostgreSQL indexRef = new IndexRefQueryPostgreSQL(allTables, indexTables, allColumns, indexColumns, concatenatedList, db, Namedb);
                        //---------------------------------

                        // Logic here for correcly creating NI in a concatenated list for foreign key mapping
                        var concatListArray = concatenatedList.ToString().TrimEnd().Split("\n");         
                        int no = 0;
                        foreach(var e in concatListArray)
                        {
                            concatPairedlist.Add(e);
                            no++;
                        }
                    }
                    
                    // Logic here for appending to Pii tables if Tables with index refs do not exist
                    StringBuilder tablesToSesam = new StringBuilder();
                    foreach (string i in tablesWithIndxRefs.ToString().TrimEnd().Split("\n"))
                    {
                        if (tablesToSesam.ToString().Contains(i) == false)
                        {
                            tablesToSesam.AppendLine(i);
                        }
                    }
                    foreach (string f in piiIndxTables.ToString().TrimEnd().Split("\n"))
                    {
                        if (tablesToSesam.ToString().Contains(f) == false)
                        {
                            tablesToSesam.AppendLine(f);
                        }
                    }
                tables = tablesToSesam.ToString();
                }
                else if (Mapping.ToLower().Contains("fkey"))
                {
                    //Setting FKey reference variables
                    StringBuilder fKeyTables = new StringBuilder();
                    StringBuilder niRefColumns = new StringBuilder();
                    StringBuilder niRefTables = new StringBuilder();
                    StringBuilder fKeyNiTables = new StringBuilder();
                    StringBuilder fKeyNiColumns = new StringBuilder();

                    if (Dbase.ToLower().Contains("mysql"))
                    {
                        FKeyQuery query = new FKeyQuery(fKeyTables, niRefColumns, niRefTables, fKeyNiColumns, fKeyNiTables, db, Namedb);
                        //---------------------------------

                        // Logic here for correcly creating NI in a concatenated list for foreign key mapping
                        var niRefColumnsArray = niRefColumns.ToString().TrimEnd().Split("\n");
                        var niRefTablesArray = niRefTables.ToString().TrimEnd().Split("\n");
                        var fKeyNiTablesArray = fKeyNiTables.ToString().TrimEnd().Split("\n");
                        var fKeyNiColumnsArray = fKeyNiColumns.ToString().TrimEnd().Split("\n");     
                        int no = 0;
                        foreach(var e in niRefTablesArray)
                        {
                            concatlist.Add(fKeyNiTablesArray[no] + ";" + e + ";" + niRefColumnsArray[no] + ";" + fKeyNiColumnsArray[no]);
                            no++;
                        }  
                    }
                    
                    else if (Dbase.ToLower().Contains("postgresql"))
                    {
                        //Setting all columns and tables variables from column schema 
                        StringBuilder allColumns = new StringBuilder();
                        StringBuilder allTables = new StringBuilder();
                        GetAllQueryPostGreSQL queryAll = new GetAllQueryPostGreSQL(allColumns, allTables, db, Namedb);
                        string id = "id"; // Making sure id is included
                        if (allColumns.ToString().Contains("id") == false)
                        {   
                            allColumns.AppendLine(id);
                        }
                        
                        //Setting fkey refs for postgresql
                        StringBuilder fKeyTablesPostgreSQL = new StringBuilder();
                        StringBuilder fKeyColumnsPostgreSQL = new StringBuilder();
                        FKeyQueryPostgreSQL getFKeys = new FKeyQueryPostgreSQL(fKeyTablesPostgreSQL, fKeyColumnsPostgreSQL, db, Namedb);

                        //Getting matched values
                        StringBuilder concatenatedList = new StringBuilder();
                        FKeyMatchQueries query = new FKeyMatchQueries(fKeyTablesPostgreSQL, allTables, fKeyColumnsPostgreSQL, allColumns, concatenatedList, db, Namedb);
                        //----------------------------------

                        // Logic here for correcly creating NI in a concatenated list for foreign key mapping
                        var concatListArray = concatenatedList.ToString().TrimEnd().Split("\n");     
                        int no = 0;
                        foreach(var e in concatListArray)
                        {
                            concatlist.Add(e);
                            no++;
                        } 
                    }

                    // Generating string array with Fkey referencing PII tables
                    StringBuilder tablesKeyPii = new StringBuilder();
                    foreach (var et in candidates)
                    {    
                        if (et.TotalScore >= 1)
                        {
                            foreach (string i in et.EntityType.Name.ToString().TrimEnd().Split("\n"))
                            {
                                if (tablesKeyPii.ToString().Contains(i) == false)
                                {
                                    tablesKeyPii.AppendLine(i);
                                }
                            }
                        }
                    }

                    foreach (string i in fKeyTables.ToString().TrimEnd().Split(";"))
                    {
                        if (tablesKeyPii.ToString().Contains(i) == false)
                        {
                            tablesKeyPii.AppendLine(i);
                        }
                    }
                tables = tablesKeyPii.ToString();
                }
            return new OkObjectResult(tables);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error occurred: " + ex.Message + " : " + ex.StackTrace);
                return new OkObjectResult(request);
            }    
        }



        [ProducesResponseType(200)]
        [HttpGet("tables/")]
        public ActionResult GetTables()
        {
            //Console.WriteLine("Test to see tables : " + tables);
            return new OkObjectResult(tables);
        }



        [ProducesResponseType(typeof(JobRequestDescription), 200)]
        [HttpPost("pipes/")]
        public IActionResult PostToSesam([FromBody] JobRequestDescription request)
        {
            try
            {
                // Setting variables
                SesamJWT = request.SesamJWT.ToString();
                SesamSubID = request.SesamSubID.ToString();
                if (Mapping.ToString().Contains("index"))
                {
                    //create a system
                    var client = new SesamNetCoreClient.Client(SesamJWT, SesamSubID);
                    SesamSystem system = null;
                    if (Dbase.ToLower().Contains("mysql"))
                    {
                        system = new SesamSystem(Namedb)
                            .OfType(SystemType.MYSQL)
                            .With("host", Host)
                            .With("database", Namedb)
                            .With("username", User)
                            .With("password", Password);
                    }
                    else if (Dbase.ToLower().Contains("postgresql"))
                    {
                        system = new SesamSystem(Namedb)
                            .OfType(SystemType.POSTGRESQL)
                            .With("host", Host)
                            .With("database", Namedb)
                            .With("username", User)
                            .With("password", Password);
                    }

                    try
                    { 
                        client.CreateSystem(system);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine("Skipping the creation of system, because it already exists. Skipping with error: \n{0}", ex);
                    }

                    // create pipes
                    foreach (string et in tables.ToString().TrimEnd().Split("\n"))
                    {   
                        if (et != "")
                        {
                            var namePrefix = et.TrimEnd()
                                .Replace(".", "-")
                                .Replace("_", "-");
                            
                            var pipe = new Pipe("");
                            if (Dbase.ToLower().Contains("mysql"))
                            {
                                pipe = new Pipe(string
                                .Format("automagic-mysql-{0}", et.TrimEnd()
                                .Replace(".", "-")
                                .Replace("_", "-")));
                            }
                            else if (Dbase.ToLower().Contains("postgresql"))
                            {
                                pipe = new Pipe(string
                                .Format("automagic-postgresql-{0}", et.TrimEnd()
                                .Replace(".", "-")
                                .Replace("_", "-")));
                            }

                            var source = new SqlSource();
                            source.SetTable(et.TrimEnd());
                            source.SetSystem(Namedb);
                            source.SetType("sql");
                            source.SetKey("id");

                            var transform = new Transform();
                            transform.SetType("dtl");
                            transform.AddRule();
                            transform.MakeDefaultRule();
                            transform.AddCopy("default", "*");
                            
                            if (Dbase.ToLower().Contains("mysql"))
                            {
                                transform.AddRdfType("default", "automagic-mysql-"+et.TrimEnd().Replace(".", "-").Replace("_", "-"), et.TrimEnd());
                            
                                foreach (string f in concatPairedlist)
                                {        
                                    if (f != "")
                                    {
                                        if (f.TrimEnd().Split(";")[0] == et.TrimEnd())
                                        {        
                                            transform.AddMakeNi("default", f.TrimEnd().Split(";")[1]+"-"+f.TrimEnd().Split(";")[3], "automagic-mysql-"+f.TrimEnd().Split(";")[1], f.TrimEnd().Split(";")[2]);
                                        }
                                    }
                                }
                            }
                            
                            else if (Dbase.ToLower().Contains("postgresql"))
                            {
                                transform.AddRdfType("default", "automagic-postgresql-"+et.TrimEnd().Replace(".", "-").Replace("_", "-"), et.TrimEnd());
                            
                                foreach (string f in concatPairedlist)
                                {        
                                    if (f != "")
                                    {
                                        if (f.TrimEnd().Split(";")[0] == et.TrimEnd())
                                        {        
                                            transform.AddMakeNi("default", f.TrimEnd().Split(";")[1]+"-"+f.TrimEnd().Split(";")[2], "automagic-postgresql-"+f.TrimEnd().Split(";")[1], f.TrimEnd().Split(";")[3]);
                                        }
                                    }
                                }
                            }

                            pipe.WithSource(source).WithTransform(transform);    
                            
                            try
                            {
                                if (Dbase.ToLower().Contains("mysql"))
                                {
                                    client.DeletePipe(string
                                    .Format("automagic-mysql-{0}", et.TrimEnd()
                                    .Replace(".", "-")
                                    .Replace("_", "-")));
                                }
                                else if (Dbase.ToLower().Contains("postgresql"))
                                {
                                    client.DeletePipe(string
                                    .Format("automagic-postgresql-{0}", et.TrimEnd()
                                    .Replace(".", "-")
                                    .Replace("_", "-")));
                                }

                            }
                            catch (HttpRequestException ex)
                            {
                                Console.WriteLine($"The following error occurred when trying to configure a sesam pipe:\n {ex}");
                            }
                            finally
                            {
                                client.CreatePipe(pipe);
                            }
                        }
                    }
                }
                else if (Mapping.ToString().Contains("fkey"))
                {
                    //create a system
                    var client = new SesamNetCoreClient.Client(SesamJWT, SesamSubID);
                    SesamSystem system = null;
                    if (Dbase.ToLower().Contains("mysql"))
                    {
                        system = new SesamSystem(Namedb)
                            .OfType(SystemType.MYSQL)
                            .With("host", Host)
                            .With("database", Namedb)
                            .With("username", User)
                            .With("password", Password);
                    }
                    else if (Dbase.ToLower().Contains("postgresql"))
                    {
                        system = new SesamSystem(Namedb)
                            .OfType(SystemType.POSTGRESQL)
                            .With("host", Host)
                            .With("database", Namedb)
                            .With("username", User)
                            .With("password", Password);
                    }
                    
                    try
                    { 
                        client.CreateSystem(system);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine("Skipping the creation of system, because it already exists. Skipping with error: \n{0}", ex);
                    }

                    // create pipes
                    foreach (string o in tables.ToString().TrimEnd().Split("\n"))
                    {    
                        if (o != "")
                        {   
                            var namePrefix = o.TrimEnd()
                                .Replace(".", "-")
                                .Replace("_", "-");
                            var pipe = new Pipe(string
                                .Format("automagic-{0}", o.TrimEnd()
                                .Replace(".", "-")
                                .Replace("_", "-")));
                            
                            var source = new SqlSource();
                            source.SetTable(o.TrimEnd().Split('.')[1]);
                            source.SetSystem(Namedb);
                            source.SetType("sql");
                            source.SetKey("id");

                            var transform = new Transform();
                            transform.SetType("dtl");
                            transform.AddRule();
                            transform.MakeDefaultRule();
                            transform.AddCopy("default", "*");
                            transform.AddRdfType("default", "automagic-"+o.TrimEnd().Replace(".", "-").Replace("_", "-"), o.TrimEnd().Split('.')[1]);

                            foreach (string f in concatlist)
                            {        
                                if (f != "")
                                {
                                    if (f.TrimEnd().Split(";")[0] == o.TrimEnd().Split(".")[1])
                                    {        
                                        transform.AddMakeNi("default", o.TrimEnd().Replace(".", "-").Replace("_", "-").Replace(o.TrimEnd().Split(".")[1], f.TrimEnd().Split(";")[1])+"-"+f.TrimEnd().Split(";")[2], "automagic-"+o.TrimEnd().Replace(".", "-").Replace("_", "-").Replace(o.TrimEnd().Split(".")[1], f.TrimEnd().Split(";")[1]), f.TrimEnd().Split(";")[3]);
                                    }
                                }
                            }

                            pipe.WithSource(source).WithTransform(transform);    
                            
                            try
                            {
                                client.DeletePipe(string
                                .Format("automagic-{0}", o.TrimEnd()
                                .Replace(".", "-")
                                .Replace("_", "-")));
                            }
                            catch (HttpRequestException ex)
                            {
                                Console.WriteLine($"The following error occurred when trying to configure a sesam pipe:\n {ex}");
                            }
                            finally
                            {
                                client.CreatePipe(pipe);
                            }
                        }
                    }
                }
                tables = "";
                User = "";
                Dbase = "";
                Namedb = "";
                Host = "";
                Port = "";
                Password = "";
                Mapping = "";
                SesamJWT = "";
                SesamSubID = "";
                concatPairedlist = new List<string>();
                concatlist = new List<string>();
                return new OkObjectResult(request);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error occurred: " + ex.Message + " : " + ex.StackTrace);
                return new OkObjectResult(request);
            }
        }
    }
}