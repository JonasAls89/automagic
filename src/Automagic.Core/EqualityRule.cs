using System;
using Automagic.Core.MetaModel;

namespace Automagic.Core
{
    /// <summary>
    /// This defines an equality rule
    /// </summary>
    public class EqualityRule
    {
        public EntityType SourceEntityType { get; set; }
        public EntityType TargetEntityType { get; set; }
        public PropertyType SourcePropertyType { get; set; }
        public PropertyType TargetPropertyType { get; set; }

        // might need some match indicator
    }
}
