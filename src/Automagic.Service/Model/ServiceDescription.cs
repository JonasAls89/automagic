using System;
namespace Automagic.Service.Model
{
    public class ServiceDescription
    {
        public string Version { get; set; }
        public string SwaggerEndpoint { get; set; }

        public ServiceDescription()
        {
            Version = "1.0";
            SwaggerEndpoint = "/swagger";
        }
    }
}
