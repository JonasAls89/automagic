using System;
using System.Text;
using Automagic.Core.DataAccess;
using Automagic.Core.IndexAccess;
using Automagic.Core.FKeyAccess;

namespace GetAllTablesAndColumns
{
    public class GetAllQuery
    {
        public GetAllQuery(StringBuilder allColumns, StringBuilder allTables, Db db, string dbName)
        {
            // Some logic here for finding pii columns
            IndxReference sqlstring_for_pii_columns = new IndxReference();
            var connect_for_columns = db.GetConnection();
            connect_for_columns.Open();
            var Pii_columns = db.GetCommand(sqlstring_for_pii_columns.GetQueryForAllTableColumns(dbName), connect_for_columns);
            Pii_columns.Prepare();
            var lineRead = Pii_columns.ExecuteReader();
            int cnt = 1;
            while(lineRead.Read()) {
                for(int e = 0 ; e < cnt ; e++) {
                    if (allColumns.ToString().Contains(lineRead.GetString(3)) == false)
                    {
                        allColumns.AppendLine(lineRead.GetString(3));
                    }
                    if (allTables.ToString().Contains(lineRead.GetString(2)) == false)
                    {
                        allTables.AppendLine(lineRead.GetString(2));
                    }
                }
            }
            //Console.WriteLine("Validating All Columns\n " + allColumns);
            //Console.WriteLine("Validating All Tables\n " + allTables);
            connect_for_columns.Close();
        }
    }

    public class GetAllQueryPostGreSQL
    {
        public GetAllQueryPostGreSQL(StringBuilder allColumns, StringBuilder allTables, Db db, string dbName)
        {
            // Some logic here for finding all tables and columns
            FKeyReference sqlstring_for_pii_columns = new FKeyReference();
            var connect_for_columns = db.GetConnection();
            connect_for_columns.Open();
            var Pii_columns = db.GetCommand(sqlstring_for_pii_columns.GetAllTablesAndColumns(dbName), connect_for_columns);
            Pii_columns.Prepare();
            var lineRead = Pii_columns.ExecuteReader();
            int cnt = 1;
            while(lineRead.Read()) {
                for(int e = 0 ; e < cnt ; e++) {
                    if (allColumns.ToString().Contains(lineRead.GetString(5)) == false)
                    {
                        allColumns.AppendLine(lineRead.GetString(5));
                    }
                    if (allTables.ToString().Contains(lineRead.GetString(4)) == false)
                    {
                        allTables.AppendLine(lineRead.GetString(4));
                    }
                }
            }
            //Console.WriteLine("Validating All Columns\n " + allColumns);
            //Console.WriteLine("Validating All Tables\n " + allTables);
            connect_for_columns.Close();
        }
    }
}

//Validating All Columns
//id
//name
//sdate
//email
//domain
//city
//lastname
//address
//country
//registry_date
//birthdate
//phone_number
//locale
//id_company
//password
//ip
//countrycode
//useragent

//Validating All Tables
//company
//customer
//detailed_registration
//simple_registration
//user_agent