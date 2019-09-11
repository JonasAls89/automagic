using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SesamNetCoreClient
{

    /// <summary>
    /// Interface providing methods for "source" part of a pipe
    /// 
    /// `
    /// {
    ///     "_id": "case-salesforce",
    ///     "type": "pipe",
    ///     "source": {
    ///         "type": "dataset",
    ///         "dataset": "salesforce-case"
    ///     },
    ///     "transform": {
    ///         "type": "dtl",
    ///         "rules": {
    ///             "default": [
    ///                 ["add", "Id", "_S.Id"],
    ///                 ["add", "ContactId", null]
    ///             ]
    ///         }
    ///     }
    ///}
    /// `
    /// </summary>
    public interface ISource
    {
        /// <summary>
        /// Method to set a source type
        /// Check Sesam.io documentation for list of availbale sources
        /// </summary>
        /// <param name="type"></param>
        void SetType(string type);
        /// <summary>
        /// Method that returns all "source" attributes
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetAttributes();
        /// <summary>
        /// Method that check if provided source is correctly formed according to its type
        /// This method should throw a ValidationException if source configuration is not valid
        /// </summary>
        void Validate();
    }
    /// <summary>
    /// SQL source 
    /// </summary>

    public interface DTL
    {
        /// <summary>
        /// Method to set a source type
        /// Check Sesam.io documentation for list of availbale sources
        /// </summary>
        /// <param name="type"></param>
        void SetType(string type);
        /// <summary>
        /// Method that returns all "transform" attributes
        /// </summary>
        /// <returns></returns>
        //Dictionary<string, string> GetTransformAttributes();
        JObject GetTransformAttributes();
        /// <summary>
        /// Method that check if provided source is correctly formed according to its type
        /// This method should throw a ValidationException if source configuration is not valid
        /// </summary>
        void Validate();
    }
    /// <summary>
    /// SQL source 
    /// </summary>


    public class SqlSource : ISource
    {
        private Dictionary<string, string> attrs;

        public SqlSource()
        {
            this.attrs = new Dictionary<string, string>();
        }
        
        public void SetType(string type)
        {
            this.attrs.Add("type", type);
        }

        public void SetKey(string type)
        {
            this.attrs.Add("primary_key", type);
        }

        public void SetTable(string tableName)
        {
            this.attrs.Add("table", tableName);
        }

        public void SetSystem(string systemName)
        {
            this.attrs.Add("system", systemName);
        }

        public Dictionary<string, string> GetAttributes()
        {
            return this.attrs;
        } 

        public void Validate() {
            if (!this.attrs.ContainsKey("system")) {
                throw new ValidationException("source doesn't contain a system");
            }

            if (!this.attrs.ContainsKey("table") && !this.attrs.ContainsKey("query")) {
                throw new ValidationException("table or qury attribute must be presented");
            }
        }
    }

    public class Transform : DTL
    {
        private JObject attrs;

        public Transform()
        {
            this.attrs = new JObject();
        }

        public void SetType(string type)
        {
            this.attrs["type"] = type;
        }

        public void AddRule()
        { 
            this.attrs["rules"] = new JObject();
            //this.attrs.Property("type").AddAfterSelf(new JProperty("rules", new JObject(new JProperty("default"))));
        }   

        public void MakeDefaultRule() {
            //this.attrs["rules"] = new JObject();
            this.attrs["rules"]["default"] = new JArray();
        }

        public void AddCopy(string rule, string property)
        {
            var copy = new JArray();
            copy.Add("copy");
            copy.Add(property);

            JArray r = this.attrs["rules"][rule] as JArray;
            r.Add(copy);
        }

        public void AddRdfType(string rule, string pipeName, string tableName)
        {
            var rdf = new JArray();
            rdf.Add("ni");
            rdf.Add(pipeName);
            rdf.Add(tableName);
            
            var add = new JArray();
            add.Add("add");
            add.Add("rdf:type");
            add.Add(rdf);

            JArray r = this.attrs["rules"][rule] as JArray;
            r.Add(add);
        }

        public void AddMakeNi(string rule, string propertyName, string prefix, string val)
        {
            // create the array for the ni function
            var ni = new JArray();
            ni.Add("ni");
            ni.Add(prefix);
            ni.Add("_S."+val);

            // create the array for the add function
            // include the ni function as the last element
            var add = new JArray();
            add.Add("add");
            add.Add(propertyName+"-ni");
            add.Add(ni);

            // add the function to the rule specified
            JArray r = this.attrs["rules"][rule] as JArray;
            r.Add(add);
        }

        public JObject GetTransformAttributes()
        {
            return this.attrs;
        }

        public void Validate() {
            if (!this.attrs.ContainsKey("system")) {
                throw new ValidationException("source doesn't contain a system");
            }

            if (!this.attrs.ContainsKey("table") && !this.attrs.ContainsKey("query")) {
                throw new ValidationException("table or qury attribute must be presented");
            }
        }
        
    }
    public sealed class Pipe
    {
        public string id { get; }
        public Dictionary<string, object> attrs { get; }

        public Pipe(string id)
        {
            this.id = id;
            this.attrs = new Dictionary<string, object>();
            this.attrs.Add("_id", this.id);
            this.attrs.Add("type", "pipe");
        }

        public Pipe WithSource(ISource s)
        {
            this.attrs.Add("source", s.GetAttributes());
            return this;
        }

        public Pipe WithTransform(DTL s)
        {
            this.attrs.Add("transform", s.GetTransformAttributes());
            return this;
        }

        public Pipe WithSink()
        {
            //Not implemented yet
            return this;
        }

        public Pipe WithPump()
        {
            //Not implemented yet
            return this;
        }
    }
}
