using System;
using System.Collections.Generic;
using System.IO;

namespace Automagic.Core
{
    public class ReferenceData
    {
        private Dictionary<string, string> _firstNames;
        private Dictionary<string, string> _lastNames;

        public ReferenceData(string firstNamesCsv, string lastNamesCsv)
        {
            _firstNames = new Dictionary<string, string>();
            _lastNames = new Dictionary<string, string>();
            LoadData(firstNamesCsv, lastNamesCsv);
        }

        public bool IsName(string candidate)
        {
            if (_firstNames.ContainsKey(candidate)) return true;
            if (_lastNames.ContainsKey(candidate)) return true;
            return false;
        }

        private void LoadData(string firstNamesCsv, string lastNamesCsv)
        {
            using (StreamReader reader = new StreamReader(firstNamesCsv))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // System.Console.WriteLine(line);
                    if (!_firstNames.ContainsKey(line.Trim().ToLower()))
                        _firstNames.Add(line.Trim().ToLower(), null);
                }
            }

            using (StreamReader reader = new StreamReader(lastNamesCsv))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!_lastNames.ContainsKey(line.Trim().ToLower()))
                        _lastNames.Add(line.Trim().ToLower(), null);
                }
            }
        }
    }
}
