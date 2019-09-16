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
        private readonly IVkApi vkApi;

        private Tasker tasker;

        public CallbackController(IVkApi vkApi, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.vkApi = vkApi;
            tasker = new Tasker(this.vkApi);
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
                        MsgAnswer(msg);
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
                Message = MsgText,
            });
            return Ok("ok");
        }

        public void MsgAnswer(Message msg)
        {
            string mess = msg.Text.ToLower();
            string Answer;
            switch (mess)
            {
                case "добавить":
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            try
            {
                Answer = SendMsg.Answers[mess];
            }
            catch (System.Exception)
            {
                Answer = "Чёт я тебя не понял.( Напиши слово \"Инструкция\" и я скажу, что умею.";
            }
            VKSendMsg(msg.PeerId.Value, Answer);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///<summary>
        ///Работа с задачами пользователей.
        ///</summary>

        private List<string> Tasks = new List<string>();//Удалить потом.
        public delegate IActionResult TaskDeleg();
    }
}