using System;
using System.Text;
using Automagic.Core.DataAccess;
using Automagic.Core.IndexAccess;

namespace IndexRefMapping
{
    public class IndexRefQuery
    {
        public IndexRefQuery(StringBuilder allTables, StringBuilder piiIndxTables, StringBuilder indexColumns, StringBuilder allColumns, StringBuilder tablesWithIndxRefs, StringBuilder pairingIndxTables, StringBuilder pairingIndxColumns, StringBuilder columnsWithIndxRefs, Db db, string dbName)
        {
            // Some logic here for finding index reference tables
            IndxReference sqlstring_for_index_refs = new IndxReference();
            var connect_for_refs = db.GetConnection();
            connect_for_refs.Open();
            int run = 1;
            
            foreach (string i in allTables.ToString().Split("\n"))
            {
                foreach (string f in piiIndxTables.ToString().Split("\n"))
                {
                    foreach (string d in indexColumns.ToString().Split("\n"))
                    {
                        foreach (string k in allColumns.ToString().Split("\n"))
                        {
                            if (i != "" && f != "" && d != "" && k != "") 
                            {
                                try
                                {
                                    //Console.WriteLine("Printing iteration count : {0}", run);
                                    var Index_refs = db.GetCommand(sqlstring_for_index_refs.GetMatchedValues(i, f, d, k), connect_for_refs);
                                    Index_refs.Prepare();  
                                    var linReader = Index_refs.ExecuteScalar();
                                    if (linReader.ToString() != "")
                                    //Console.WriteLine(linReader);
                                    //Console.WriteLine("Printing for NI");
                                    {
                                        if (tablesWithIndxRefs.ToString().Contains(i) == false || pairingIndxTables.ToString().Contains(f) == false || pairingIndxColumns.ToString().Contains(d) == false || columnsWithIndxRefs.ToString().Contains(k) == false)
                                        {
                                            tablesWithIndxRefs.AppendLine(String.Format(i));                                         
                                            pairingIndxTables.AppendLine(String.Format(f));
                                            pairingIndxColumns.AppendLine(String.Format(d));
                                            columnsWithIndxRefs.AppendLine(String.Format(k));
                                        }
                                    }
                                    run++;
                                }
                                catch (Exception ex)
                                {
                                    //Console.WriteLine("The following tables does not have indexes that match, and so returns an empty string : \n{0}", ex);
                                }
                            }
                        }
                    }
                }
            }
            //Console.WriteLine("Printing Columns with indx refs -------------");
            //Console.WriteLine(columnsWithIndxRefs);
            //Console.WriteLine("Printing PairingPiiColumns --------------");
            //Console.WriteLine(pairingIndxColumns);
            connect_for_refs.Close();
            //Console.WriteLine("Printing the tables that match");
            //Console.WriteLine(tablesWithIndxRefs);
            //Console.WriteLine("------------------------------");
        }
    }
}

//Now printing columns for testing
//Printing PairingPiiTables -------------
//customer
//detailed_registration
//company
//company
//customer
//simple_registration
//detailed_registration
//customer

//Printing PairingPiiColumns --------------
//id
//email
//email
//id_company
//email
//email
//email
//id

//Printing the tables that match
//company
//customer
//customer
//customer
//detailed_registration
//detailed_registration
//simple_registration
//user_agent