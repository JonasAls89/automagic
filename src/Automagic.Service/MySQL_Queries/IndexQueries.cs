using Automagic.Core.IndexAccess;
using System.Text;
using Automagic.Core.DataAccess;
using System;

namespace IndexMapping
{
    public class IndexQuery
    {
        public IndexQuery(StringBuilder indexColumns, StringBuilder indexTables, Db db, string dbName)
        {
            // Some logic here for finding index column names and table names
            IndxReference sqlstring_for_index_columns = new IndxReference();
            var connect_for_columns_and_tables = db.GetConnection();
            //Console.WriteLine(connect.State);
            connect_for_columns_and_tables.Open();
            //Console.WriteLine(connect.State);                
            var Columns = db.GetCommand(sqlstring_for_index_columns.GetQueryForIndexColumnAndTableName(dbName), connect_for_columns_and_tables);
            Columns.Prepare();
            //Console.WriteLine("Prepared");
            var lineReading = Columns.ExecuteReader();
            int counts = 1;
            while(lineReading.Read()) {
                for(int e = 0 ; e < counts ; e++) {
                    if (indexColumns.ToString().Contains(lineReading.GetString(7)) == false)
                    {
                        indexColumns.AppendLine(lineReading.GetString(7));
                    }
                    if (indexTables.ToString().Contains(lineReading.GetString(2)) == false)
                    {
                        indexTables.AppendLine(lineReading.GetString(2));
                    }
                }
            }
            connect_for_columns_and_tables.Close();
            //Console.WriteLine("Writing index columns --------------");
            //Console.WriteLine(indexColumns);
            //Console.WriteLine("Writing index tables ----------------");
            //Console.WriteLine(indexTables);
        }
    }
}

//Writing index columns --------------
//id
//email
//id_company

//Writing index tables ----------------
//company
//customer
//detailed_registration
//simple_registration
//user_agent