using System.Data;

namespace Automagic.Core.DataAccess
{
    public abstract class Db
    {
        public abstract IDbConnection GetConnection();
        public abstract IDbCommand GetCommand(string sql, IDbConnection conn);
        public abstract IDbDataParameter MakeStringParameter(string name, object val);
        public abstract string GetQueryForTableSample(string tableName, int sampleSize);
    }
}


