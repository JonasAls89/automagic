namespace Automagic.Core.FKeyAccess
{
    public abstract class FKey
    {        
        public abstract string GetQueryForFKeyTables();

        public abstract string GetAllTablesAndColumns(string dbName);

        public abstract string PostgreSQLMatchedFkeyTables(string fkey_table, string table, string fkey_column, string column);
    }
}


