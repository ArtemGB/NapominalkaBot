using System.Collections.Generic;

namespace VkBot.Controllers
{
    public static class SendMsg
    {
        public readonly static string FirstHellowMsg = " Привет, меня зовут f1rstvkbot. \nПока что я умею только отвечать на вопрос: \"Как дела\"";
        public readonly static string HellowAnsw = "Здарова.))";
        public readonly static string HowAreYouAnsw = "Нормас, ты как?)";
        public readonly static string DontUnderstadAnsw = "Чёт я тебя не понял.(";

        public readonly static Dictionary<string, string> Answers = new Dictionary<string, string>
        {
            {"привет", "Здарова"},
            {"как дела?","Нормас.)"},
            {"Инструкция","Её пока что делают)"}
        };
    }
}