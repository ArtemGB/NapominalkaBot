using System;

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

        public override string ToString() => TaskText + "\n" + TaskDate.ToShortDateString() + " " + TaskDate.ToShortTimeString();
    }
}