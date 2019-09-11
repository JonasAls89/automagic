using System;
using System.Text;
using System.IO;
using System.Linq;
using FKeyMapping;
using IndexMapping;
using GetAllTablesAndColumns;
using IndexRefMapping;
using IndexRefMappingPostgreSQL;
using PostgreSQLIndexMapping;
using FKeyMappingPostgreSQL;
using FKeyRefQueries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net;
using Automagic.Core;
using Automagic.Core.DataAccess;

namespace Automagic.Test
{
    [TestClass]
    public class QueryTests
    {
        [TestMethod]
        public void TestMethodFKeyMapping()
        {
            StringBuilder fKeyTables = new StringBuilder();
            StringBuilder niRefColumns = new StringBuilder();
            StringBuilder niRefTables = new StringBuilder();
            StringBuilder fKeyNiTables = new StringBuilder();
            StringBuilder fKeyNiColumns = new StringBuilder();
            string connSql = string.Format("User ID=postgres;Password=/(/YHIkuhskdf987yhgik;Host=35.228.48.175;Port=5432;Pooling=true;Database=test_db2");
            var db = new PostgreSqlDb(connSql); 
            string dbName = "test_db2";
            
            FKeyQuery query = new FKeyQuery(fKeyTables, niRefColumns, niRefTables, fKeyNiColumns, fKeyNiTables, db, dbName); 

            Console.WriteLine(fKeyNiTables);

            // Logic here for correctly creating NI in a concatenated list for foreign key mapping
            var niRefColumnsArray = niRefColumns.ToString().TrimEnd().Split("\n");
            Console.WriteLine("Now writing {0}", niRefColumnsArray);
            var niRefTablesArray = niRefTables.ToString().TrimEnd().Split("\n");
            Console.WriteLine("Now writing {0}", niRefTablesArray);
            var fKeyNiTablesArray = fKeyNiTables.ToString().TrimEnd().Split("\n");
            Console.WriteLine("Now writing {0}", fKeyNiTablesArray);
            var fKeyNiColumnsArray = fKeyNiColumns.ToString().TrimEnd().Split("\n");
            Console.WriteLine("Now writing {0}", fKeyNiColumnsArray);
            List<string> concatPairedlist = new List<string>();
            int dk = 0;
            foreach(var e in niRefTablesArray)
            {
                concatPairedlist.Add(fKeyNiTablesArray[dk] + ";" + e + ";" + niRefColumnsArray[dk] + ";" + fKeyNiColumnsArray[dk]);
                dk++;
            }
            concatPairedlist.ForEach(i => Console.Write("{0}\t", i));

            foreach (string et in fKeyNiTables.ToString().Split('\n'))
            {   
                if (et != "")
                {
                    foreach (string f in concatPairedlist)
                    {        
                        if (f != "")
                        {
                            if (f.Split(";")[0] == et)
                            {
                                Console.WriteLine("printing matching tables---------");
                                Console.WriteLine(et);
                                Console.WriteLine(f);
                                Console.WriteLine("---------------------------------");
                            }
                        }   
                    }
                }
            }

            // Assert statements
            Assert.AreEqual("mysql_VBJMMMDQ.company;", fKeyTables.ToString());
            Assert.AreEqual("id", niRefColumns.ToString().TrimEnd());
            Assert.AreEqual("company", niRefTables.ToString().TrimEnd());
            Assert.AreEqual("id_company", fKeyNiColumns.ToString().TrimEnd());
            Assert.AreEqual("customer", fKeyNiTables.ToString().TrimEnd());
            Assert.AreEqual("customer;company;id;id_company", concatPairedlist.ToString());
            //
        }

