using System.Collections.Generic;

namespace VkBot.Controllers
{
    public static class SendMsg
    {
        public readonly static Dictionary<string, string> Answers = new Dictionary<string, string>
        {
            {"привет", "Здарова."},
            {"как дела","Нормас.)"},
            {"инструкция","Напиши \"напомни\", чтобы добавить напоминание.\n Напиши \"покажи\", чтобы посмотреть список напоминаний. \n Напиши \"очистить\", чтобы очистить список задач."},
        };
        public readonly static string TaskAddingFistInstruction = "Что тебе напомнить?";
        public readonly static string TaskDateAddingInstruction = "Напиши, когда тебе напомнить, в формате \"дд.мм чч:мм\", либо в формате \"через дд.мм чч:мм\", чтобы напомнить через указанное время.";
        public readonly static string EmptyTaskList = "У вас нет напоминаний.";
        public readonly static string ClearTasks = "Список напоминаний был очищен.";
        public readonly static string BadEntry = "Вы ввели неверные данные, попробуй ещё раз или напиши \"отмена\", если передумал.";
        public readonly static string ZeroOrVeryBigDate = "Значение полей дд.мм не могут быть равны нулю, значение месяца не может быть больше 12, а дней больше 31.";
        public readonly static string Cancel = "Отменено.";
        public readonly static string DateBeforeError = "Боюсь, что уже слишком поздно, попробуй более позднее время.";
    }
}