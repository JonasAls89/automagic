using System;
using Automagic.Core.DataAccess;
using Automagic.Core.MetaModel;

namespace Automagic.Core
{
    /// <summary>
    /// System. A representation of some external system, database, endpoint etc.
    /// </summary>
    public class System
    {
        /// <summary>
        /// Logical and unique name
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the type of the system.
        /// </summary>
        /// <value>The type of the system.</value>
        public string SystemType { get; set; }

        // json ignore
        public Db Database { get; set; }
        public Model Model { get; set; }
    }
}
