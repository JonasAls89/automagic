// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DateTime;
using System.Linq;
using Automagic.Core;
using Automagic.Core.DataAccess;
using FKeyMapping;
using FKeyMappingPostgreSQL;
using PostgreSQLIndexMapping;
using IndexRefMappingPostgreSQL;
using IndexMapping;
using GetAllTablesAndColumns;
using IndexRefMapping;
using FKeyRefQueries;
using SesamNetCoreClient;
using System.Net.Http;

namespace Automagic.Chatbot.Bots
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    public class CustomPromptBot : ActivityHandler
    {
        private readonly BotState _userState;
        private readonly BotState _conversationState;

        private const string WelcomeMessage = "Welcome to the automagic chatbot. My name is Steve, and I'm here to help you!\nType anything to get started";

        public CustomPromptBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
            _userState = userState;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> WelcomeUser, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text(WelcomeMessage), cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
            var flow = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationFlow());

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var profile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            await FillOutUserProfileAsync(flow, profile, turnContext);

            // Save changes.
            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }

        private static async Task FillOutUserProfileAsync(ConversationFlow flow, UserProfile profile, ITurnContext turnContext)
        {
            string input = turnContext.Activity.Text?.Trim();
            string message;

            switch (flow.LastQuestionAsked)
            {
                case ConversationFlow.Question.None:
                    await turnContext.SendActivityAsync("First I'd like to know your name, so what is your name?");
                    flow.LastQuestionAsked = ConversationFlow.Question.Name;
                    break;
                case ConversationFlow.Question.Name:
                    if (ValidateName(input, out string name, out message))
                    {
                        profile.Name = name;
                        await turnContext.SendActivityAsync($"Hi {profile.Name}.");
                        await turnContext.SendActivityAsync("What is the jwt token of your Sesam Client that you want me to configure?");
                        await turnContext.SendActivityAsync("When creating the jwt token under the 'Subscription' section in Sesam, remember to set priviledges to admin");
                        flow.LastQuestionAsked = ConversationFlow.Question.JwTSesam;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.JwTSesam:
                    if (ValidateName(input, out string JwTSesam, out message))
                    {
                        profile.JwTSesam = JwTSesam;
                        await turnContext.SendActivityAsync("Next, what is the subscription ID of your Sesam Client that you want me to configure?");
                        flow.LastQuestionAsked = ConversationFlow.Question.SubIDSesam;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.SubIDSesam:
                    if (ValidateName(input, out string SubIDSesam, out message))
                    {
                        profile.SubIDSesam = SubIDSesam;
                        var reply = MessageFactory.SuggestedActions(
                                        new CardAction[]
                                        {
                                            new CardAction() { Title = "MySQL", Type = ActionTypes.ImBack, Value = "MySQL" },
                                            new CardAction() { Title = "PostgreSQL", Type = ActionTypes.ImBack, Value = "PostgreSQL" }
                                        }, text: $"What database type do you want to connect to?");
                        await turnContext.SendActivityAsync(reply);
                        flow.LastQuestionAsked = ConversationFlow.Question.Dbase;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.Dbase:
                    if (ValidateDBase(input, out string database, out message))
                    {
                        profile.Dbase = database;
                        await turnContext.SendActivityAsync($"I have your database as {profile.Dbase}");
                        if (profile.Dbase.ToLower().Contains("postgresql"))
                        {
                            await turnContext.SendActivityAsync($"What port exposes your database, i.e. 5000?");
                            flow.LastQuestionAsked = ConversationFlow.Question.dbPort;
                            break;
                        }
                        else
                        {
                            profile.dbPort = "3306"; // Helper to make the program run when this value is not needed.
                            await turnContext.SendActivityAsync("What IP address/Host server do you want me to use to connect to your database? Obs. your own IP won't work.. ;)");
                            flow.LastQuestionAsked = ConversationFlow.Question.IP;
                            break;
                        }
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.dbPort:
                    if (ValidateDbPort(input, out string dbPort, out message))
                    {
                        profile.dbPort = dbPort;
                        await turnContext.SendActivityAsync($"I have your port as {profile.dbPort}");
                        await turnContext.SendActivityAsync("What IP address/Host server do you want me to use to connect to your database? Obs. your own IP won't work.. ;)");
                        flow.LastQuestionAsked = ConversationFlow.Question.IP;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.IP:
                    if (ValidateIP(input, out string ip_adress, out message))
                    {
                        profile.IP = ip_adress;
                        await turnContext.SendActivityAsync($"I have your IP address/Host server as {profile.IP}");
                        await turnContext.SendActivityAsync("What is the name of your database? i.e. mysql_database");
                        flow.LastQuestionAsked = ConversationFlow.Question.dbName;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.dbName:
                    if (ValidateDbName(input, out string dbName, out message))
                    {
                        profile.dbName = dbName;
                        await turnContext.SendActivityAsync($"I have your database name as {profile.dbName}.");
                        await turnContext.SendActivityAsync($"Then I need to know your username for connecting to the database");
                        flow.LastQuestionAsked = ConversationFlow.Question.dbUser;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.dbUser:
                    if (ValidateDbUser(input, out string dbUser, out message))
                    {
                        profile.dbUser = dbUser;
                        await turnContext.SendActivityAsync($"I have your username as {profile.dbUser}.");
                        await turnContext.SendActivityAsync($"Albeit a username without a password won't do, so what password do you use when connecting to the database?");
                        flow.LastQuestionAsked = ConversationFlow.Question.dbPassword;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.dbPassword:
                    if (ValidateDbPassword(input, out string dbPassword, out message))
                    {
                        profile.dbPassword = dbPassword;
                        await turnContext.SendActivityAsync($"I have your password as {profile.dbPassword}.");
                        var reply = MessageFactory.SuggestedActions(
                                        new CardAction[]
                                        {
                                            new CardAction() { Title = "Index Mapping", Type = ActionTypes.ImBack, Value = "Index Mapping" },
                                            new CardAction() { Title = "Foreign Key Mapping", Type = ActionTypes.ImBack, Value = "Foreign Key Mapping" }
                                        }, text: $"Finally, would you like me to scan your database for personal identifiable indicators based on index mapping or foreign key mapping?");
                        await turnContext.SendActivityAsync(reply);
                        flow.LastQuestionAsked = ConversationFlow.Question.configChoice;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that. Type one of the two options provided.");
                        break;
                    }
                case ConversationFlow.Question.configChoice:
                    if (ValidateConfigChoice(input, out string configChoice, out message))
                    {
                        profile.configChoice = configChoice;
                        if (profile.configChoice.ToString().Contains("Index Mapping"))
                        {
                            await turnContext.SendActivityAsync($"I will now run a scan of your database based on index mapping...");
                            await turnContext.SendActivityAsync($"Give me a minute or so.");
                            try
                            {
                                // Setting variables
                                string User = profile.dbUser.ToString();
                                string Dbase = profile.Dbase.ToString();
                                string Namedb = profile.dbName.ToString();
                                string Host = profile.IP.ToString();
                                string Port = profile.dbPort.ToString();
                                string Password = profile.dbPassword.ToString();
                                string SesamJwt = profile.JwTSesam.ToString();
                                string SesamSubscriptionId = profile.SubIDSesam.ToString();
                                
                                // Making connection to different databases dynamic - add databases here when needed
                                string connSql= "";
                                Db db = null;   
                                if (Dbase.ToLower().Contains("mysql")) 
                                {
                                    connSql = string.Format("server={0};uid={1};pwd={2};database={3}", Host, User, Password, Namedb);
                                    db = new MySQLDb(connSql);
                                }
                                else if (Dbase.ToLower().Contains("postgresql"))
                                {
                                    connSql = string.Format("User ID={0};Password={1};Host={2};Port={3};Pooling=true;Database={4}", User, Password, Host, Port, Namedb);
                                    db = new PostgreSqlDb(connSql);
                                }
                                
                                var se = new ModelBuilder(db);
                                var m = new Core.MetaModel.Model(new Core.System() {Name = "test"});
                                
                                WebClient myWebClient = new WebClient();
                                Stream firstnames = myWebClient.OpenRead("https://csbf8630753f67dx4346x85e.blob.core.windows.net/automagic-test/firstnames.csv");
                                Stream alllastnames = myWebClient.OpenRead("https://csbf8630753f67dx4346x85e.blob.core.windows.net/automagic-test/alllastnames.csv");

                                var refData = new ReferenceDataBlobs(firstnames, alllastnames);
                                firstnames.Close();
                                alllastnames.Close();

                                se.PopulateModel(m);

                                var pdef = new PersonalDataFinder(refData);
                                var candidates = pdef.GetPersonalDataRoots(m, db, new List<string>(Array.Empty<string>()));
                                
                                // Logic here for finding and naming Pii tables for index mapping ".last()"
                                StringBuilder piiIndxTables = new StringBuilder();
                                foreach (var et in candidates)
                                {   
                                    if (et.TotalScore >= 1)
                                    {
                                        piiIndxTables.AppendLine(et.EntityType.Name.ToString().TrimEnd().Split('.').Last());
                                    }
                                }

                                //Setting Index table and column variables from table schema
                                StringBuilder indexColumns = new StringBuilder();
                                StringBuilder indexTables = new StringBuilder();

                                //Setting all columns and tables variables from column schema 
                                StringBuilder allColumns = new StringBuilder();
                                StringBuilder allTables = new StringBuilder();

                                //Setting index ref tables and columns
                                StringBuilder tablesWithIndxRefs = new StringBuilder();
                                StringBuilder pairingIndxTables = new StringBuilder();
                                StringBuilder pairingIndxColumns = new StringBuilder();
                                StringBuilder columnsWithIndxRefs = new StringBuilder();

                                //Setting concatenatedlist for index reference mapping
                                List<string> concatPairedlist = new List<string>();

                                if (Dbase.ToLower().Contains("mysql"))
                                {
                                    IndexQuery index = new IndexQuery(indexColumns, indexTables, db, Namedb); 
                                    //---------------------------------
                                    
                                    GetAllQuery queryAll = new GetAllQuery(allColumns, allTables, db, Namedb); 
                                    //---------------------------------
                                    
                                    IndexRefQuery indexRef = new IndexRefQuery(allTables, piiIndxTables, indexColumns, allColumns, tablesWithIndxRefs, pairingIndxTables, pairingIndxColumns, columnsWithIndxRefs, db, Namedb);
                                    //---------------------------------
                                
                                    // Logic here for correctly creating NI in a concatenated list for index mapping
                                    var tablesWithIndxRefsArray = tablesWithIndxRefs.ToString().TrimEnd().Split("\n");
                                    var columnsWithIndxRefsArray = columnsWithIndxRefs.ToString().TrimEnd().Split("\n");
                                    var pairingIndxTablesArray = pairingIndxTables.ToString().TrimEnd().Split("\n");
                                    var pairingIndxColumnsArray = pairingIndxColumns.ToString().TrimEnd().Split("\n");
                                    int dk = 0;
                                    foreach(string e in pairingIndxTablesArray)
                                    {
                                        concatPairedlist.Add(tablesWithIndxRefsArray[dk] + ";" + e + ";" + pairingIndxColumnsArray[dk] + ";" + columnsWithIndxRefsArray[dk]);
                                        dk++;   
                                    }
                                }

                                else if (Dbase.ToLower().Contains("postgresql"))
                                {
                                    IndexQueryPostgreSQL index = new IndexQueryPostgreSQL(indexTables, indexColumns, db);
                                    tablesWithIndxRefs = indexTables;
                                    //---------------------------------

                                    GetAllQueryPostGreSQL queryAll = new GetAllQueryPostGreSQL(allColumns, allTables, db, Namedb);
                                    string id = "id"; // Making sure id is included
                                    if (allColumns.ToString().Contains("id") == false)
                                    {   
                                        allColumns.AppendLine(id);
                                    }
                                    //---------------------------------

                                    //Getting matched values
                                    StringBuilder concatenatedList = new StringBuilder();
                                    IndexRefQueryPostgreSQL indexRef = new IndexRefQueryPostgreSQL(allTables, indexTables, allColumns, indexColumns, concatenatedList, db, Namedb);
                                    //---------------------------------

                                    // Logic here for correcly creating NI in a concatenated list for foreign key mapping
                                    var concatListArray = concatenatedList.ToString().TrimEnd().Split("\n");         
                                    int no = 0;
                                    foreach(var e in concatListArray)
                                    {
                                        concatPairedlist.Add(e);
                                        no++;
                                    }
                                }
                                
                                // Logic here for appending to Pii tables if Tables with index refs do not exist
                                StringBuilder tablesToSesam = new StringBuilder();
                                foreach (string i in tablesWithIndxRefs.ToString().TrimEnd().Split("\n"))
                                {
                                    if (tablesToSesam.ToString().Contains(i) == false)
                                    {
                                        tablesToSesam.AppendLine(i);
                                    }
                                }
                                foreach (string f in piiIndxTables.ToString().TrimEnd().Split("\n"))
                                {
                                    if (tablesToSesam.ToString().Contains(f) == false)
                                    {
                                        tablesToSesam.AppendLine(f);
                                    }
                                }

                                // Returning results in the chatbot
                                await turnContext.SendActivityAsync($"The following tables were found containing personal identifiable indicators.\n " + tablesToSesam);
                                //await turnContext.SendActivityAsync($"Which of the tables would you like me to create a pipe from?");
                                //StringBuilder chosenTables = new StringBuilder();
                                //int counts = 1;
                                
                                //foreach (string table in piiIndxTables.ToString().TrimEnd().Split("\n"))
                                //{
                                    //for(int e = 0 ; e < counts ; e++) 
                                    //{                 
                                    //    var reply = MessageFactory.SuggestedActions(
                                    //    new CardAction[]
                                    //    {
                                    //        new CardAction() { Title = "Yes", Type = ActionTypes.ImBack, Value = "Yes" },
                                    //        new CardAction() { Title = "No", Type = ActionTypes.ImBack, Value = "No" }
                                    //    }, text: $"Should table : '{table}' be included?");
                                    //    Console.WriteLine("Now writing the reply " + reply);
                                    //    await turnContext.SendActivityAsync(reply);
                                    //}
                                //}
                                await turnContext.SendActivityAsync($"I will now make pipes of the found tables. The tables will be named with the prefix 'automagic'.");
                                await turnContext.SendActivityAsync($"Operation in progress...");

                                //create a system
                                var client = new SesamNetCoreClient.Client(SesamJwt, SesamSubscriptionId);
                                SesamSystem system = null;
                                if (Dbase.ToLower().Contains("mysql"))
                                {
                                    system = new SesamSystem(Namedb)
                                        .OfType(SystemType.MYSQL)
                                        .With("host", Host)
                                        .With("database", Namedb)
                                        .With("username", User)
                                        .With("password", Password);
                                }
                                else if (Dbase.ToLower().Contains("postgresql"))
                                {
                                    system = new SesamSystem(Namedb)
                                        .OfType(SystemType.POSTGRESQL)
                                        .With("host", Host)
                                        .With("database", Namedb)
                                        .With("username", User)
                                        .With("password", Password);
                                }

                                try
                                { 
                                    client.CreateSystem(system);
                                }
                                catch (HttpRequestException ex)
                                {
                                    Console.WriteLine("Skipping the creation of system, because it already exists. Skipping with error: \n{0}", ex);
                                }

                                // create pipes
                                foreach (string et in tablesToSesam.ToString().TrimEnd().Split("\n"))
                                {   
                                    if (et != "")
                                    {
                                        var namePrefix = et.TrimEnd()
                                            .Replace(".", "-")
                                            .Replace("_", "-");
                                        
                                        var pipe = new Pipe("");
                                        if (Dbase.ToLower().Contains("mysql"))
                                        {
                                            pipe = new Pipe(string
                                            .Format("automagic-mysql-{0}", et.TrimEnd()
                                            .Replace(".", "-")
                                            .Replace("_", "-")));
                                        }
                                        else if (Dbase.ToLower().Contains("postgresql"))
                                        {
                                            pipe = new Pipe(string
                                            .Format("automagic-postgresql-{0}", et.TrimEnd()
                                            .Replace(".", "-")
                                            .Replace("_", "-")));
                                        }

                                        var source = new SqlSource();
                                        source.SetTable(et.TrimEnd());
                                        source.SetSystem(Namedb);
                                        source.SetType("sql");
                                        source.SetKey("id");

                                        var transform = new Transform();
                                        transform.SetType("dtl");
                                        transform.AddRule();
                                        transform.MakeDefaultRule();
                                        transform.AddCopy("default", "*");
                                        
                                        if (Dbase.ToLower().Contains("mysql"))
                                        {
                                            transform.AddRdfType("default", "automagic-mysql-"+et.TrimEnd().Replace(".", "-").Replace("_", "-"), et.TrimEnd());
                                        
                                            foreach (string f in concatPairedlist)
                                            {        
                                                if (f != "")
                                                {
                                                    if (f.TrimEnd().Split(";")[0] == et.TrimEnd())
                                                    {        
                                                        transform.AddMakeNi("default", f.TrimEnd().Split(";")[1]+"-"+f.TrimEnd().Split(";")[3], "automagic-mysql-"+f.TrimEnd().Split(";")[1], f.TrimEnd().Split(";")[2]);
                                                    }
                                                }
                                            }
                                        }
                                        
                                        else if (Dbase.ToLower().Contains("postgresql"))
                                        {
                                            transform.AddRdfType("default", "automagic-postgresql-"+et.TrimEnd().Replace(".", "-").Replace("_", "-"), et.TrimEnd());
                                        
                                            foreach (string f in concatPairedlist)
                                            {        
                                                if (f != "")
                                                {
                                                    if (f.TrimEnd().Split(";")[0] == et.TrimEnd())
                                                    {        
                                                        transform.AddMakeNi("default", f.TrimEnd().Split(";")[1]+"-"+f.TrimEnd().Split(";")[2], "automagic-postgresql-"+f.TrimEnd().Split(";")[1], f.TrimEnd().Split(";")[3]);
                                                    }
                                                }
                                            }
                                        }

                                        pipe.WithSource(source).WithTransform(transform);    
                                        
                                        try
                                        {
                                            if (Dbase.ToLower().Contains("mysql"))
                                            {
                                                client.DeletePipe(string
                                                .Format("automagic-mysql-{0}", et.TrimEnd()
                                                .Replace(".", "-")
                                                .Replace("_", "-")));
                                            }
                                            else if (Dbase.ToLower().Contains("postgresql"))
                                            {
                                                client.DeletePipe(string
                                                .Format("automagic-postgresql-{0}", et.TrimEnd()
                                                .Replace(".", "-")
                                                .Replace("_", "-")));
                                            }

                                        }
                                        catch (HttpRequestException ex)
                                        {
                                            Console.WriteLine($"The following error occurred when trying to configure a sesam pipe:\n {ex}");
                                        }
                                        finally
                                        {
                                            client.CreatePipe(pipe);
                                        }
                                    } 
                                }
                                await turnContext.SendActivityAsync("Your pipes have now been created! Open and login to Sesam to take a look and use your new pipes.");
                                await turnContext.SendActivityAsync("Thank you for your patience!");
                            }
                            
                            catch (Exception ex)
                            {
                                await turnContext.SendActivityAsync($"Error occurred: " + ex.Message + " : " + ex.StackTrace);
                            }
                        }    
                        else if (profile.configChoice.ToString().Contains("Foreign Key Mapping"))
                        {
                            await turnContext.SendActivityAsync($"I will now run a scan of your database based on foreign key mapping...");
                            await turnContext.SendActivityAsync($"Give me a minute or so.");
                            try
                            {
                                // Setting variables
                                string User = profile.dbUser.ToString();
                                string Dbase = profile.Dbase.ToString();
                                string Namedb = profile.dbName.ToString();
                                string Host = profile.IP.ToString();
                                string Port = profile.dbPort.ToString();
                                string Password = profile.dbPassword.ToString();
                                string SesamJwt = profile.JwTSesam.ToString();
                                string SesamSubscriptionId = profile.SubIDSesam.ToString();
                                
                                // Making connection to different databases dynamic - add databases here when needed
                                string connSql= "";
                                Db db = null;   
                                if (Dbase.ToLower().Contains("mysql")) 
                                {
                                    connSql = string.Format("server={0};uid={1};pwd={2};database={3}", Host, User, Password, Namedb);
                                    db = new MySQLDb(connSql);
                                }
                                else if (Dbase.ToLower().Contains("postgresql"))
                                {
                                    connSql = string.Format("User ID={0};Password={1};Host={2};Port={3};Pooling=true;Database={4}", User, Password, Host, Port, Namedb);
                                    db = new PostgreSqlDb(connSql);
                                }

                                var se = new ModelBuilder(db);
                                var m = new Core.MetaModel.Model(new Core.System() {Name = "test"});

                                WebClient myWebClient = new WebClient();
                                Stream firstnames = myWebClient.OpenRead("https://csbf8630753f67dx4346x85e.blob.core.windows.net/automagic-test/firstnames.csv");
                                Stream alllastnames = myWebClient.OpenRead("https://csbf8630753f67dx4346x85e.blob.core.windows.net/automagic-test/alllastnames.csv");

                                var refData = new ReferenceDataBlobs(firstnames, alllastnames);
                                firstnames.Close();
                                alllastnames.Close();

                                se.PopulateModel(m);

                                var pdef = new PersonalDataFinder(refData);
                                var candidates = pdef.GetPersonalDataRoots(m, db, new List<string>(Array.Empty<string>()));

                                //Setting FKey reference variables
                                StringBuilder fKeyTables = new StringBuilder();
                                StringBuilder niRefColumns = new StringBuilder();
                                StringBuilder niRefTables = new StringBuilder();
                                StringBuilder fKeyNiTables = new StringBuilder();
                                StringBuilder fKeyNiColumns = new StringBuilder();
                                
                                // Logic here for making NI for either database run
                                List<string> concatlist = new List<string>();

                                if (Dbase.ToLower().Contains("mysql"))
                                {
                                    FKeyQuery query = new FKeyQuery(fKeyTables, niRefColumns, niRefTables, fKeyNiColumns, fKeyNiTables, db, Namedb);
                                    //---------------------------------

                                    // Logic here for correcly creating NI in a concatenated list for foreign key mapping
                                    var niRefColumnsArray = niRefColumns.ToString().TrimEnd().Split("\n");
                                    var niRefTablesArray = niRefTables.ToString().TrimEnd().Split("\n");
                                    var fKeyNiTablesArray = fKeyNiTables.ToString().TrimEnd().Split("\n");
                                    var fKeyNiColumnsArray = fKeyNiColumns.ToString().TrimEnd().Split("\n");     
                                    int no = 0;
                                    foreach(var e in niRefTablesArray)
                                    {
                                        concatlist.Add(fKeyNiTablesArray[no] + ";" + e + ";" + niRefColumnsArray[no] + ";" + fKeyNiColumnsArray[no]);
                                        no++;
                                    }  
                                }
                                 
                                else if (Dbase.ToLower().Contains("postgresql"))
                                {
                                    //Setting all columns and tables variables from column schema 
                                    StringBuilder allColumns = new StringBuilder();
                                    StringBuilder allTables = new StringBuilder();
                                    GetAllQueryPostGreSQL queryAll = new GetAllQueryPostGreSQL(allColumns, allTables, db, Namedb);
                                    string id = "id"; // Making sure id is included
                                    if (allColumns.ToString().Contains("id") == false)
                                    {   
                                        allColumns.AppendLine(id);
                                    }
                                    
                                    //Setting fkey refs for postgresql
                                    StringBuilder fKeyTablesPostgreSQL = new StringBuilder();
                                    StringBuilder fKeyColumnsPostgreSQL = new StringBuilder();
                                    FKeyQueryPostgreSQL getFKeys = new FKeyQueryPostgreSQL(fKeyTablesPostgreSQL, fKeyColumnsPostgreSQL, db, Namedb);

                                    //Getting matched values
                                    StringBuilder concatenatedList = new StringBuilder();
                                    FKeyMatchQueries query = new FKeyMatchQueries(fKeyTablesPostgreSQL, allTables, fKeyColumnsPostgreSQL, allColumns, concatenatedList, db, Namedb);
                                    //----------------------------------

                                    // Logic here for correcly creating NI in a concatenated list for foreign key mapping
                                    var concatListArray = concatenatedList.ToString().TrimEnd().Split("\n");     
                                    int no = 0;
                                    foreach(var e in concatListArray)
                                    {
                                        concatlist.Add(e);
                                        no++;
                                    } 
                                }

                                // Generating string array with Fkey referencing PII tables
                                StringBuilder tablesKeyPii = new StringBuilder();
                                foreach (var et in candidates)
                                {    
                                    if (et.TotalScore >= 1)
                                    {
                                        foreach (string i in et.EntityType.Name.ToString().TrimEnd().Split("\n"))
                                        {
                                            if (tablesKeyPii.ToString().Contains(i) == false)
                                            {
                                                tablesKeyPii.AppendLine(i);
                                            }
                                        }
                                    }
                                }

                                foreach (string i in fKeyTables.ToString().TrimEnd().Split(";"))
                                {
                                    if (tablesKeyPii.ToString().Contains(i) == false)
                                    {
                                        tablesKeyPii.AppendLine(i);
                                    }
                                }

                                // Returning results in the chatbot
                                await turnContext.SendActivityAsync($"The following tables were found containing personal identifiable indicators.\n " + tablesKeyPii);
                                //await turnContext.SendActivityAsync($"Which of the tables would you like me to create a pipe from?");
                                //StringBuilder chosenTables = new StringBuilder();
                                //int counts = 1;
                                //foreach (string table in tablesKeyPii.ToString().TrimEnd().Split("\n"))
                                //{ 
                                    //for(int e = 0 ; e < counts ; e++) 
                                    //{   
                                    //    var reply = MessageFactory.SuggestedActions(
                                    //    new CardAction[]
                                    //    {
                                    //        new CardAction() { Title = "Yes", Type = ActionTypes.ImBack, Value = "Yes" },
                                    //        new CardAction() { Title = "No", Type = ActionTypes.ImBack, Value = "No" }
                                    //    }, text: $"Should table : '{table}' be included?");
                                    //    Console.WriteLine("Now writing the reply " + reply);
                                    //    await turnContext.SendActivityAsync(reply);
                                    //}
                                //}
                                await turnContext.SendActivityAsync($"I will now make pipes of the found tables. The tables will be named with the prefix 'automagic'.");
                                await turnContext.SendActivityAsync($"Operation in progress...");

                                //create a system
                                var client = new SesamNetCoreClient.Client(SesamJwt, SesamSubscriptionId);
                                SesamSystem system = null;
                                if (Dbase.ToLower().Contains("mysql"))
                                {
                                    system = new SesamSystem(Namedb)
                                        .OfType(SystemType.MYSQL)
                                        .With("host", Host)
                                        .With("database", Namedb)
                                        .With("username", User)
                                        .With("password", Password);
                                }
                                else if (Dbase.ToLower().Contains("postgresql"))
                                {
                                    system = new SesamSystem(Namedb)
                                        .OfType(SystemType.POSTGRESQL)
                                        .With("host", Host)
                                        .With("database", Namedb)
                                        .With("username", User)
                                        .With("password", Password);
                                }
                                
                                try
                                { 
                                    client.CreateSystem(system);
                                }
                                catch (HttpRequestException ex)
                                {
                                    Console.WriteLine("Skipping the creation of system, because it already exists. Skipping with error: \n{0}", ex);
                                }

                                // create pipes
                                foreach (string o in tablesKeyPii.ToString().TrimEnd().Split("\n"))
                                {    
                                    if (o != "")
                                    {   
                                        var namePrefix = o.TrimEnd()
                                            .Replace(".", "-")
                                            .Replace("_", "-");
                                        var pipe = new Pipe(string
                                            .Format("automagic-{0}", o.TrimEnd()
                                            .Replace(".", "-")
                                            .Replace("_", "-")));
                                        
                                        var source = new SqlSource();
                                        source.SetTable(o.TrimEnd().Split('.')[1]);
                                        source.SetSystem(Namedb);
                                        source.SetType("sql");
                                        source.SetKey("id");

                                        var transform = new Transform();
                                        transform.SetType("dtl");
                                        transform.AddRule();
                                        transform.MakeDefaultRule();
                                        transform.AddCopy("default", "*");
                                        transform.AddRdfType("default", "automagic-"+o.TrimEnd().Replace(".", "-").Replace("_", "-"), o.TrimEnd().Split('.')[1]);

                                        foreach (string f in concatlist)
                                        {        
                                            if (f != "")
                                            {
                                                if (f.TrimEnd().Split(";")[0] == o.TrimEnd().Split(".")[1])
                                                {        
                                                    transform.AddMakeNi("default", o.TrimEnd().Replace(".", "-").Replace("_", "-").Replace(o.TrimEnd().Split(".")[1], f.TrimEnd().Split(";")[1])+"-"+f.TrimEnd().Split(";")[2], "automagic-"+o.TrimEnd().Replace(".", "-").Replace("_", "-").Replace(o.TrimEnd().Split(".")[1], f.TrimEnd().Split(";")[1]), f.TrimEnd().Split(";")[3]);
                                                }
                                            }
                                        }

                                        pipe.WithSource(source).WithTransform(transform);    
                                        
                                        try
                                        {
                                            client.DeletePipe(string
                                            .Format("automagic-{0}", o.TrimEnd()
                                            .Replace(".", "-")
                                            .Replace("_", "-")));
                                        }
                                        catch (HttpRequestException ex)
                                        {
                                            Console.WriteLine($"The following error occurred when trying to configure a sesam pipe:\n {ex}");
                                        }
                                        finally
                                        {
                                            client.CreatePipe(pipe);
                                        }
                                    }
                                }
                                await turnContext.SendActivityAsync("Your pipes have now been created! Open and login to Sesam to take a look and use your new pipes.");
                                await turnContext.SendActivityAsync("Thank you for your patience!");
                            }
                            
                            catch (Exception ex)
                            {
                                await turnContext.SendActivityAsync($"Error occurred: " + ex.Message + " : " + ex.StackTrace);
                            }
                        }
                        else
                        {
                            break;
                        }
                        await turnContext.SendActivityAsync($"Type anything to run the bot again.");
                        flow.LastQuestionAsked = ConversationFlow.Question.None;
                        profile = new UserProfile();
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                
                }
            }
                        
        private static bool ValidateName(string input, out string name, out string message)
        {
            name = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a name that contains at least one character.";
            }
            else
            {
                name = input.Trim();
            }

            return message is null;
        }

        private static bool ValidateDBase(string input, out string database, out string message)
        {
            database = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a valid name for your database type.";
            }
            else
            {
                database = input.Trim();
            }
            return message is null;
        }

        private static bool ValidateIP(string input, out string ip_adress, out string message)
        {
            ip_adress = null;
            message = null;
            
            try
            {
                string ipAddr = input;
                bool flag = IPAddress.TryParse(ipAddr, out System.Net.IPAddress IP);
                if (flag == true && ipAddr.Contains(".") || ipAddr.ToLower().Contains("localhost"))
                {
                    ip_adress = ipAddr;
                }
                else
                {
                    message = "Please enter a valid IP address/Host server.";
                }
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an IP address. Please type something like the following : '130.45.78.200'.";
            }

            return message is null;
        }

        private static bool ValidateDbName(string input, out string dbName, out string message)
        {
            dbName = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a database that contains at least one character.";
            }
            else
            {
                dbName = input.Trim();
            }

            return message is null;
        }

        private static bool ValidateDbPort(string input, out string dbPort, out string message)
        {
            dbPort = null;
            message = null;

            if (input.Length != 4)
            {
                message = "Please enter a valid port.";
            }
            else
            {
                dbPort = input.Trim();
            }

            return message is null;
        }

        private static bool ValidateDbUser(string input, out string dbUser, out string message)
        {
            dbUser = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a username that contains at least one character.";
            }
            else
            {
                dbUser = input.Trim();
            }

            return message is null;
        }

        private static bool ValidateDbPassword(string input, out string dbPassword, out string message)
        {
            dbPassword = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a password that contains at least one character.";
            }
            else
            {
                dbPassword = input.Trim();
            }

            return message is null;
        }

        private static bool ValidateConfigChoice(string input, out string configChoice, out string message)
        {
            configChoice = null;
            message = null;

            if (input.Contains("Index Mapping") || input.Contains("Foreign Key Mapping"))
            {
                configChoice = input;
            }
            else
            {
                message = "Please enter a valid choice for scanning your database.";
            }

            return message is null;
        }

    }
}
