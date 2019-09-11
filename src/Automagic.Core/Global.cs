using System;
using System.Collections.Generic;
using Automagic.Core.MetaModel;

namespace Automagic.Core
{
    public class Global
    {
        public string Name { get; set; }
        public List<EntityType> Contributors { get; set; }
        public List<string> EqualityRules { get; set; }
    }
}

