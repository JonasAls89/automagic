using Automagic.Core.IndexAccess;
using System.Text;
using Automagic.Core.DataAccess;
using System;
using Npgsql.PostgresTypes;

namespace PostgreSQLIndexMapping
{
    public class IndexQueryPostgreSQL
    {
        public IndexQueryPostgreSQL(StringBuilder indexTables, StringBuilder indexColumns, Db db)
        {
            // Some logic here for finding index column names and table names
            IndxReference sqlstring_for_index_tables = new IndxReference();
            var connect_for_tables = db.GetConnection();
            //Console.WriteLine(connect.State);
            connect_for_tables.Open();
            //Console.WriteLine(connect.State);                
            var Columns = db.GetCommand(sqlstring_for_index_tables.GetIndexTablesPostgreSQL(), connect_for_tables);
            Columns.Prepare();
            //Console.WriteLine("Prepared");
            
            var lineReading = Columns.ExecuteReader();
            int counts = 1;
            while(lineReading.Read()) {
                for(int e = 0 ; e < counts ; e++) {
                    if (indexTables.ToString().Contains(lineReading.GetString(1)) == false)
                    {
                        indexTables.AppendLine(lineReading.GetString(1));
                        indexTables.ToString().Split("_");
                    }
                }
            }
            connect_for_tables.Close();

            IndxReference sqlstring_for_index_columns = new IndxReference();
            var connect_for_columns = db.GetConnection();
            connect_for_columns.Open();
            var Column = db.GetCommand(sqlstring_for_index_columns.GetIndexColumnsPostgreSQL(), connect_for_columns);
            Column.Prepare();
            //Console.WriteLine("Prepared");
            
            var lineRead = Column.ExecuteReader();
            int count = 1;
            while(lineRead.Read()) {
                for(int e = 0 ; e < count ; e++) {
                    if (indexColumns.ToString().Contains(lineRead.GetString(5)) == false)
                    {
                        indexColumns.AppendLine(lineRead.GetString(5));
                    }
                }
            }
            connect_for_columns.Close();

            //Console.WriteLine("Writing index tables ----------------");
            //Console.WriteLine(indexTables);
            //Console.WriteLine("Writing index columns ----------------");
            //Console.WriteLine(indexColumns);
        }
    }
}