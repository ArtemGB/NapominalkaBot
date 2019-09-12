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
        private readonly IConfiguration _configuration;
        private readonly IVkApi _vkApi;

        private List<string> Tasks = new List<string>();

        public CallbackController(IVkApi vkApi, IConfiguration configuration)
        {
            _configuration = configuration;
            _vkApi = vkApi;
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
                        return Ok(_configuration["Config:Confirmation"]);
                    }
                case "message_allow":
                    {
                        _vkApi.Messages.Send(new MessagesSendParams
                        {
                            RandomId = new DateTime().Millisecond,
                        });
                        break;
                    }
                // Новое сообщение
                case "message_new":
                    {
                        var msg = Message.FromJson(new VkResponse(updates.Object));
                        _vkApi.Messages.Send(new MessagesSendParams
                        {
                            RandomId = new DateTime().Millisecond,
                            PeerId = msg.PeerId.Value,
                            Message = MsgAnswer(msg.Text)
                        });
                        break;
                    }
            }

            return Ok("ok");
        }

        public string MsgAnswer(string msg)
        {
            string mess = msg.ToLower();
            _vkApi.Messages.Send(new MessagesSendParams
                        {
                            Message = "Ответ метода."
                        });
            try
            {
                return SendMsg.Answers[mess];
            }
            catch (System.Exception)
            {
              return "Чёт я тебя не понял.( Напиши слово \"Инструкция\" и я скажу, что умею";   
            }
            
        }

        public void AddTask(string TastText)
        {

        }

    }
}