        [TestMethod]
        public void TestMethodIndexMapping()
        {
            StringBuilder indexColumns = new StringBuilder();
            StringBuilder indexTables = new StringBuilder();
            string connSql = string.Format("User ID=postgres;Password=/(/YHIkuhskdf987yhgik;Host=35.228.48.175;Port=5432;Pooling=true;Database=test_db2");
            var db = new PostgreSqlDb(connSql);
            string dbName = "test_db2";
            
            IndexQuery indxQuery = new IndexQuery(indexColumns, indexTables, db, dbName);
        
            // Assert statements
            Assert.AreEqual("id\nemail\nid_company", indexColumns.ToString().TrimEnd());
            Assert.AreEqual("company\ncustomer\ndetailed_registration\nsimple_registration\nuser_agent", indexTables.ToString().TrimEnd());
            //
        }

        [TestMethod]
        public void TestMethodGetAllTablesAndColumns()
        {
            StringBuilder allColumns = new StringBuilder();
            StringBuilder allTables = new StringBuilder();
            string connSql = string.Format("User ID=postgres;Password=/(/YHIkuhskdf987yhgik;Host=35.228.48.175;Port=5432;Pooling=true;Database=test_db2");
            var db = new PostgreSqlDb(connSql); 
            string dbName = "test_db2";

            GetAllQuery getAllQuery = new GetAllQuery(allColumns, allTables, db, dbName);
            
            string expectedColumns = "id\nname\nsdate\nemail\ndomain\ncity\nlastname\naddress\ncountry\nregistry_date\nbirthdate\nphone_number\nlocale\nid_company\npassword\nip\ncountrycode\nuseragent";

            string expectedTables = "company\ncustomer\ndetailed_registration\nsimple_registration\nuser_agent";

            // Assert statements
            Assert.AreEqual(expectedColumns, allColumns.ToString().TrimEnd());
            Assert.AreEqual(expectedTables, allTables.ToString().TrimEnd());
            //
        }

        [TestMethod]
        public void TestMethodIndexRefMapping()
        {
            string connSql = string.Format("User ID=postgres;Password=/(/YHIkuhskdf987yhgik;Host=35.228.48.175;Port=5432;Pooling=true;Database=test_db2");
            var db = new PostgreSqlDb(connSql); 
            string dbName = "test_db2";
            StringBuilder allTables = new StringBuilder();
            StringBuilder allColumns = new StringBuilder();
            GetAllQuery getAllQuery = new GetAllQuery(allColumns, allTables, db, dbName);
            StringBuilder piiIndxTables = new StringBuilder("customer\ndetailed_registration\nsimple_registration\ncompany");
            StringBuilder indexColumns = new StringBuilder();
            StringBuilder indexTables = new StringBuilder();
            IndexQuery indxQuery = new IndexQuery(indexColumns, indexTables, db, dbName);
            StringBuilder tablesWithIndxRefs = new StringBuilder();
            StringBuilder pairingIndxTables = new StringBuilder();
            StringBuilder pairingIndxColumns = new StringBuilder();
            StringBuilder columnsWithIndxRefs = new StringBuilder();
            StringBuilder test = new StringBuilder();

            IndexRefQuery indxRefQuery = new IndexRefQuery(allTables, piiIndxTables, indexColumns, allColumns, tablesWithIndxRefs, pairingIndxTables, pairingIndxColumns, columnsWithIndxRefs, db, dbName);

            foreach (string e in tablesWithIndxRefs.ToString().TrimEnd().Split("\n"))
            {
                if (test.ToString().Contains(e) == false)
                {
                    test.AppendLine(e.TrimEnd());
                } 
            }

            Console.WriteLine(test);

            string expectedPairingIndxTables = "customer\ndetailed_registration\ncompany\ncompany\ncustomer\nsimple_registration\ndetailed_registration\ncustomer";

            string expectedPairingIndxColumns = "id\nemail\nemail\nid_company\nemail\nemail\nemail\nid";

            string expectedTablesWithIndxRefs = "company\ncustomer\ncustomer\ncustomer\ndetailed_registration\ndetailed_registration\nsimple_registration\nuser_agent";

            string expectedColumnsWithIndxRefs = "id_company\nemail\ndomain\nid\nemail\nemail\nemail\nid";


            // Assert statements
            Assert.AreEqual(expectedPairingIndxTables, pairingIndxTables.ToString().TrimEnd());
            Assert.AreEqual(expectedPairingIndxColumns, pairingIndxColumns.ToString().TrimEnd());
            Assert.AreEqual(expectedTablesWithIndxRefs, tablesWithIndxRefs.ToString().TrimEnd());
            Assert.AreEqual(expectedColumnsWithIndxRefs, columnsWithIndxRefs.ToString().TrimEnd());
            //
        
        }

