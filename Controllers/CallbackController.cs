using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Abstractions;
using VkNet.Model.RequestParams;
using System;

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
                        Message = SendMsg.FirstHellowMsg,
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
            string mesg = msg.ToLower();
            if (mesg.Contains("привет"))
                return SendMsg.HellowAnsw;
            else if (mesg.Contains("как дела"))
                return SendMsg.HowAreYouAnsw;
            else
                return SendMsg.DontUnderstadAnsw;
        }
    }
}

