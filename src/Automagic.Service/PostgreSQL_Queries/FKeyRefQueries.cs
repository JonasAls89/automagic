using System;
using System.Text;
using System.Collections.Generic;
using Automagic.Core.DataAccess;
using Automagic.Core.FKeyAccess;

namespace FKeyRefQueries
{
    public class FKeyMatchQueries
    {
        public FKeyMatchQueries(StringBuilder fKeyTablesPostgreSQL, StringBuilder allTables, StringBuilder fKeyColumnsPostgreSQL, StringBuilder allColumns, StringBuilder concatenatedList, Db db, string dbName)
        {
            // Some logic here for finding index reference tables
            FKeyReference sqlstring_for_fkey_refs = new FKeyReference();
            var connect_for_refs = db.GetConnection();
            connect_for_refs.Open();
            int run = 1;
            
            foreach (string i in fKeyTablesPostgreSQL.ToString().TrimEnd().Split("\n"))
            {
                foreach (string f in allTables.ToString().TrimEnd().Split("\n"))
                {
                    foreach (string d in fKeyColumnsPostgreSQL.ToString().TrimEnd().Split("\n"))
                    {
                        foreach (string k in allColumns.ToString().TrimEnd().Split("\n"))
                        {
                            if (i != f)
                            {
                                try
                                {
                                    //Console.WriteLine("Printing iteration count : {0}", run);
                                    var Index_refs = db.GetCommand(sqlstring_for_fkey_refs.PostgreSQLMatchedFkeyTables(i, f, d, k), connect_for_refs);
                                    Index_refs.Prepare();  
                                    var linReader = Index_refs.ExecuteScalar();
                                    if (linReader.ToString() != "")
                                    {
                                        //Console.WriteLine("--------------------------------");
                                        concatenatedList.AppendLine(i.TrimEnd()+";"+f.TrimEnd()+";"+k.TrimEnd()+";"+d.TrimEnd());
                                        //Console.WriteLine(concatenatedList); 
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
            connect_for_refs.Close();
        }
    }
}