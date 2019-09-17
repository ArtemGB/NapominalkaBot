using System.Collections.Generic;

namespace VkBot.Controllers
{
    public static class SendMsg
    {
        public readonly static Dictionary<string, string> Answers = new Dictionary<string, string>
        {
            {"привет", "Здарова."},
            {"как дела?","Нормас.)"},
            {"инструкция","Напиши \"добавить\", чтобы добавить напоминание или \"покажи\", чтобы посмотреть список напоминаний"},
        };
        public readonly static string TaskAddingFistInstruction = "Что тебе напомнить?";
        public readonly static string TaskDateAddingInstruction = "Во сколько тебе напомнить. Напиши в формате чч:мм.";
    }
}