        [TestMethod]
        public void TestMethodPipeNiCreation()
        {
            StringBuilder niCreated = new StringBuilder();
            StringBuilder piiIndxTables = new StringBuilder("company\ncustomer\ndetailed_registration\nsimple_registration\nuser_agent");
            StringBuilder pairingIndxTables = new StringBuilder("customer\ndetailed_registration\ncompany\ncompany\ncustomer\nsimple_registration\ndetailed_registration\ncustomer");
            StringBuilder pairingIndxColumns = new StringBuilder("id\nemail\nemail\nid_company\nemail\nemail\nemail\nid");
            StringBuilder tablesWithIndxRefs = new StringBuilder("company\ncustomer\ncustomer\ncustomer\ndetailed_registration\ndetailed_registration\nsimple_registration\nuser_agent");
            StringBuilder columnsWithIndxRefs = new StringBuilder("id_company\nemail\ndomain\nid\nemail\nemail\nemail\nid");

            var tablesWithIndxRefsArray = tablesWithIndxRefs.ToString().Split("\n");
            var columnsWithIndxRefsArray = columnsWithIndxRefs.ToString().Split("\n");
            var pairingIndxTablesArray = pairingIndxTables.ToString().Split("\n");
            var pairingIndxColumnsArray = pairingIndxColumns.ToString().Split("\n");
            List<string> concatPairedlist = new List<string>();
            int i = 0;
            foreach(string e in pairingIndxTablesArray)
            {
                concatPairedlist.Add(tablesWithIndxRefsArray[i] + ";" + e + ";" + pairingIndxColumnsArray[i] + ";" + columnsWithIndxRefsArray[i]);
                //Console.WriteLine(e);
                //Console.WriteLine(pairingIndxColumnsArray[i]);
                i++;   
            }     
            foreach (string et in piiIndxTables.ToString().Split('\n'))
            {   
                if (et != "")
                {
                    foreach (string f in concatPairedlist)
                    {        
                        if (f != "")
                        {
                            if (f.TrimEnd().Split(";")[0] == et)
                            {
                                Console.WriteLine("printing matching tables---------");
                                Console.WriteLine(et);
                                Console.WriteLine(f);
                                Console.WriteLine("---------------------------------");
                            }
                        }   
                    }
                }
            }
            //string expectedResultCompany = "[customer, id], [customer, email],[customer, email],[customer, id_company],[detailed_registration, id],[detailed_registration, email],[]";
            //string expectedResultCustomer = "[]";

            // Assert statements
            Assert.AreEqual("something unlikely", pairingIndxTables.ToString().TrimEnd());
            //    
        }

        [TestMethod]
        public void TestMethodReadFilesFromAzure()
        {
            WebClient myWebClient = new WebClient();
            Stream firstnames = myWebClient.OpenRead("https://csbf8630753f67dx4346x85e.blob.core.windows.net/automagic-test/firstnames.csv");
            Stream alllastnames = myWebClient.OpenRead("https://csbf8630753f67dx4346x85e.blob.core.windows.net/automagic-test/alllastnames.csv");

            var refData = new ReferenceDataBlobs(firstnames, alllastnames);
            firstnames.Close();
            alllastnames.Close();

            Assert.AreEqual("Something note likely", firstnames);

        }

