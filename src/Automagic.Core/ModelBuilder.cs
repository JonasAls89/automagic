using System;
using System.Collections.Generic;
using Automagic.Core.DataAccess;
using Automagic.Core.MetaModel;

namespace Automagic.Core
{
    public class ModelBuilder
    {
        // private string _connectionString;
        private Db _database;

        private static object DbValue(object obj)
        {
            if (obj == null)
            {
                return DBNull.Value;
            }
            return obj;
        }


        public ModelBuilder(Db db)
        {
            _database = db;
        }

        public void PopulateModel(Model m)
        {
            var schemas = GetSchemas();
            Console.WriteLine("\tFound Schemas (" + schemas.Count + ")");
            foreach (var s in schemas)
            {
                Console.WriteLine("\t\t" + s);
            }

            var tables = GetTables(schemas);
            Console.WriteLine("\n\n\tFound Tables (" + tables.Count + ")");

            foreach (var t in tables)
            {
                Console.WriteLine("\t\t" + t);
            }

            PopulateEntityTypes(tables, m);

        }

        private const string GetSchemasSql = @"select * from information_schema.schemata";

        protected List<string> GetSchemas()
        {
            var schemas = new List<string>();
            using (var conn = _database.GetConnection())
            {
                conn.Open();
                using (var cmd = _database.GetCommand(GetSchemasSql, conn))
                {
                    cmd.Prepare();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            schemas.Add((string)reader["schema_name"]);
                        }
                    }
                }
            }
            return schemas;
        }

        private const string GetTablesSql = @"SELECT * FROM information_schema.tables WHERE table_schema = @schemaName and table_type = 'BASE TABLE'";

        protected List<string> GetTables(List<string> schemas)
        {
            var tables = new List<string>();
            foreach (var s in schemas)
            {
                if (s.StartsWith("pg_", StringComparison.InvariantCultureIgnoreCase) || s == "information_schema") continue;

                using (var conn = _database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = _database.GetCommand(GetTablesSql, conn))
                    {
                        var p = _database.MakeStringParameter("@schemaName", s);

                        /* new NpgsqlParameter
                        {
                            ParameterName = "@schemaName",
                            Value = s,
                            NpgsqlDbType = NpgsqlDbType.Text
                        }; */

                        cmd.Parameters.Add(p);
                        cmd.Prepare();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tables.Add(s + "." + (string)reader["table_name"]);
                            }
                        }
                    }
                }
            }
            return tables;
        }

        private const string GetColumnsSql = @"SELECT * FROM information_schema.columns WHERE table_schema = @schemaName and table_name = @tableName ORDER BY ordinal_position ";

        protected void PopulateEntityTypes(List<string> tables, Model m)
        {
            foreach (var t in tables)
            {
                var tmp = t.Split('.');
                var schemaName = tmp[0];
                var tableName = tmp[1];

                var entityType = new EntityType(m);
                entityType.Name = t;
                m.EntityTypes.Add(entityType);

                using (var conn = _database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = _database.GetCommand(GetColumnsSql, conn))
                    {
                        var p = _database.MakeStringParameter("@schemaName", schemaName);

                        /*new NpgsqlParameter
                    {
                        ParameterName = "@schemaName",
                        Value = schemaName,
                        NpgsqlDbType = NpgsqlDbType.Text
                    }; */
                        cmd.Parameters.Add(p);

                        p = _database.MakeStringParameter("@tableName", tableName);

                        /* new NpgsqlParameter
                    {
                        ParameterName = "@tableName",
                        Value = tableName,
                        NpgsqlDbType = NpgsqlDbType.Text
                    }; */
                        cmd.Parameters.Add(p);

                        cmd.Prepare();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var prop = new PropertyType();
                                prop.Name = (string)reader["column_name"];
                                prop.DataType = (string)reader["data_type"];
                                entityType.PropertyTypes.Add(prop);
                            }
                        }
                    }
                }

            }
        }

        protected void PopulateRelationship(List<string> tables, Model m)
        {

        }
    }

}
