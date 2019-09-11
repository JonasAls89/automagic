using System;
using System.Collections.Generic;

namespace Automagic.Core.MetaModel
{
    /// <summary>
    /// Entity type.
    /// </summary>
    public class EntityType
    {
        /// <summary>
        /// The model that this type belongs to
        /// </summary>
        /// <value>The model.</value>
        public Model Model { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Automagic.Core.MetaModel.EntityType"/> class.
        /// </summary>
        public EntityType()
        {
            PropertyTypes = new List<PropertyType>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Automagic.Core.MetaModel.EntityType"/> class.
        /// </summary>
        /// <param name="model">Model.</param>
        public EntityType(Model model)
        {
            PropertyTypes = new List<PropertyType>();
            Model = model;
        }

        /// <summary>
        /// This is the globally unique name for this type
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// This is the name used in the source system. e.g. tablename, endpoint type etc
        /// </summary>
        /// <value>The name of the source.</value>
        public string SourceName { get; set; }

        /// <summary>
        /// List of the property types
        /// </summary>
        /// <value>The property types.</value>
        public List<PropertyType> PropertyTypes { get; set; }
    }
}