        [TestMethod]
        public void TestMethodFKeyQueryPostgreSQL()
        {
            string connSql = string.Format("User ID=postgres;Password=/(/YHIkuhskdf987yhgik;Host=35.228.48.175;Port=5432;Pooling=true;Database=test_db2");
            var db = new PostgreSqlDb(connSql); 
            string dbName = "test_db2";
            StringBuilder fKeyTables = new StringBuilder();
            StringBuilder fKeyColumns = new StringBuilder();
            FKeyQueryPostgreSQL getFKeys = new FKeyQueryPostgreSQL(fKeyTables, fKeyColumns, db, dbName);

            Assert.AreEqual("customer", fKeyTables.ToString().Trim());
            Assert.AreEqual("company_id", fKeyColumns.ToString().Trim());
        }

        [TestMethod]
        public void TestMethodFKeyReferenceMatch()
        {
            string connSql = string.Format("User ID=postgres;Password=/(/YHIkuhskdf987yhgik;Host=35.228.48.175;Port=5432;Pooling=true;Database=test_db2");
            var db = new PostgreSqlDb(connSql); 
            string dbName = "test_db2";
            StringBuilder concatList = new StringBuilder();
            StringBuilder fKeyTables = new StringBuilder("customer"); 
            //Console.WriteLine(fKeyTables);
            StringBuilder allTables = new StringBuilder("company\ncustomer\ndetailed_registration\nsimple_registration\nuser_agent"); 
            //Console.WriteLine(allTables); 
            StringBuilder fKeyColumns = new StringBuilder("company_id");
            //Console.WriteLine(fKeyColumns); 
            StringBuilder allColumns = new StringBuilder("id\nname\nsdate\nemail\ndomain\ncity\nlastname\naddress\ncountry\nregistry_date\nbirthdate\nphone_number\nlocale\nid_company\npassword\nip\ncountrycode\nuseragent");
            //Console.WriteLine(allColumns); 
            FKeyMatchQueries matching = new FKeyMatchQueries(fKeyTables, allTables, fKeyColumns, allColumns, concatList, db, dbName);
            List<string> test = new List<string>();

            var concatListArray = concatList.ToString().TrimEnd().Split("\n");     
            int no = 0;
            foreach(var e in concatListArray)
            {
                test.Add(e);
                no++;
            }

            foreach (string f in test)
            {
                Console.WriteLine(f);
            }

            foreach (string et in allTables.ToString().Split('\n'))
            {   
                if (et != "")
                {
                    foreach (string f in test)
                    {        
                        if (f != "")
                        {
                            if (f.TrimEnd().Split(";")[0] == et)
                            {
                                Console.WriteLine("printing matching tables---------");
                                Console.WriteLine(et);
                                Console.WriteLine(f);
                                Console.WriteLine("---------------------------------");
                            }
                        }   
                    }
                }
            }

            Assert.AreEqual("Something Highly Unlikely", concatList);
        }

