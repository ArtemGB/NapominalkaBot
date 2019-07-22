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

                // Новое сообщение
                case "message_new":
                    {
                        MesAnswer(updates);
                        break;
                    }
            }

            return Ok("ok");
        }

        [HttpPost]
        public void MesAnswer([FromBody] Updates updates)
        {
            var msg = Message.FromJson(new VkResponse(updates.Object));
            string mesg = msg.Text.ToLower();
            if (mesg.Contains("привет"))
            {
                _vkApi.Messages.Send(new MessagesSendParams
                {
                    Message = "Здарова.))"
                });
            }
            else if (mesg.Contains("как дела"))
            {
                _vkApi.Messages.Send(new MessagesSendParams
                {
                    Message = "Збс, твои?)"
                });
            } 
            else
            {
                _vkApi.Messages.Send(new MessagesSendParams
                {
                    Message = "Чёт я тебя не понял."
                });
            }
        }
    }
}

