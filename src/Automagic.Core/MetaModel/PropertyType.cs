using System;

namespace Automagic.Core.MetaModel
{
    /// <summary>
    /// Property type. Forms part of an EntityType to indicate the allowed proprties.
    /// </summary>
    public class PropertyType
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        public string DataType { get; set; }
    }
}
