using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Abstractions;
using System.Collections.Generic;
using VkBot.Users;
using System.Threading;

namespace VkBot.Controllers
{
    ///<summary>
    ///Работа с напоминаниями пользователей.
    ///</summary>
    public class Tasker : Controller
    {
        private static IVkApi vkApi;
        public static AllUsers allUsers = new AllUsers(); //Хранит все данные пользователей.

        public static bool IsTaskChangingInProgress;//Показывает, выполняется ли сейчас какая-либо операция.
        public delegate void TaskDelegat(Message msg);
        public static TaskDelegat TaskProcces; //Переключатель методогв выполнения операций с напоминаниями.

        private static TimerCallback Invoke;//Делегат на метод сериализации.
        private static Timer Inv;//Таймер, сохраняющий данные пользователей.
        private static int TimerI = 0;
        public Tasker(IVkApi _vkApi)
        {
            Invoke = new TimerCallback(Reminder);
            Inv = new Timer(Reminder, 0, 0, 100000);
            vkApi = _vkApi;
        }

        private static void Reminder(object obj)
        {
            DateTime UtcNow = DateTime.UtcNow.AddHours(3);
            foreach (var usr in allUsers.Users)
            {
                foreach (var tsk in usr.Value.Tasks)
                {
                    //if(UtcNow.Subtract(tsk.TaskDate).Days == UtcNow.Day)
                }
            }
            Console.WriteLine("Timer " + ++TimerI);
        }

        public static void StartTaskAdding(Message msg)//Начинает процесс сохранения напоминания.
        {
            IsTaskChangingInProgress = true;
            VKSendMsg(msg.PeerId.Value, MsgTexts.TaskAddingFistInstruction);
            TaskProcces = AddTaskText;
        }

        public static void AddTaskText(Message msg)//Сохраняет текст напоминания.
        {
            allUsers.Users[msg.FromId.Value].Tasks.Add(new UserTask(msg.Text, DateTime.Now));
            VKSendMsg(msg.PeerId.Value, MsgTexts.TaskDateAddingInstruction);
            TaskProcces = AddTaskDate;
        }

        public static void AddTaskDate(Message msg)//Добавление даты напоминания.
        {
            try
            {
                string[] DateAndTime = msg.Text.Split(' ');//Делим строку с временем на две части: "дд.мм" и "чч:мм".
                string[] Date = DateAndTime[DateAndTime.Length - 2].Split(',', '.');//Разделяем дату на месяцы и дни.
                string[] Time = DateAndTime[DateAndTime.Length - 1].Split(':');//Разделяем время на часы и минуты.
                DateTime TaskTime = new DateTime();
                DateAndTime[0] = DateAndTime[0].ToLower();
                if (DateAndTime[0] == "через")
                {
                    TaskTime = DateTime.UtcNow.AddHours(3);
                    TaskTime = TaskTime.AddDays(double.Parse(Date[0])).AddMonths(int.Parse(Date[1]))
                .AddHours(double.Parse(Time[0])).AddMinutes(double.Parse(Time[1]));
                }
                else
                {
                    if (int.Parse(Date[1]) > 12 || int.Parse(Date[1]) <= 0 || int.Parse(Date[1]) > 31)
                        throw new Exception();
                    TaskTime = new DateTime(DateTime.UtcNow.Year, int.Parse(Date[1]), int.Parse(Date[0]),
                     int.Parse(Time[0]), int.Parse(Time[1]), 0);
                    if (TaskTime <= DateTime.UtcNow.AddHours(3))
                    {
                        VKSendMsg(msg.PeerId.Value, MsgTexts.DateBeforeError);
                        return;
                    }
                }
                allUsers.Users[msg.FromId.Value].Tasks[allUsers.Users[msg.FromId.Value].Tasks.Count - 1].TaskDate = TaskTime;
                AddTaskComplete(msg);//Сообщаем о успешном завершении добавления напоминания.
            }
            catch (System.FormatException)
            {
                VKSendMsg(msg.PeerId.Value, MsgTexts.BadEntry);
            }
            catch (Exception)
            {
                VKSendMsg(msg.PeerId.Value, MsgTexts.ZeroOrVeryBigDate);
            }
        }

        public static void AddTaskComplete(Message msg)
        {
            VKSendMsg(msg.PeerId.Value, "Напоминание добавлено.");
            //SaveAll();
            IsTaskChangingInProgress = false;//Выставляем флаг в 0.
        }

        public static void ShowTasks(Message msg)//Показывает все напоминания для текущего пользователя.
        {
            if (allUsers.Users[msg.FromId.Value].Tasks.Count != 0)
            {
                string tasks = "Твои напоминания:\n";
                foreach (var task in allUsers.Users[msg.FromId.Value].Tasks)
                    tasks += "\n" + task.ToString() + "\n";
                VKSendMsg(msg.PeerId.Value, tasks);
            }
            else VKSendMsg(msg.PeerId.Value, MsgTexts.EmptyTaskList);

        }

        //Отправляет сообщение пользователю.
        public static IActionResult VKSendMsg(long _PeerId, string MsgText)
        {
            vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = DateTime.Now.Millisecond + new Random().Next(),
                PeerId = _PeerId,
                Message = MsgText
            });
            return new OkObjectResult("ok");
        }

        //Сделать запрос на подтверждение.
        public static void ClearTasks(long FromId)//Очищает список напоминаний.
        {
            allUsers.Users[FromId].Tasks.Clear();
            VKSendMsg(FromId, MsgTexts.ClearTasks);
            //SaveAll();
        }

        public static void SaveAll()//Сериализует пользователей и их данные в файл.
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream fs = new FileStream(@Environment.CurrentDirectory + @"/Data/Users.dat", FileMode.OpenOrCreate))
            {
                bf.Serialize(fs, allUsers);
            }
        }

        public static AllUsers OpenAll()//Десериализует данные пользователей.
        {
            BinaryFormatter bf = new BinaryFormatter();
            AllUsers users;
            using (FileStream fs = new FileStream(@Environment.CurrentDirectory + @"/Data/Users.dat", FileMode.OpenOrCreate))
            {
                users = (AllUsers)bf.Deserialize(fs);
            }
            return users;
        }
    }
}