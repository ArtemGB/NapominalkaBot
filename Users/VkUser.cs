using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace VkBot.Users
{
    [Serializable]
    public class VkUser
    {
        public long VkId { get; private set; }
        public List<UserTask> Tasks;

        public VkUser(long VkId)
        {
            this.VkId = VkId;
            Tasks = new List<UserTask>();
        }

    }
}