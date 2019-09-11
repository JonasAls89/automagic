using System;
using System.Text;
using Automagic.Core.DataAccess;
using Automagic.Core.FKeyAccess;

namespace IndexRefMappingPostgreSQL
{
    public class IndexRefQueryPostgreSQL
    {
        public IndexRefQueryPostgreSQL(StringBuilder allTables, StringBuilder indexTables, StringBuilder allColumns, StringBuilder indexColumns, StringBuilder concatenatedList, Db db, string Namedb)
        {
            // Some logic here for finding index reference tables
            FKeyReference sqlstring_for_index_refs = new FKeyReference();
            var connect_for_refs = db.GetConnection();
            connect_for_refs.Open();
            int run = 1;
            
            foreach (string i in indexTables.ToString().Split("\n"))
            {
                foreach (string f in allTables.ToString().Split("\n"))
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
                                    var Index_refs = db.GetCommand(sqlstring_for_index_refs.PostgreSQLMatchedFkeyTables(i, f, d, k), connect_for_refs);
                                    Index_refs.Prepare();  
                                    var linReader = Index_refs.ExecuteScalar();
                                    if (linReader.ToString() != "")
                                    //Console.WriteLine(linReader);
                                    //Console.WriteLine("Printing for NI");
                                    {
                                        concatenatedList.AppendLine(i.TrimEnd()+";"+f.TrimEnd()+";"+k.TrimEnd()+";"+d.TrimEnd());
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

            Console.WriteLine("What is being concatenated : " + concatenatedList);
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