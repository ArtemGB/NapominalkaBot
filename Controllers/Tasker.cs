using System;
using Microsoft.AspNetCore.Mvc;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Abstractions;

namespace VkBot.Controllers
{
    public class Tasker : ControllerBase
    {
        private IVkApi _vkApi;
        
        public Tasker(IVkApi _vkApi)
        {
            this._vkApi = _vkApi;
        }

        public IActionResult SendTestMsg(Message msg)
        {
            _vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = new DateTime().Millisecond,
                UserId = 71947751,
                Message = "This message has been send from other class."
            });
            return Ok("ok");
        }


    }
}