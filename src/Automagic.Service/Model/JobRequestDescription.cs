using SesamNetCoreClient;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Automagic.Service.Model
{
    public enum JobMode {
        //take all tables
        ALL,
        //take only tables with personal data
        PERSONAL,
        //take only tables without personal data
        NON_PERSONAL
    }
    /// <summary>
    /// Definition of request to start "automagic" job
    /// </summary>
    public class JobRequestDescription
    {  
        /// <summary>
        /// Hostname or IP for Database server we will get data from
        /// </summary>
        [Required]
        public string dbHost { get; set; }

        /// <summary>
        /// Database name
        /// </summary>
        [Required]
        public string dbName { get; set; }
        
        /// <summary>
        /// Database user
        /// </summary>
        [Required]
        public string Dbase { get; set; }

        /// <summary>
        /// Database user
        /// </summary>
        public string SesamJWT { get; set; }

        /// <summary>
        /// Database user
        /// </summary>
        public string SesamSubID { get; set; }

        /// <summary>
        /// Database user
        /// </summary>
        [Required]
        public string dbUser { get; set; }

        /// <summary>
        /// Database password
        /// </summary>
        [Required]
        public string MappingChoice { get; set; }
        /// <summary>
        /// Database password
        /// </summary>
        [Required]
        public string dbPassword { get; set; }

        /// <summary>
        /// Database port
        /// </summary>
        public string dbPort { get; set; }

        /// <summary>
        /// Source type ORACLE|ORACLE_TNS|MSSQL|MYSQL|POSTGRESQL
        /// </summary>
        [Required]
        public SystemType sourceType { get; set; }

        /// <summary>
        /// List of tables to not inspect
        /// </summary>
        public List<string> blacklistTables { get; set; }

        /// <summary>
        /// In which mode job will be performed
        /// </summary>
        public JobMode mode { get; set; }
    }
}
