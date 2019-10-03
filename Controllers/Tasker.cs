using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using Microsoft.AspNetCore.Mvc;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Abstractions;
using VkBot.Users;
using VkBot.Strings;

namespace VkBot.Controllers
{
    ///<summary>
    ///Работа с напоминаниями пользователей.
    ///</summary>
    public class Tasker : Controller
    {
        private static IVkApi vkApi;
        public static AllUsers allUsers; //Хранит все данные пользователей.

        public delegate void TaskDelegat(Message msg);
        public static TaskDelegat TaskProcces; //Переключатель методов выполнения операций с напоминаниями.

        public Tasker(IVkApi _vkApi)
        {
            vkApi = _vkApi;
        }

        /// <summary>
        /// Отправляет пользователю сообщение с просьбой ввести текст напоминания.
        /// </summary>
        public static void StartTaskAdding(Message msg)//Начинает процесс сохранения напоминания.
        {
            allUsers.Users[msg.FromId.Value].IsTaskChangingInProgress = true;
            SaveAll();
            VKSendMsg(msg.PeerId.Value, MsgTexts.TaskAddingFistInstruction);
            TaskProcces = AddTaskText;
        }

        /// <summary>
        /// Сохраняет текст напоминания и просит пользователя ввести время.
        /// </summary>
        public static void AddTaskText(Message msg)//Сохраняет текст напоминания.
        {
            allUsers.Users[msg.FromId.Value].Tasks.Add(new UserTask(msg.Text, new DateTime()));
            SaveAll();
            VKSendMsg(msg.PeerId.Value, MsgTexts.TaskDateAddingInstruction);
            TaskProcces = AddTaskDate;
        }

        /// <summary>
        /// Добавление даты напоминания.
        /// </summary>
        public static void AddTaskDate(Message msg)
        {
            try
            {
                string[] DateAndTime = msg.Text.Trim().Split(' ');//Делим строку с временем на две части: "дд.мм" и "чч:мм".
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
                    if (int.Parse(Date[1]) > 12 || int.Parse(Date[1]) <= 0 || int.Parse(Date[0]) > 31)
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

        /// <summary>
        /// Сообщает об успешном завершении добавления напоминания.
        /// </summary>
        public static void AddTaskComplete(Message msg)
        {
            VKSendMsg(msg.PeerId.Value, "Напоминание добавлено.");
            allUsers.Users[msg.FromId.Value].IsTaskChangingInProgress = false;//Выставляем флаг в 0.
            SaveAll();
        }

        /// <summary>
        /// Показывает список всех напоминаний для пользователя.
        /// </summary>
        public static void ShowTasks(Message msg)
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

        /// <summary>
        /// Отправляет сообщение пользователю.
        /// </summary>
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
        /// <summary>
        /// Очищает список всех напоминаний.
        /// </summary>
        public static void ClearTasks(long FromId)//Очищает список напоминаний.
        {
            allUsers.Users[FromId].Tasks.Clear();
            VKSendMsg(FromId, MsgTexts.ClearTasks);
            SaveAll();
        }

        /// <summary>
        /// Сериализует пользователей и их данные в файл.
        /// </summary>
        public static void SaveAll()
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream fs = new FileStream(@"Data/Users.dat", FileMode.OpenOrCreate))
                {
                    bf.Serialize(fs, allUsers);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Десериализует данные пользователей.
        /// </summary>
        public static AllUsers OpenAll()
        {
            BinaryFormatter bf = new BinaryFormatter();
            AllUsers users;
            try
            {
                using (FileStream fs = new FileStream(@"Data/Users.dat", FileMode.OpenOrCreate))
                {
                    if (fs.Length == 0)
                    {
                        users = new AllUsers();
                    }
                    else users = (AllUsers)bf.Deserialize(fs);
                }
                //Console.WriteLine("Open was succes");
                return users;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}