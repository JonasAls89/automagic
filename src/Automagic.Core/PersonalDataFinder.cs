using System;
using System.Collections.Generic;
using Automagic.Core.MetaModel;
using Automagic.Core.DataAccess;
using System.Text.RegularExpressions;
using System.Linq;

namespace Automagic.Core
{
    public class WeightedValue
    {
        public string Key { get; set; }
        public float Weight { get; set; }
    }

    public class EntityScoreCard
    {
        public int TotalScore;

        public double NamePercentage;
        public int NameScore;

        public double EmailPercentage;
        public int EmailScore;

        public int PhoneNumberScore;

        public int SchemaScore;

        public EntityType EntityType;
    }

    public class Matchers
    {
        public static string NameMatch = @"^([a-zA-Z]{2,}\s[a-zA-z]{1,}'?-?[a-zA-Z]{2,}\s?([a-zA-Z]{1,})?)";
        public static string EmailMatch = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
        
        // As is the below regex string will not exclusively match on phone number, but will i.e. also match on Social Security number.
        // public static string PhoneNumberMatch = @"(\(?\+?[0-9]*\)?)?[0-9_\- \(\)].{5}.*$";
        // if dates span a range from 1920 - 2020 then probably dob
    }

    public class PersonalDataFinder
    {
        private List<string> indicatorTypeNames = new List<string>() { "person", "employee", "customer" };
        private List<string> indicatorColumnNames = new List<string>() { "firstname", "lastname", "mobile", "dob", "birth", "address", "national", "cust", "gender", "hired", "employed", "email" };
        private ReferenceDataBlobs _refData;

        public PersonalDataFinder(ReferenceDataBlobs refData)
        {
            _refData = refData;
        }

        /* public IEnumerable<EntityType> GetPersonalDataRoots(Model m) {
            HashSet<EntityType> piiEntities = new HashSet<EntityType>();

            foreach (var e in m.EntityTypes) {
                bool match = false;
                var tname = e.Name.Split(".")[1].ToLower();

                foreach (var tnameIndicator in indicatorTypeNames) {
                    if (tname.Contains(tnameIndicator))
                    {
                        Console.WriteLine(tnameIndicator + " " + e.Name);
                        piiEntities.Add(e);
                    }
                }
                if (match) continue;

                foreach (var pt in e.PropertyTypes) {
                    foreach (var colIndicator in indicatorColumnNames) {
                        if (pt.Name.ToLower().Contains(colIndicator)) {
                            Console.WriteLine(colIndicator + " " + e.Name + " " + pt.Name);
                            piiEntities.Add(e);
                        }
                    }
                }
            }

            return piiEntities;
        } */

        public IEnumerable<EntityScoreCard> GetPersonalDataRoots(Model m, Db db, List<string> includes)
        {

            List<EntityScoreCard> scoreCards = new List<EntityScoreCard>();

            Regex rgx = new Regex(Matchers.NameMatch);
            Regex emailRgx = new Regex(Matchers.EmailMatch);

            foreach (var e in m.EntityTypes)
            {

                var scoreCard = new EntityScoreCard();
                scoreCard.EntityType = e;
                scoreCards.Add(scoreCard);

                if (includes.Count > 0 && !includes.Contains(e.Name))
                {
                    continue;
                }

                int rowCount = 0;
                // int phoneMatch = 0;
                int emailMatch = 0;
                int nameMatch = 0;

                var valOccurrenceCount = new Dictionary<int, Dictionary<string, int>>(); // calculation of the number of unique values and their occurrences in a given column
                var countColumnNameMatch = new Dictionary<int, int>(); // how many names matches in the specified column 

                var tname = e.Name.Split('.')[1].ToLower();

                // select all data from that table and iterate in a reader. 
                string sql = db.GetQueryForTableSample(e.Name, 5000);

                var distinctValuesOfNameMatches = new Dictionary<string, string>();

                using (var conn = db.GetConnection())
                {
                    conn.Open();

                    // Retrieve all rows
                    using (var cmd = db.GetCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rowCount++;

                            // for each row check all string values for name, email, phone number
                            int colCount = reader.FieldCount;
                            for (int i = 0; i < colCount; i++)
                            {
                                var colval = reader[i];
                                if (colval is string)
                                {
                                    var cv = colval.ToString();
                                    if (cv.Trim().Length < 25 && cv.Trim().Length > 3)
                                    {
                                        var tokens = cv.Split(' ');
                                        if (tokens.Length == 1)
                                        {
                                            // might be a first name or last name
                                            var t = tokens[0].Trim().ToLower();
                                            if (_refData.IsName(t))
                                            {
                                                nameMatch++;
                                                if (!distinctValuesOfNameMatches.ContainsKey(t))
                                                {
                                                    distinctValuesOfNameMatches.Add(t, t);
                                                }
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            int localMatch = 0;
                                            foreach (var t in tokens)
                                            {
                                                var tv = t.Trim().ToLower();
                                                if (tv.Length > 5 && _refData.IsName(tv))
                                                {
                                                    localMatch++;
                                                    if (!distinctValuesOfNameMatches.ContainsKey(t))
                                                    {
                                                        distinctValuesOfNameMatches.Add(t, t);
                                                    }
                                                }
                                            }

                                            if (localMatch >= 2)
                                            {
                                                nameMatch++;
                                            }
                                        }
                                    }

                                    if (emailRgx.IsMatch(colval.ToString()))
                                    {
                                        emailMatch++;
                                    }
                                }
                            }
                        }
                    }
                }

                double percentage = (double)nameMatch / (double)rowCount;
                double uniqueNamesPercentage = (double)distinctValuesOfNameMatches.Count / (double)nameMatch;

                double emailPercentage = (double)emailMatch / (double)rowCount;

                scoreCard.NamePercentage = percentage;
                scoreCard.EmailPercentage = emailPercentage;

                // iterate the columns and then look at % in that column.
                if (rowCount > 0 && ((((double)nameMatch / (double)rowCount) > 0.60)))
                {
                    scoreCard.NameScore = 1;
                    scoreCard.TotalScore += 1;
                }

                if (emailPercentage > 0)
                {
                    scoreCard.EmailScore = 1;
                    scoreCard.TotalScore += 1;
                }

                // do schema stuff 
                foreach (var tnameIndicator in indicatorTypeNames)
                {
                    if (tname.Contains(tnameIndicator))
                    {
                        scoreCard.SchemaScore = 1;
                        scoreCard.TotalScore += 1;
                    }
                }

                foreach (var pt in e.PropertyTypes)
                {
                    foreach (var colIndicator in indicatorColumnNames)
                    {
                        if (pt.Name.ToLower().Contains(colIndicator))
                        {
                            scoreCard.SchemaScore += 1;
                            scoreCard.TotalScore += 1;
                        }
                    }
                }

                //global::System.Console.WriteLine("entity " + e.Name + "\n\t rowcount : " + rowCount.ToString() + "\n\t nameMatches: "
                //                         + nameMatch + "\n\t % : " + percentage + "\n\t unique name match values : "
                //                         + distinctValuesOfNameMatches.Count.ToString() + "\n\t unique name match % : "
                //                         + uniqueNamesPercentage.ToString() + " emailmatch: " + emailPercentage.ToString() + "\n");
            }

            return scoreCards.OrderBy(x => x.TotalScore).Reverse();
        }
    }

}
