using System;

namespace Automagic.Core.IndexAccess
{
    public class IndxReference : Indx
    {           
        public override string GetQueryForIndexColumnAndTableName(string db_name)
        {
            return String.Format("SELECT * FROM information_schema.statistics WHERE table_schema = '{0}'", db_name); 
        }         
        public override string GetQueryForAllTableColumns(string db_name)
        {
            return String.Format("SELECT  * FROM information_schema.columns WHERE table_schema = '{0}'", db_name);
        }
        public override string GetMatchedValues(string index_table_name, string pii_table_name, string index_column_name, string all_columns_in_pii_table)
        {
            string sql_query = String.Format("SELECT * FROM {0} JOIN {1} WHERE {0}.{2} = {1}.{3} LIMIT 500", index_table_name, pii_table_name, index_column_name, all_columns_in_pii_table);
            //Console.WriteLine("Now printing SQL string : \n {0}", sql_query);
            return sql_query; 
        }

        public override string GetIndexColumnsPostgreSQL()
        {   
            string sql_string = @"
            SELECT i.relname as indname,
                   i.relowner as indowner,
                   idx.indrelid::regclass as tables,
                   am.amname as indam,
                   idx.indkey,(
                   SELECT pg_get_indexdef(idx.indexrelid, k + 1, true)
                   FROM generate_subscripts(idx.indkey, 1) as k
                   ORDER BY k
                   ) as indkey_names,
                   idx.indexprs IS NOT NULL as indexprs,
                   idx.indpred IS NOT NULL as indpred
            FROM   pg_index as idx
            JOIN   pg_class as i
            ON     i.oid = idx.indexrelid
            JOIN   pg_am as am
            ON     i.relam = am.oid
            JOIN   pg_namespace as ns
            ON     ns.oid = i.relnamespace
            AND    ns.nspname = ANY(current_schemas(false))";
            
            return String.Format(sql_string);
        }

        public override string GetIndexTablesPostgreSQL()
        {
            return String.Format("SELECT * FROM pg_indexes where schemaname='public'");
        }   
    }     
}