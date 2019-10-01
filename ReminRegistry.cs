using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using FluentScheduler;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace VkBot
{
    public class ReminRegistry : Registry
    {
        public ReminRegistry()
        {
            Action Remind = new Action(() =>
            {
                VkApi api = new VkApi();
                api.Authorize(new ApiAuthParams { AccessToken = "fa82ca901cffa5f36b0f92fd860d6984d1df297f1457da04c6032eddd168bd1b9f57245e97d299c7106ad" });
                
                Users.AllUsers allUsers = Controllers.Tasker.OpenAll();
                DateTime UtcNow = DateTime.UtcNow.AddHours(3);
                if (allUsers == null) return;
                foreach (var usr in allUsers.Users)
                {
                    foreach (var tsk in usr.Value.Tasks)
                    {
                        TimeSpan Difference = UtcNow.Subtract(tsk.TaskDate);
                        if (Difference.Days == 0 && Difference.Hours == 0 && Math.Abs(Difference.Minutes) < 2)
                        {
                            //Наломинаем пользователю о его задаче.
                            api.Messages.Send(new MessagesSendParams
                            {
                                RandomId = DateTime.Now.Millisecond + new Random().Next(),
                                UserId = usr.Key,
                                Message = Strings.MsgTexts.Remind + tsk.TaskText + Strings.MsgTexts.EndRemind
                            });

                            //Удаление задачи после напоминания.
                            allUsers.Users[usr.Key].Tasks = allUsers.Users[usr.Key].Tasks
                                .Where(x => x.TaskText != tsk.TaskText && x.TaskDate != tsk.TaskDate).ToList();
                            
                            //Сохранение изменённого списка в файл.
                            BinaryFormatter bf = new BinaryFormatter();
                            try
                            {
                                using (FileStream fs = new FileStream(@"Data/Users.dat", FileMode.OpenOrCreate))
                                {
                                    bf.Serialize(fs, allUsers);
                                }
                                Console.WriteLine("Save was succes");
                                api.Messages.Send(new MessagesSendParams
                                {
                                    RandomId = DateTime.Now.Millisecond + new Random().Next(),
                                    UserId = usr.Key,
                                    Message = String.Format("Таймер сохранил файл.")
                                });
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                    }
                }
            });
            // Schedule an IJob to run at an interval
            Schedule(Remind).ToRunNow().AndEvery(1).Minutes();
        }


    }
}
