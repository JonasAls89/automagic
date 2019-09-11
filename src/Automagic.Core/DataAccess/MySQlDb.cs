using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Automagic.Core.DataAccess
{
    public class MySQLDb : Db
    {
        private string _connstr;
        public MySQLDb(string connstr)
        {
            _connstr = connstr;
        }

        public override IDbConnection GetConnection()
        {
            return new MySqlConnection(_connstr);
        }

        public override IDbCommand GetCommand(string sql, IDbConnection conn)
        {
            return new MySqlCommand(sql, (MySqlConnection)conn);
        }

        public override IDbDataParameter MakeStringParameter(string name, object val)
        {
            MySqlParameter param = new MySqlParameter(name, MySqlDbType.VarChar, 512);
            param.Value = val;
            return param;
        }

        public override string GetQueryForTableSample(string tableName, int sampleSize)
        {
            return String.Format("SELECT * FROM {0} LIMIT {1}", tableName, sampleSize);  
        }
    }        
}