using System;
using System.Collections.Generic;

namespace VkBot.Users
{
    [Serializable]
    public class AllUsers
    {
      public Dictionary<long, VkUser> Users;
      public AllUsers()
      {
          Users = new Dictionary<long, VkUser>();
      }
    }
}