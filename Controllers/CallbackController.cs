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
        private static AllUsers allUsers = new AllUsers();


        public CallbackController(IVkApi _vkApi, IConfiguration configuration)
        {
            this.configuration = configuration;
            vkApi = _vkApi;
            tasker = new Tasker(vkApi);
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
                        /* if (!allUsers.Users.ContainsKey(msg.UserId.Value)) //Добавление нового пользователя.
                        {
                            allUsers.Users.Add(msg.UserId.Value, new VkUser(msg.UserId.Value));
                            VKSendMsg(msg.PeerId.Value, MsgTexts.HelloNewUser);
                        }
                        else */
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
            if (mess == "отмена" && Tasker.IsTaskChangingInProgress == true)
            {
                Tasker.IsTaskChangingInProgress = false;
                VKSendMsg(msg.PeerId.Value, MsgTexts.Cancel);
                return;
            }
            if (Tasker.IsTaskChangingInProgress == true)
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
                        Tasker.ClearTasks(msg.PeerId.Value);
                        break;
                    }
                case "время":
                    {
                        VKSendMsg(msg.PeerId.Value, DateTime.Now.AddHours(3).ToLocalTime().ToString());
                        break;
                    }
                case "время сообщения":
                    {
                        VKSendMsg(msg.PeerId.Value, msg.Date.Value.ToString());
                        break;
                    }
                case "пользователи":
                    {
                        if (allUsers.Users.Count > 0)
                        {
                            string users = "Пользователи:\n";
                            foreach (var user in allUsers.Users)
                                users += user.Value.VkId + "\n";
                            VKSendMsg(msg.PeerId.Value, users);
                        }
                        else VKSendMsg(msg.PeerId.Value, "Пользователей нет.");
                        break;
                    }
                case "Id":
                    {
                        VKSendMsg(msg.PeerId.Value, msg.UserId.Value.ToString());
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
                            Answer = "Чёт я тебя не понял.( Напиши слово \"Инструкция\" и я скажу, что умею.";
                        }
                        VKSendMsg(msg.PeerId.Value, Answer);
                        break;
                    }
            }
        }
    }
}