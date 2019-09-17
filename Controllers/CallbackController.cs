using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Abstractions;
using VkNet.Model.RequestParams;
using System;
using VkNet.Enums.Filters;
using System.Linq;
using System.Collections.Generic;

namespace VkBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration configuration;
        private static IVkApi vkApi;

        private Tasker tasker;

        public CallbackController(IVkApi _vkApi, IConfiguration configuration)
        {
            this.configuration = configuration;
            vkApi = _vkApi;
            tasker = new Tasker(vkApi);
            IsTaskChangingInProgress = false;
        }

        [HttpPost]
        public IActionResult Callback([FromBody] Updates updates)
        {
            // Тип события
            switch (updates.Type)
            {
                // Ключ-подтверждение
                case "confirmation":
                    {
                        return Ok(configuration["Config:Confirmation"]);
                    }
                // Новое сообщение
                case "message_new":
                    {
                        var msg = Message.FromJson(new VkResponse(updates.Object));
                        MsgReceiver(msg);
                        break;
                    }
            }
            return Ok("ok");
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

        public void MsgReceiver(Message msg)
        {
            string mess = msg.Text.ToLower();
            if (IsTaskChangingInProgress == true)
            {
                VKSendMsg(msg.PeerId.Value, "Check");
                TaskProcces(msg);
                return;
            }
            switch (mess)
            {
                case "добавить":
                    {
                        StartTaskAdding(msg);
                        break;
                    }
                case "покажи":
                    {
                        ShowTasks(msg);
                        break;
                    }
                default:
                    {
                        string Answer;
                        try
                        {
                            Answer = SendMsg.Answers[mess];
                        }
                        catch (System.Exception)
                        {
                            Answer = "Чёт я тебя не понял.( Напиши слово \"Инструкция\" и я скажу, что умею.";
                        }
                        VKSendMsg(msg.PeerId.Value, Answer);
                        break;
                    }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///<summary>
        ///Работа с задачами пользователей.
        ///</summary>

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
    }
}