using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VkBot.Controllers;
using VkBot.Strings;
using VkBot.Users;

namespace VkBot
{
    internal sealed class RemindService : BackgroundService, IRemindService
    {
        private const int InitialDelay = 5 * 1000;  //5 seconds;
        private const int Delay = 60000;

        private readonly ILogger<RemindService> m_Logger;
        private readonly IServiceProvider m_ServiceProvider;

        public RemindService(ILogger<RemindService> logger, IServiceProvider serviceProvider)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            m_Logger = logger;
            m_ServiceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                m_Logger.LogDebug($"RemindService is starting.");

                stoppingToken.Register(() => m_Logger.LogDebug($"RemindService background task is stopping because cancelled."));

                if (!stoppingToken.IsCancellationRequested)
                {
                    m_Logger.LogDebug($"RemindService is waiting to be scheduled.");
                    await Task.Delay(InitialDelay, stoppingToken);
                }

                m_Logger.LogDebug($"RemindService is working.");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Reminder();

                    await Task.Delay(Delay);
                }

                m_Logger.LogDebug($"RemindService background task is stopping.");
            }
            catch (Exception ex)
            {
                m_Logger.LogDebug("RemindService encountered a fatal error while w task is stopping: {Exception}.", ex.ToString());
            }
        }

        private async Task Reminder()
        {
            AllUsers allUsers = Tasker.OpenAll();
            DateTime UtcNow = DateTime.UtcNow.AddHours(3);
            if (allUsers == null) return;
            foreach (var usr in allUsers.Users)
            {
                foreach (var tsk in usr.Value.Tasks)
                {
                    TimeSpan Difference = UtcNow.Subtract(tsk.TaskDate);
                    if (Difference.Days == 0 && Difference.Hours == 0 && Math.Abs(Difference.Minutes) < 2)
                    {
                        Tasker.VKSendMsg(usr.Key, MsgTexts.Remind + tsk.TaskText + MsgTexts.EndRemind);
                        allUsers.Users[usr.Key].Tasks = allUsers.Users[usr.Key].Tasks
                            .Where(x => x.TaskText != tsk.TaskText && x.TaskDate != tsk.TaskDate).ToList();
                    }
                }
            }
            Console.WriteLine("Reminder.");
            await Task.Delay(1000);
        }

    }
}
