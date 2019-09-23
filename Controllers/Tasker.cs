using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Abstractions;
using System.Collections.Generic;
using VkBot.Users;

namespace VkBot.Controllers
{
    public class Tasker : Controller
    {
        ///<summary>
        ///Работа с задачами пользователей.
        ///</summary>
        private static IVkApi vkApi;
        public List<User> Users;

        public Tasker(IVkApi _vkApi) => vkApi = _vkApi;

        public static bool IsTaskChangingInProgress;
        public delegate void TaskDelegat(Message msg);
        public static TaskDelegat TaskProcces; //Переключатель методогв выполнения операций с напоминаниями.
        public static List<UserTask> Tasks = new List<UserTask>();//Удалить потом.
        public static void StartTaskAdding(Message msg)
        {
            IsTaskChangingInProgress = true;
            VKSendMsg(msg.PeerId.Value, MsgTexts.TaskAddingFistInstruction);
            TaskProcces = AddTaskText;
        }

        public static void AddTaskText(Message msg)
        {
            Tasks.Add(new UserTask(msg.Text, DateTime.Now));
            VKSendMsg(msg.PeerId.Value, MsgTexts.TaskDateAddingInstruction);
            TaskProcces = AddTaskDate;
        }

        public static void AddTaskDate(Message msg)
        {
            try
            {
                string[] DateAndTime = msg.Text.Split(' ');
                string DateMMDD = DateAndTime[DateAndTime.Length - 2], TimeHHMM = DateAndTime[DateAndTime.Length - 1];
                string[] Date = DateMMDD.Split(',', '.');
                string[] Time = TimeHHMM.Split(':');
                DateTime TaskTime = new DateTime();
                DateAndTime[0] = DateAndTime[0].ToLower();
                if (DateAndTime[0] == "через")
                {
                    TaskTime = DateTime.UtcNow.AddHours(3);
                    TaskTime = TaskTime.AddDays(double.Parse(Date[0])).AddMonths(int.Parse(Date[1]))
                .AddHours(double.Parse(Time[0])).AddMinutes(double.Parse(Time[1]));
                }
                else
                {
                    if (int.Parse(Date[1]) > 12 || int.Parse(Date[1]) <= 0 || int.Parse(Date[1]) > 31)
                        throw new Exception();
                    TaskTime = new DateTime(DateTime.UtcNow.Year, int.Parse(Date[1]), int.Parse(Date[0]),
                     int.Parse(Time[0]), int.Parse(Time[1]), 0);
                    if (TaskTime <= DateTime.UtcNow.AddHours(3))
                    {
                        VKSendMsg(msg.PeerId.Value, MsgTexts.DateBeforeError);
                        return;
                    }
                }
                Tasks[Tasks.Count - 1].TaskDate = TaskTime;
                VKSendMsg(msg.PeerId.Value, "Напоминание добавлено.");
                IsTaskChangingInProgress = false;
            }
            catch (System.FormatException)
            {
                VKSendMsg(msg.PeerId.Value, MsgTexts.BadEntry);
            }
            catch (Exception)
            {
                VKSendMsg(msg.PeerId.Value, MsgTexts.ZeroOrVeryBigDate);
            }
        }
        
        public static void ShowTasks(Message msg)
        {
            if (Tasks.Count != 0)
            {
                string tasks = "Твои напоминания:\n";
                foreach (var task in Tasks)
                    tasks += "\n" + task.ToString() + "\n";
                VKSendMsg(msg.PeerId.Value, tasks);
            }
            else VKSendMsg(msg.PeerId.Value, MsgTexts.EmptyTaskList);

        }

        public static IActionResult VKSendMsg(long _PeerId, string MsgText)
        {
            vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = DateTime.Now.Millisecond + new Random().Next(),
                PeerId = _PeerId,
                Message = MsgText
            });
            return new OkObjectResult("ok");
        }

        public static void ClearTasks(long PeerId)
        {
            Tasks.Clear();
            VKSendMsg(PeerId, MsgTexts.ClearTasks);
        }
    }
}