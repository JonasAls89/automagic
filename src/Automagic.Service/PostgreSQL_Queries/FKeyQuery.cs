using Automagic.Core.FKeyAccess;
using System.Text;
using Automagic.Core.DataAccess;
using System;

namespace FKeyMappingPostgreSQL
{
    public class FKeyQueryPostgreSQL
    {
        public FKeyQueryPostgreSQL(StringBuilder fKeyTables, StringBuilder fKeyColumns, Db db, string dbName)
        {
        // Some logic here for finding fkey reference tables
        FKeyReference sqlstring = new FKeyReference();
        var connect = db.GetConnection();
        //Console.WriteLine(connect.State);
        connect.Open();
        //Console.WriteLine(connect.State);                
        var FKey = db.GetCommand(sqlstring.GetQueryForFKeyTables(), connect);
        FKey.Prepare();
        //Console.WriteLine("Prepared");
        var lineReader = FKey.ExecuteReader();
        int count = 1;
        while(lineReader.Read()) {
            for(int i = 0 ; i < count ; i++) {
                //Console.WriteLine(lineReader.GetString(5));
                fKeyTables.AppendLine(String.Format(lineReader.GetString(14)));
                fKeyColumns.AppendLine(String.Format(lineReader.GetString(15)));
            }
        }
        connect.Close();
        //Console.WriteLine("Writing fKey Tables");
        //Console.WriteLine(fKeyTables);
        //Console.WriteLine("Writing fKey Columns");
        //Console.WriteLine(fKeyColumns);
        //Console.WriteLine("Writing NI ref tables");
        //Console.WriteLine(niRefTables);
        //Console.WriteLine("Writing NI tables");
        //Console.WriteLine(niTables);
        }
    }
}