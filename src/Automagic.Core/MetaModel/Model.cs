using System;
using System.Collections.Generic;

namespace Automagic.Core.MetaModel
{
    /// <summary>
    /// Model. Contains a collection of entity types that defines kinds of data.
    /// </summary>
    public class Model
    {
        /// <summary>
        /// The source system where the model is derived from.
        /// </summary>
        /// <value>The system.</value>
        public System System { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Automagic.Core.MetaModel.Model"/> class.
        /// </summary>
        /// <param name="system">System.</param>
        public Model(System system)
        {
            EntityTypes = new List<EntityType>();
            System = system;
        }

        /// <summary>
        /// Gets or sets the entity types.
        /// </summary>
        /// <value>The entity types.</value>
        public List<EntityType> EntityTypes { get; set; }
    }
}

