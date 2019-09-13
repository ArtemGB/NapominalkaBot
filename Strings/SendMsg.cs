using System.Collections.Generic;

namespace VkBot.Controllers
{
    public static class SendMsg
    {
        public delegate string TaskDeleg();
        public readonly static Dictionary<string, string> Answers = new Dictionary<string, string>
        {
            {"привет", "Здарова."},
            {"как дела?","Нормас.)"},
            {"инструкция","Её пока что делают.)"},
        };

        public readonly static Dictionary<string, TaskDeleg> Tasks = new Dictionary<string, TaskDeleg>();

        public static string Add()
        {
            return "";
        }
    }
}