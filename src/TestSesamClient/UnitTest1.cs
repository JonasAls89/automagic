using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SesamNetCoreClient;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TestSesamClient
{
    [TestClass]
    public class UnitTest1
    {
        private static string jwt;
        private static string subscriptionId;

        /* 
        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            using (var file = File.OpenText("../../../Properties/launchSettings.json"))
            {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);

                var variables = jObject
                    .GetValue("profiles")
                    //select a proper profile here
                    .SelectMany(profiles => profiles.Children())
                    .SelectMany(profile => profile.Children<JProperty>())
                    .Where(prop => prop.Name == "environmentVariables")
                    .SelectMany(prop => prop.Value.Children<JProperty>())
                    .ToList();

                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                }
            }
            jwt = Environment.GetEnvironmentVariable("SESAM_JWT");
            subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
        }

        [AssemblyCleanup]
        public static void DeInit()
        {
            using (var file = File.OpenText("../../../Properties/launchSettings.json"))
            {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);

                var variables = jObject
                    .GetValue("profiles")
                    //select a proper profile here
                    .SelectMany(profiles => profiles.Children())
                    .SelectMany(profile => profile.Children<JProperty>())
                    .Where(prop => prop.Name == "environmentVariables")
                    .SelectMany(prop => prop.Value.Children<JProperty>())
                    .ToList();

                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Name, null);
                }
            }
            Client client = new Client(jwt, subscriptionId);
            client.DeleteSystem("id-for-my-new-system");
            client.DeletePipe("id-for-my-new-pipe");

        }

        [TestMethod]
        public void testInit()
        {

            Client client = new Client(jwt, subscriptionId);
            var result = client.Ping();
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        [ExpectedException(typeof(System.AggregateException))]
        public void testInitMustFail()
        {
            Client client = new Client("", "fake-id");
            var result = client.Ping();
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestSystemCreate()
        {
            Client client = new Client(jwt, subscriptionId);
            SesamSystem s = new SesamSystem("id-for-my-new-system")
                .OfType(SystemType.POSTGRESQL)
                .With("host","35.228.48.175")
                .With("port", 5432)
                .With("database", "test_db2")
                .WithEnv("username", "my-db-username", "postgres")
                .WithSecret("password", "my-db-password", "rootpassword123", true);
            string json = client.CreateSystem(s);

            Assert.IsTrue(json.Length > 100);
        } */

        [TestMethod]
        public void TestPipeCreate() {
            // Client client = new Client(jwt, subscriptionId);
            /*  Pipe p = new Pipe("id-for-my-new-pipe");
            var source = new SqlSource();

            source.SetTable("customer");
            source.SetSystem("id-for-my-new-system");
            source.SetType("sql");
            p.WithSource(source); */

            var t = new Transform();

            t.MakeDefaultRule();
            t.AddMakeNi("default", "prop", "test", "person");
            
            var json = JsonConvert.SerializeObject(t);

            Console.WriteLine("output " + json);
            // string json = client.CreatePipe(p);
        }
    }
}
