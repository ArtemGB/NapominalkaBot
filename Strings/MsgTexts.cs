using System;
using System.Collections.Generic;

namespace VkBot.Strings
{
    public static class MsgTexts
    {
        public readonly static Dictionary<string, string> Answers = new Dictionary<string, string>
        {
            {"привет", "Здарова."},
            {"как дела","Нормас.)"},
            {"инструкция","Напиши \"напомни\", чтобы добавить напоминание.\n Напиши \"покажи\", чтобы посмотреть список напоминаний. \n Напиши \"очистить\", чтобы очистить список задач."},
            {"Date", DateTime.UtcNow.ToString() }
        };
        public readonly static string TaskAddingFistInstruction = "Что тебе напомнить?";
        public readonly static string TaskDateAddingInstruction = "Напиши, когда тебе напомнить, в формате \"дд.мм чч:мм\", либо в формате \"через дд.мм чч:мм\", чтобы напомнить через указанное время.";
        public readonly static string EmptyTaskList = "У вас нет напоминаний.";
        public readonly static string ClearTasks = "Список напоминаний был очищен.";
        public readonly static string BadEntry = "Вы ввели неверные данные, попробуй ещё раз или напиши \"отмена\", если передумал.";
        public readonly static string ZeroOrVeryBigDate = "Значение полей дд.мм не могут быть равны нулю, значение месяца не может быть больше 12, а дней больше 31.";
        public readonly static string Cancel = "Отменено.";
        public readonly static string DateBeforeError = "Боюсь, что уже слишком поздно, попробуй более позднее время.";
        public readonly static string HelloNewUser = "Приветсвую тебя, я Напоминатель, ты можешь попросить меня напомнить тебе что-то и я сделаю это в указанное тобой время.)) Напиши \"инструкция\" и я расскажу тебе, как это сделать.))";
        public readonly static string DontUnderstand = "Чёт я тебя не понял.( Напиши слово \"Инструкция\" и я скажу, что умею.";
        public readonly static string Remind = "Хэй, ты просил меня напомнить тебе вот это: ";
        public readonly static string EndRemind = "\n Буду рад напомнить ещё что-нибудь.))";
        public readonly static string NoUsers = "Пользователей нет.";
    }
}