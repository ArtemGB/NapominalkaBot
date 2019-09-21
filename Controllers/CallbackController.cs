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

        public static Tasker tasker;

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
                        MsgReceiver(msg);
                        break;
                    }
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
                VKSendMsg(msg.PeerId.Value, SendMsg.Cancel);
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
    }
}