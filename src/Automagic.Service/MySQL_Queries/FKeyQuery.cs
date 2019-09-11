using Automagic.Core.FKeyAccess;
using System.Text;
using Automagic.Core.DataAccess;
using System;

namespace FKeyMapping
{
    public class FKeyQuery
    {
        public FKeyQuery(StringBuilder fKeyTables, StringBuilder niRefColumns, StringBuilder niRefTables, StringBuilder fKeyNiColumns, StringBuilder fKeyNiTables, Db db, string dbName)
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
                fKeyTables.AppendJoin(";", dbName + "." + lineReader.GetString(10));
                fKeyTables.Append(";");
                fKeyNiColumns.AppendLine(String.Format(lineReader.GetString(17)));
                niRefTables.AppendLine(String.Format(lineReader.GetString(21)));
                niRefColumns.AppendLine(String.Format(lineReader.GetString(22)));
                fKeyNiTables.AppendLine(String.Format(lineReader.GetString(16)));
            }
        }
        connect.Close();
        //Console.WriteLine("Writing fKey Tables");
        //Console.WriteLine(fKeyTables);
        //Console.WriteLine("Writing NI ref columns");
        //Console.WriteLine(niRefColumns);
        //Console.WriteLine("Writing NI ref tables");
        //Console.WriteLine(niRefTables);
        //Console.WriteLine("Writing NI tables");
        //Console.WriteLine(niTables);
        }
    }
}

//Now printing columns for testing
//Writing fKey Tables
//mysql_VBJMMMDQ.company;
//Writing NI ref columns
//id_company
//Writing NI ref tables
//company
//Writing NI tables
//customer