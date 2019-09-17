using System.Collections.Generic;

namespace VkBot.Controllers
{
    public static class SendMsg
    {
        public readonly static Dictionary<string, string> Answers = new Dictionary<string, string>
        {
            {"привет", "Здарова."},
            {"как дела?","Нормас.)"},
            {"инструкция","Напиши \"добавить\", чтобы добавить напоминание.\n Напиши \"покажи\", чтобы посмотреть список напоминаний. \n Напиши \"очистить\", чтобы очистить список задач."},
        };
        public readonly static string TaskAddingFistInstruction = "Что тебе напомнить?";
        public readonly static string TaskDateAddingInstruction = "Напиши в формате \"дд.мм чч:мм\", когда тебе напомнить.";
        public readonly static string EmptyTaskList = "У вас нет напоминаний.";
        public readonly static string ClearTasks = "Список напоминаний был очищен.";
        public readonly static string BadEntry = "Вы ввели неверные данные, попробуй ещё раз или напиши \"отмена\", если передумал.";
        public readonly static string Cancel = "Отменено.";
    }
}