using System.Collections.Generic;

namespace VkBot.Controllers
{
    public static class SendMsg
    {
        public readonly static Dictionary<string, string> Answers = new Dictionary<string, string>
        {
            {"привет", "Здарова"},
            {"как дела?","Нормас.)"},
            {"инструкция","её пока что делают)"}
        };
    }
}