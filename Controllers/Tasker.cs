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
            TaskProcces = TaskAddingComplete;
        }

        public static void AddTaskDateInstruction(Message msg)
        {
            VKSendMsg(msg.PeerId.Value, SendMsg.TaskDateAddingInstruction);
            TaskProcces = AddTaskDate;
        }

        public static void AddTaskDate(Message msg)
        {
            string[] Time = msg.Text.Split(":");
            DateTime TaskTime = DateTime.Now;
            TaskTime.AddHours(Convert.ToDouble(Time[0]));
            TaskTime.AddMinutes(Convert.ToDouble(Time[1]));
            Tasks[Tasks.Count - 1] = (Tasks[Tasks.Count - 1].Item1, TaskTime);
            TaskAddingComplete(msg);
        }

        public static void TaskAddingComplete(Message msg)
        {
            VKSendMsg(msg.PeerId.Value, "Напоминание добавлено.");
            IsTaskChangingInProgress = false;
        }

        public static void ShowTasks(Message msg)
        {
            string tasks = "Твои напоминания:\n";
            foreach (var task in Tasks)
                tasks += "\n" + task.Item1 + " " + task.Item2.ToLongDateString() + "\n";
            VKSendMsg(msg.PeerId.Value, tasks);
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
    }
}