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
                        // vkApi.Messages.Send(new MessagesSendParams
                        // {
                        //     RandomId = DateTime.Now.Millisecond + new Random().Next(),
                        //     PeerId = msg.PeerId.Value,
                        //     Message = MsgAnswer(msg.Text)
                        // });
                        MsgAnswer(msg);
                        break;
                    }
            }
            return new OkObjectResult("ok");
            //return Ok("ok");
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

        public IActionResult MsgAnswer(Message msg)
        {
            string mess = msg.Text.ToLower();
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
            return Ok("ok");
        }

        // public string MsgAnswer(string msg)
        // {
        //     string mess = msg.ToLower();
        //     try
        //     {
        //         return SendMsg.Answers[mess];
        //     }
        //     catch (System.Exception)
        //     {
        //         return "Чёт я тебя не понял.( Напиши слово \"Инструкция\" и я скажу, что умею.";
        //     }
        // }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///<summary>
        ///Работа с задачами пользователей.
        ///</summary>

        private List<string> Tasks = new List<string>();//Удалить потом.
        public delegate IActionResult TaskDeleg();
        // public Dictionary<string, TaskDeleg> TaskControll = new Dictionary<string, TaskDeleg>()
        // {
        //     {"добавить", new TaskDeleg(StartAdding)}
        // };

        // public static IActionResult StartAdding()
        // {
        //     vkApi.Messages.Send(new MessagesSendParams
        //     {
        //         RandomId = DateTime.Now.Millisecond + new Random().Next(),
        //         PeerId = ,
        //         Message = ,
        //     });
        //     return new OkObjectResult("ok");
        // }
    }
}