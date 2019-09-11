using System;
using System.Collections.Generic;
using Automagic.Core.MetaModel;

namespace Automagic.Core
{
    /// <summary>
    /// A single project managing state of a configuration
    /// </summary>
    public class Project
    {
        public string Name { get; set; }
        public string SubscriptionId { get; set; }
        public string Jwt { get; set; }
        public List<System> Systems;
        public List<Global> Globals;
        public List<EntityType> Incoming;

        public Project()
        {
            Systems = new List<System>();
            Globals = new List<Global>();
        }

        public Object GetSesamConfiguration()
        {
            return null;
        }
    }
}



