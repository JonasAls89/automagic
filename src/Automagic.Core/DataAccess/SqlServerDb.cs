using System;
using System.Data;
using System.Data.SqlClient;

namespace Automagic.Core.DataAccess
{
    public class SqlServerDb : Db
    {
        private string _connstr;

        public SqlServerDb(string connstr)
        {
            _connstr = connstr;
        }

        public override IDbConnection GetConnection()
        {
            return new SqlConnection(_connstr);
        }

        public override IDbCommand GetCommand(string sql, IDbConnection conn)
        {
            return new SqlCommand(sql, (SqlConnection)conn);
        }

        public override IDbDataParameter MakeStringParameter(string name, object val)
        {
            SqlParameter param = new SqlParameter(name, SqlDbType.NVarChar, 512);
            param.Value = val;
            return param;
        }

        public override string GetQueryForTableSample(string tableName, int sampleSize)
        {
            return "SELECT top " + sampleSize.ToString() + " * FROM " + tableName;
        }
    }
}
