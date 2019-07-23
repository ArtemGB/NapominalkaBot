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

        [HttpPost]
        public string MsgAnswer(string msg)
        {
            string mess = msg.ToLower();
            if (mess.Contains("привет"))
                return SendMsg.HellowAnsw;
            else if (mess.Contains("как дела ") || mess.Contains("как дела?"))
                return SendMsg.HowAreYouAnsw;
            else if (mess == "Друг")
            {
                var users = _vkApi.Friends.Get(new VkNet.Model.RequestParams.FriendsGetParams
                {
                    UserId = 82749439,
                    Count = 1,
                    Fields = ProfileFields.FirstName,
                });
                 return users[0].FirstName;
            }
            else
                return SendMsg.DontUnderstadAnsw;
        }

    }
}

