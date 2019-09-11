using System;
namespace Automagic.Service.Model
{
    public class ServiceV2Description
    {
        public string Version { get; set; }
        public string SwaggerEndpoint { get; set; }

        public ServiceV2Description()
        {
            Version = "2.0";
            SwaggerEndpoint = "/swagger";
        }
    }
}
