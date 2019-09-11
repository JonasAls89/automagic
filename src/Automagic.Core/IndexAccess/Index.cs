namespace Automagic.Core.IndexAccess
{
    public abstract class Indx
    {        
        public abstract string GetQueryForIndexColumnAndTableName(string db_name);
        public abstract string GetQueryForAllTableColumns(string db_name);
        public abstract string GetMatchedValues(string index_table_name, string pii_table_name, string index_column_name, string all_columns_in_pii_table);
        public abstract string GetIndexTablesPostgreSQL();

        public abstract string GetIndexColumnsPostgreSQL();
    }
}


