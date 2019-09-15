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
        private IVkApi vkApi;
        public delegate void TaskDeleg();
        public delegate IActionResult TaskProcces();
        public bool IsTaskCreatingInProgres;
        
        public TaskMethods taskMethods = new TaskMethods();

        public Dictionary<string, TaskDeleg> TaskControll = new Dictionary<string, TaskDeleg>()
        {
            {"добавить", new TaskDeleg(Huy)}
        };
        
        public Tasker(IVkApi vkApi)
        {
            this.vkApi = vkApi;
        }
       public static void Huy()
       {

       }

       public static IActionResult Huy11()
       {
           return new OkObjectResult("ok");
       }
       
    }
}