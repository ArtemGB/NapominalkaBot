using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace VkBot.Reminder
{
    internal class ReminderHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;

        public ReminderHostedService(ILogger<ReminderHostedService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reminder Background Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Reminder Background Service is working.");
            Remind();
            WakeUp();

        }

        /// <summary>
        /// Делает запрос на самого себя, чтобы сервер не прекращал работу, обеспечивая бесперебойную работу таймера.
        /// </summary>
        public void WakeUp()
        {
            WebRequest request = WebRequest.Create("http://www.u0818396.plsk.regruhosting.ru/api/callback/getanswer");
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            response.Close();
            Console.WriteLine("Request by WakeUp succes.");
        }

        /// <summary>
        /// Проверяет весь список всех задач пользователей и напоминает им, если время напоминания пришло.
        /// </summary>
        public void Remind()
        {
            VkApi api = new VkApi();
            api.Authorize(new ApiAuthParams { AccessToken = "MyAccessToken" });
            Users.AllUsers allUsers = Controllers.Tasker.OpenAll(); //Открываем файл с данными пользователей.
            DateTime UtcNow = DateTime.UtcNow.AddHours(3); //Текущая дата.
            if (allUsers == null) return; //Выходим, если файл с данными пустой.
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
                            Console.WriteLine("Save was succes.");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reminder Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}