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
        private static IVkApi vkApi;
        public List<User> Users;
        
        public Tasker(IVkApi _vkApi)
        {
            vkApi = _vkApi;
        }

        public static bool IsTaskChangingInProgress;
        public delegate void TaskDelegat(Message msg);
        public static TaskDelegat TaskProcces;
        public static List<string> Tasks = new List<string>();//Удалить потом.
        public static void StartTaskAdding(Message msg)
        {
            IsTaskChangingInProgress = true;
            VKSendMsg(msg.PeerId.Value, SendMsg.TaskAddingFistInstruction);
            TaskProcces = AddTask;
        }

        public static void AddTask(Message msg)
        {
            Tasks.Add(msg.Text);
            TaskProcces = TaskAddingComplete;
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
                tasks += "\n" + task + "\n";
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