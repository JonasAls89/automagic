using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SesamNetCoreClient
{
    public class Client
    {
        private readonly HttpClient httpClient = new HttpClient();
        private static readonly string API_URL = "https://datahub-{0}.sesam.cloud/api/";

        public Client(string jwt, string subscriptionId)
        {
            var apiUrl = BuildApiUrlFromSubscriptionId(subscriptionId);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            httpClient.BaseAddress = new Uri(apiUrl);
        }

        private String BuildApiUrlFromSubscriptionId(string subId)
        {
            if (!subId.Contains('-'))
            {
                throw new ArgumentException("Bad subscription id");
            }
            return string.Format(API_URL, subId.Split('-')[0]);
        }

        /// <summary>
        /// Check if node is healthy
        /// </summary>
        /// <returns></returns>
        public bool Ping()
        {
            HttpResponseMessage response = httpClient.GetAsync("health").Result;
            response.EnsureSuccessStatusCode();
            return true;
        }

        public string CreateSystem(SesamSystem s)
        {

            var payload = JsonConvert.SerializeObject(new[] { s.attrs });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage resSystem = httpClient.PostAsync("systems?force=true", content).Result;
            resSystem.EnsureSuccessStatusCode();
            var responseText = resSystem.Content.ReadAsStringAsync().Result;

            //store secrets

            var globals = from secret in s.secrets where secret.Value.isGlobal == true select secret;
            if (globals.Count() > 0)
            {
                var result = globals.ToDictionary(x => x.Key, x => x.Value.value);
                var globalsPayload = JsonConvert.SerializeObject(result);
                var globalsContent = new StringContent(globalsPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage resGlobals = httpClient.PostAsync("secrets", globalsContent).Result;
                resGlobals.EnsureSuccessStatusCode();
            }

            var locals = from secret in s.secrets where secret.Value.isGlobal == false select secret;
            if (locals.Count() > 0)
            {
                var result = locals.ToDictionary(x => x.Key, x => x.Value.value);
                var localsPayload = JsonConvert.SerializeObject(result);
                var localsContent = new StringContent(localsPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage resLocals = httpClient.PostAsync(String.Format("systems/{0}/secrets", s.id), localsContent).Result;
                resLocals.EnsureSuccessStatusCode();
            }

            //store env vars
            if (s.envs.Count > 0)
            {
                var envsPayload = JsonConvert.SerializeObject(s.envs);
                var envsContent = new StringContent(envsPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage resEnvs = httpClient.PostAsync(String.Format("env", s.id), envsContent).Result;
                resEnvs.EnsureSuccessStatusCode();
            }

            return responseText;
        }

        public string CreatePipe(Pipe p)
        {
            var payload = JsonConvert.SerializeObject(new[] { p.attrs });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage resSystem = httpClient.PostAsync("pipes?force=true", content).Result;
            resSystem.EnsureSuccessStatusCode();
            var responseText = resSystem.Content.ReadAsStringAsync().Result;
            return responseText;
        }

        public void DeleteSystem(string systemId)
        {
            HttpResponseMessage res = httpClient.DeleteAsync(String.Format("systems/{0}", systemId)).Result;
            res.EnsureSuccessStatusCode();
        }

        public void DeletePipe(string pipeId)
        {
            HttpResponseMessage res = httpClient.DeleteAsync(String.Format("pipes/{0}", pipeId)).Result;
            res.EnsureSuccessStatusCode();
        }


    }
}
