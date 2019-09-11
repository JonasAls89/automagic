using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Automagic.Core.DataAccess
{
    public class PostgreSqlDb : Db
    {
        private string _connstr;

        public PostgreSqlDb(string connstr)
        {
            _connstr = connstr;
        }

        public override IDbConnection GetConnection()
        {
            return new NpgsqlConnection(_connstr);
        }

        public override IDbCommand GetCommand(string sql, IDbConnection conn)
        {
            return new NpgsqlCommand(sql, (NpgsqlConnection)conn);
        }

        public override IDbDataParameter MakeStringParameter(string name, object val)
        {
            var param = new NpgsqlParameter(name, NpgsqlDbType.Varchar, 512);
            param.Value = val;
            return param;
        }

        public override string GetQueryForTableSample(string tableName, int sampleSize)
        {
            return "SELECT * FROM " + tableName + " LIMIT " + sampleSize.ToString();
        }
    }
}
