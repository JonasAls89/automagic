using System;

namespace Automagic.Core.FKeyAccess
{
    public class FKeyReference : FKey
    {           
        public override string GetQueryForFKeyTables()
        {
            return String.Format(sql_string);
        }
        string sql_string = @"
            SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC
            
            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU1
                ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG
                AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA
                AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME"
        ; 

        public override string GetAllTablesAndColumns(string dbName)
        {
            return String.Format("SELECT * FROM information_schema.role_column_grants where table_catalog='{0}'", dbName);
        }

        public override string PostgreSQLMatchedFkeyTables(string fkey_table, string table, string fkey_column, string column)
        {
            return String.Format("SELECT * FROM {0}, {1} where {0}.{2}={1}.{3}", fkey_table, table, fkey_column, column);
        }

    }        
}