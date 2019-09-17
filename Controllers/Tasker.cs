using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Abstractions;
using System.Collections.Generic;

namespace VkBot.Controllers
{
    public class Tasker : Controller
    {
        ///<summary>
        ///Работа с задачами пользователей.
        ///</summary>
        private static IVkApi vkApi;
        public List<User> Users;

        public Tasker(IVkApi _vkApi)
        {
            vkApi = _vkApi;
        }

        public static bool IsTaskChangingInProgress;
        public delegate void TaskDelegat(Message msg);
        public static TaskDelegat TaskProcces;
        public static List<(string, DateTime)> Tasks = new List<(string, DateTime)>();//Удалить потом.
        public static void StartTaskAdding(Message msg)
        {
            IsTaskChangingInProgress = true;
            VKSendMsg(msg.PeerId.Value, SendMsg.TaskAddingFistInstruction);
            TaskProcces = AddTaskText;
        }

        public static void AddTaskText(Message msg)
        {
            Tasks.Add((msg.Text, DateTime.Now));
            VKSendMsg(msg.PeerId.Value, SendMsg.TaskDateAddingInstruction);
            TaskProcces = AddTaskDate;
        }

        public static void AddTaskDate(Message msg)
        {

            //TaskTime.AddHours(3); //Поправка на московское время, т.к. сервер находится в Европе.
            try
            {
                string[] DateAndTime = msg.Text.Split(' ');
                string DateMMDD = DateAndTime[0], TimeHHMM = DateAndTime[1];
                string[] Date = DateMMDD.Split(',', '.');
                string[] Time = TimeHHMM.Split(':');
                DateTime TaskTime = DateTime.Today;
                TaskTime.AddDays(double.Parse(Date[0]));
                TaskTime.AddMonths(int.Parse(Date[1]));
                TaskTime.AddHours(double.Parse(Time[0]));
                TaskTime.AddMinutes(double.Parse(Time[0]));
                Tasks[Tasks.Count - 1] = (Tasks[Tasks.Count - 1].Item1, TaskTime);
                VKSendMsg(msg.PeerId.Value, "Напоминание добавлено.");
                IsTaskChangingInProgress = false;
            }
            catch (System.Exception)
            {
                VKSendMsg(msg.PeerId.Value, SendMsg.BadEntry);
            }
        }

        public static void ShowTasks(Message msg)
        {
            if (Tasks.Count != 0)
            {
                string tasks = "Твои напоминания:\n";
                foreach (var task in Tasks)
                    tasks += "\n" + task.Item1 + " " + task.Item2.ToLongDateString() + " " + task.Item2.ToShortTimeString() + "\n";
                VKSendMsg(msg.PeerId.Value, tasks);
            }
            else VKSendMsg(msg.PeerId.Value, SendMsg.EmptyTaskList);

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
            VKSendMsg(PeerId, SendMsg.ClearTasks);
        }
    }
}