using System;
using Microsoft.AspNetCore.Mvc;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Abstractions;
using System.Collections.Generic;

namespace VkBot.Controllers
{
    public class TaskMethods : ControllerBase
    {
        public IActionResult StartAdding()
       {
           return Ok("ok");
       }
    }
}