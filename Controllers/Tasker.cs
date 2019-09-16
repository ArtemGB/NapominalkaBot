using System;
using Microsoft.AspNetCore.Mvc;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Abstractions;
using System.Collections.Generic;

namespace VkBot.Controllers
{
    public class Tasker : ControllerBase
    {
        private static IVkApi vkApi;
        public delegate IActionResult TaskProcces();
        public bool IsTaskCreatingInProgres;
        public List<User> Users;
        
        public Tasker(IVkApi _vkApi)
        {
            vkApi = _vkApi;
        }

        public static IActionResult StartAdding()
        {
            vkApi.Messages.Send(new MessagesSendParams
            {
                
            });
            return new OkObjectResult("ok");
        }
    }
}