using System;
using System.Threading.Tasks;


namespace VkBot.Users
{
    [Serializable]
    public class UserTask
    {
        public string TaskText;
        public DateTime TaskDate;
        public UserTask(string TaskText, DateTime TaskDate)
        {
            this.TaskText = TaskText;
            this.TaskDate = TaskDate;
        }

        public override string ToString() => TaskText + " " + TaskDate.ToShortDateString() + " " + TaskDate.ToShortTimeString();
    }
}