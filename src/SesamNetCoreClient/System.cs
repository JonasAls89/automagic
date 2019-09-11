using System;
using System.Collections.Generic;
using System.Text;

namespace SesamNetCoreClient
{
    /// <summary>
    /// Supported Sesam system types
    /// </summary>
    public enum SystemType
    {
        ORACLE,
        ORACLE_TNS,
        MSSQL,
        MYSQL,
        POSTGRESQL
    }

    /// <summary>
    /// Represents a secret - global or specific to this particular system
    /// </summary>
    public struct Secret
    {
        public string value { get; }
        public bool isGlobal { get; }

        public Secret(string value, bool isGlobal = false)
        {
            this.value = value;
            this.isGlobal = isGlobal;
        }
    }
    /// <summary>
    /// Represent a Sesam system object
    /// </summary>
    public sealed class SesamSystem
    {
        public Dictionary<string, object> attrs { get; }
        public Dictionary<string, Secret> secrets { get; }
        public Dictionary<string, string> envs { get; }
        public SystemType systemType { get; internal set; }

        public string id { get; internal set; }

        public SesamSystem(string id)
        {
            this.id = id;
            this.attrs = new Dictionary<string, object>(8);
            this.attrs.Add("_id", id);

            this.secrets = new Dictionary<string, Secret>();
            this.envs = new Dictionary<string, string>();
        }

        /// <summary>
        /// Assign type of system to be created
        /// </summary>
        /// <param name="s">SesamSystem type</param>
        /// <returns></returns>
        public SesamSystem OfType(SystemType s)
        {
            this.systemType = s;
            this.attrs.Add("type", String.Format("system:{0}", s.ToString().ToLower()));
            return this;
        }

        /// <summary>
        /// Adds an attribute with plain text value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SesamSystem With(string key, object value)
        {
            this.attrs.Add(key, value);
            return this;
        }

        /// <summary>
        /// Adds reference to existing secret value
        /// </summary>
        /// <param name="key">key for this attribute eg 'password'</param>
        /// <param name="value">value of this attribute eg 'secret_system_password' 
        /// which will be stored as $_SECRET(secret_system_password)</param>
        /// <returns></returns>
        public SesamSystem WithExistingSecret(string key, string value)
        {
            this.attrs.Add(key, String.Format("$SECRET({0})", value));
            return this;
        }

        /// <summary>
        /// Adds reference to existing environment variable
        /// </summary>
        /// <param name="key">key for this attribute eg db_host</param>
        /// <param name="value">value for this attribute eg 'host_for_our_db'
        /// which will be storeed as $_ENV(host_for_our_db)</param>
        /// <returns></returns>
        public SesamSystem WithExistingEnv(string key, string value)
        {
            this.attrs.Add(key, String.Format("$ENV({0})", value));
            return this;
        }

        /// <summary>
        /// Adds a new secret to Sesam vault and adds reference to it into system config
        /// </summary>
        /// <param name="key">key for this secret eg 'password'</param>
        /// <param name="value">value for this secret eg 'password_from_db'</param>
        /// <param name="secretValue">self password eg Password123</param>
        /// <param name="isGlobal">true if should be stored in global vault false if should
        /// be stored in system specific vault</param>
        /// <returns></returns>
        public SesamSystem WithSecret(string key, string value, string secretValue, bool isGlobal = false)
        {
            this.secrets.Add(value, new Secret(secretValue, isGlobal));
            this.attrs.Add(key, String.Format("$SECRET({0})", value));
            return this;
        }

        /// <summary>
        /// Adds new variable in Sesam env vars and adds reference to in into system config
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="envValue"></param>
        /// <returns></returns>
        public SesamSystem WithEnv(string key, string value, string envValue)
        {
            this.envs.Add(value, envValue);
            this.attrs.Add(key, String.Format("$ENV({0})", value));
            return this;
        }


    }
}
