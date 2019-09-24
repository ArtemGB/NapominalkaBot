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
        //public static List<UserTask> Tasks = new List<UserTask>();//Удалить потом.

        //private static TimerCallback Save;//Делегат на метод сериализации.
        //private static Timer Saver;//Таймер, сохраняющий данные пользователей.

        public Tasker(IVkApi _vkApi)
        {
            /*  Save = new TimerCallback(SaveAll);
             Saver = new Timer(SaveAll, 0, 0, 30000); */
            vkApi = _vkApi;
            allUsers = OpenAll();
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
            SaveAll();
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
            SaveAll();
        }

        public static void SaveAll()//Сериализует пользователей и их данные в файл.
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream fs = new FileStream(@"Users.dat", FileMode.OpenOrCreate))
                {
                    bf.Serialize(fs, allUsers);
                }
            }
            catch (System.Exception e)
            {
                VKSendMsg(82749439, "Save " + e.Message);
                throw;
            }
        }

        public static AllUsers OpenAll()//Десериализует данные пользователей.
        {
            BinaryFormatter bf = new BinaryFormatter();
            AllUsers users;
            try
            {
                using (FileStream fs = new FileStream(@"Users.dat", FileMode.OpenOrCreate))
                {
                    users = (AllUsers)bf.Deserialize(fs);
                }
                return users;
            }
            catch (System.Exception e)
            {
                string dir = "";
                string[] drr = Directory.GetDirectories(Environment.CurrentDirectory);
                foreach (var dr in drr)
                    dir += dr + "\n";
                VKSendMsg(82749439, "Open " + e.Message + "\n" + dir);
                throw;
            }

        }
    }
}