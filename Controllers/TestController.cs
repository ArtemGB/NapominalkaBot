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
    public class TestController : ControllerBase
    {
        [HttpPost]
        public static string GetFriends(IVkApi _vkApi)
        {
            var users = _vkApi.Friends.Get(new VkNet.Model.RequestParams.FriendsGetParams
            {
                UserId = 82749439,
                Count = 1,
                Fields = ProfileFields.FirstName,
            });
            return users[0].FirstName;
        }
    }
}