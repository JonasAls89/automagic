using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Automagic.Chatbot
{
    public class ConversationFlow
{
    // Identifies the last question asked.
    public enum Question
    {
        Name,
        JwTSesam,
        SubIDSesam,
        Dbase,
        dbPort,
        IP,
        dbName,
        dbUser,
        dbPassword,
        configChoice,
        None, // Our last action did not involve a question.
        
    }

    // The last question asked.
    public Question LastQuestionAsked { get; set; } = Question.None;
}
}
