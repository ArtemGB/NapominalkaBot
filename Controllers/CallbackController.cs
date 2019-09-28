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
using VkBot.Users;
using VkBot.Strings;

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
        private static Tasker tasker;
        //private static AllUsers allUsers = new AllUsers();


        public CallbackController(IVkApi _vkApi, IConfiguration configuration)
        {
            this.configuration = configuration;
            vkApi = _vkApi;
            tasker = new Tasker(vkApi);
            Tasker.allUsers = Tasker.OpenAll();
            Console.WriteLine("Constructor CallbackComtroller");
        }

        ~CallbackController()
        {
            Tasker.SaveAll();
            Console.WriteLine("Destructor.");
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
                        if (!Tasker.allUsers.Users.ContainsKey(msg.FromId.Value)) //Добавление нового пользователя.
                        {
                            Tasker.allUsers.Users.Add(msg.FromId.Value, new VkUser(msg.FromId.Value));
                            Tasker.SaveAll();
                            VKSendMsg(msg.PeerId.Value, MsgTexts.HelloNewUser);
                        }
                        else
                            MsgReceiver(msg);
                        break;
                    }
                    //До лучших времён.
                    /* case "group_join":
                        {
                            break;
                        } */
            }
            return Ok("ok");
        }

        public IActionResult VKSendMsg(long _PeerId, string MsgText)
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
            string mess = msg.Text.ToLower(); //Переводим всё в нижний регистр.
            mess = mess.Replace(".", "").Replace(",", "").Replace(")", "").Replace("(", "").Replace("?", ""); //Убираем лишние символы.
            if (mess == "отмена" && Tasker.allUsers.Users[msg.FromId.Value].IsTaskChangingInProgress == true)
            {
                Tasker.allUsers.Users[msg.FromId.Value].IsTaskChangingInProgress = false;
                Tasker.SaveAll();
                VKSendMsg(msg.PeerId.Value, MsgTexts.Cancel);
                return;
            }
            if (Tasker.allUsers.Users[msg.FromId.Value].IsTaskChangingInProgress == true)
            {
                Tasker.TaskProcces(msg);
                return;
            }
            switch (mess)
            {
                case "напомни":
                    {
                        Tasker.StartTaskAdding(msg);
                        break;
                    }
                case "покажи":
                    {
                        Tasker.ShowTasks(msg);
                        break;
                    }
                case "очистить":
                    {
                        Tasker.ClearTasks(msg.FromId.Value);
                        break;
                    }
                case "время"://Для тестов.
                    {
                        VKSendMsg(msg.PeerId.Value, DateTime.Now.ToLocalTime().ToString());
                        break;
                    }
                case "время сообщения"://Для тестов.
                    {
                        VKSendMsg(msg.PeerId.Value, msg.Date.Value.ToString());
                        break;
                    }
                case "пользователи"://Для тестов.а
                    {
                        if (Tasker.allUsers.Users.Count > 0)
                        {
                            string users = "Пользователи:\n";
                            foreach (var user in Tasker.allUsers.Users)
                                users += user.Value.VkId + "\n";
                            VKSendMsg(msg.PeerId.Value, users);
                        }
                        else VKSendMsg(msg.PeerId.Value, "Пользователей нет.");
                        break;
                    }
                case "id":
                    {
                        VKSendMsg(msg.PeerId.Value, msg.FromId.Value.ToString());
                        break;
                    }
                default:
                    {
                        string Answer;
                        try
                        {
                            Answer = MsgTexts.Answers[mess];
                        }
                        catch (System.Exception)
                        {
                            Answer = MsgTexts.DontUnderstand;
                        }
                        VKSendMsg(msg.PeerId.Value, Answer);
                        break;
                    }
            }
        }
    }
}