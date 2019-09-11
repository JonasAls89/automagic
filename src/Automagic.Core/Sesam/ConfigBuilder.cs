using System;
namespace Automagic.Core.Sesam
{
    public class ConfigBuilder
    {
        private Config _config;

        public ConfigBuilder()
        {
            _config = new Config();
        }

        public ConfigBuilder AddPipe() {
            return this;
        }

        public ConfigBuilder AddSystem() {
            return this;    
        }
    }
}