        [TestMethod]
        public void TestMethodFooBar()
        {
            List<string> concatlist = new List<string>();
            string connSql = string.Format("User ID=postgres;Password=/(/YHIkuhskdf987yhgik;Host=35.228.48.175;Port=5432;Pooling=true;Database=test_db2");
            var db = new PostgreSqlDb(connSql); 
            string dbName = "test_db2";


            //Setting all columns and tables variables from column schema 
            StringBuilder allColumns = new StringBuilder();
            StringBuilder allTables = new StringBuilder();
            GetAllQueryPostGreSQL queryAll = new GetAllQueryPostGreSQL(allColumns, allTables, db, dbName);
            string id = "id";
            allColumns.AppendLine(id);
            //Console.WriteLine(allColumns.ToString().TrimEnd());
            //Console.WriteLine(allTables.ToString().TrimEnd());
            
            //Setting fkey refs for postgresql
            //StringBuilder fKeyTablesPostgreSQL = new StringBuilder("customer");
            //StringBuilder fKeyColumnsPostgreSQL = new StringBuilder("company_id");
            StringBuilder fKeyTablesPostgreSQL = new StringBuilder();
            StringBuilder fKeyColumnsPostgreSQL = new StringBuilder();
            FKeyQueryPostgreSQL getFKeys = new FKeyQueryPostgreSQL(fKeyTablesPostgreSQL, fKeyColumnsPostgreSQL, db, dbName);
            //Console.WriteLine(fKeyTablesPostgreSQL.ToString().TrimEnd());
            //Console.WriteLine(fKeyColumnsPostgreSQL.ToString().TrimEnd());

            //Getting matched values
            StringBuilder concatenatedList = new StringBuilder();
            FKeyMatchQueries query = new FKeyMatchQueries(fKeyTablesPostgreSQL, allTables, fKeyColumnsPostgreSQL, allColumns, concatenatedList, db, dbName);
            //----------------------------------

            // Logic here for correcly creating NI in a concatenated list for foreign key mapping
            var concatListArray = concatenatedList.ToString().TrimEnd().Split("\n");     
            int no = 0;
            foreach(var e in concatListArray)
            {
                concatlist.Add(e);
                no++;
            }

            foreach (string f in concatlist)
            {
                Console.WriteLine(f);
            }

            foreach (string et in allTables.ToString().TrimEnd().Split('\n'))
            {   
                if (et != "")
                {
                    foreach (string f in concatlist)
                    {        
                        if (f != "")
                        {
                            if (f.TrimEnd().Split(";")[0] == et.TrimEnd())
                            {
                                Console.WriteLine("printing matching tables---------");
                                Console.WriteLine(et);
                                Console.WriteLine(f);
                                Console.WriteLine("---------------------------------");
                            }
                        }   
                    }
                }
            }

            Assert.AreEqual("Something Highly Unlikely", concatlist);
        }

        [TestMethod]

        public void TestMethodPostgreSQLIndexMapping()
        {
            List<string> concatlist = new List<string>();
            string connSql = string.Format("User ID=postgres;Password=/(/YHIkuhskdf987yhgik;Host=35.228.48.175;Port=5432;Pooling=true;Database=test_db2");
            var db = new PostgreSqlDb(connSql); 
            string dbName = "test_db2";

            //Setting all columns and tables variables from column schema 
            StringBuilder allColumns = new StringBuilder();
            StringBuilder allTables = new StringBuilder();
            GetAllQueryPostGreSQL queryAll = new GetAllQueryPostGreSQL(allColumns, allTables, db, dbName);
            string id = "id";
            allColumns.AppendLine(id);
            //Console.WriteLine(allColumns.ToString().TrimEnd());
            //Console.WriteLine(allTables.ToString().TrimEnd());


            // Setting indexTables
            StringBuilder indexTables = new StringBuilder();
            StringBuilder indexColumns = new StringBuilder();
            IndexQueryPostgreSQL index = new IndexQueryPostgreSQL(indexTables, indexColumns, db);                        
            //---------------------------------

            //Getting matched values
            StringBuilder concatenatedList = new StringBuilder();
            IndexRefQueryPostgreSQL indexRef = new IndexRefQueryPostgreSQL(allTables, indexTables, allColumns, indexColumns, concatenatedList, db, dbName);
            //---------------------------------

            // Logic here for correcly creating NI in a concatenated list for foreign key mapping
            var concatListArray = concatenatedList.ToString().TrimEnd().Split("\n");     
            int no = 0;
            foreach(var e in concatListArray)
            {
                concatlist.Add(e);
                no++;
            }

            foreach (string f in concatlist)
            {
                Console.WriteLine(f);
            }

            Assert.AreEqual("Something Highly Unlikely", concatlist);

        }
        
    }
}