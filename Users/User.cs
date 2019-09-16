using System.Collections.Generic;
using System;

namespace Users
{
    public class User
    {
        public int VkId { get; private set; }
        public List<(DateTime, string)> Tasks;

        public User(int VkId)
        {
            this.VkId = VkId;
        }

    }
}