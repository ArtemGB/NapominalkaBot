using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Abstractions;
using VkNet.Model.RequestParams;
using System;
using VkBot.Users;
using VkBot.Strings;

namespace VkBot.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration configuration;
        private static IVkApi vkApi;
        private static Tasker tasker;


        public CallbackController(IVkApi _vkApi, IConfiguration configuration)
        {
            this.configuration = configuration;
            vkApi = _vkApi;
            tasker = new Tasker(vkApi);
            Tasker.allUsers = Tasker.OpenAll(); //Открывем файл с данными пользователей.
        }

        /// <summary>
        /// Принимает входящие сообщения и обрабатывает их.
        /// </summary>
        public IActionResult Callback([FromBody] Updates updates)
        {
            // Тип события
            switch (updates.Type)
            {
                // Ключ-подтверждение
                case "confirmation":
                    {
                        return Ok(configuration["Config:Confirmation"]);
                    }
                // Новое сообщение
                case "message_new":
                    {
                        var msg = Message.FromJson(new VkResponse(updates.Object));
                        //Добавление нового пользователя.
                        if (!Tasker.allUsers.Users.ContainsKey(msg.FromId.Value))
                        {
                            Tasker.allUsers.Users.Add(msg.FromId.Value, new VkUser(msg.FromId.Value));
                            Tasker.SaveAll();
                            VKSendMsg(msg.PeerId.Value, MsgTexts.HelloNewUser);
                        }
                        else
                            MsgReceiver(msg);
                        break;
                    }
            }
            return Ok("ok");
        }

        /// <summary>
        /// Даёт ответ, когда сервер вызывает сам себя.
        /// </summary>
        public string GetAnswer()
        {
            return "Answer.";
        }

        /// <summary>
        /// Отправка сообщения пользователю.
        /// </summary>
        private IActionResult VKSendMsg(long _PeerId, string MsgText)
        {
            vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = DateTime.Now.Millisecond + new Random().Next(),
                PeerId = _PeerId,
                Message = MsgText
            });
            return new OkObjectResult("ok");
        }

        /// <summary>
        /// Обработчик входящих сообщений.
        /// </summary>
        private void MsgReceiver(Message msg)
        {
            string mess = msg.Text.ToLower(); //Переводим всё в нижний регистр.
            mess = mess.Replace(".", "").Replace(",", "").Replace(")", "").Replace("(", "").Replace("?", ""); //Убираем лишние символы.
            //Отменяем выполнение операции, если в процессе её выполнения пользователь написал слово "отмена".
            if (mess == "отмена" && Tasker.allUsers.Users[msg.FromId.Value].IsTaskChangingInProgress == true)
            {
                Tasker.allUsers.Users[msg.FromId.Value].IsTaskChangingInProgress = false;
                Tasker.SaveAll();
                VKSendMsg(msg.PeerId.Value, MsgTexts.Cancel);
                return;
            }
            //Если выполняется какая-либо операция, то продолжаем её выполнять.
            if (Tasker.allUsers.Users[msg.FromId.Value].IsTaskChangingInProgress == true)
            {
                Tasker.TaskProcces(msg);
                return;
            }
            //Выбираем нужное действие в зависимости от того, что написал пользователь.
            switch (mess)
            {
                case "напомни":
                    {
                        Tasker.StartTaskAdding(msg);
                        break;
                    }
                case "покажи":
                    {
                        Tasker.ShowTasks(msg);
                        break;
                    }
                case "очистить":
                    {
                        Tasker.ClearTasks(msg.FromId.Value);
                        break;
                    }
                case "пользователи":
                    {
                        if (Tasker.allUsers.Users.Count > 0)
                        {
                            string users = "Пользователи:\n";
                            foreach (var user in Tasker.allUsers.Users)
                                users += user.Value.VkId + "\n";
                            VKSendMsg(msg.PeerId.Value, users);
                        }
                        else VKSendMsg(msg.PeerId.Value, MsgTexts.NoUsers);
                        break;
                    }
                case "id"://Для тестов.
                    {
                        VKSendMsg(msg.PeerId.Value, msg.FromId.Value.ToString());
                        break;
                    }
                case "спасибо":
                    {
                        VKSendMsg(msg.PeerId.Value, MsgTexts.Thanks);
                        break;
                    }
                default:
                    {
                        string Answer;
                        try
                        {
                            Answer = MsgTexts.Answers[mess];
                        }
                        catch (System.Exception)
                        {
                            Answer = MsgTexts.DontUnderstand;
                        }
                        VKSendMsg(msg.PeerId.Value, Answer);
                        break;
                    }
            }
        }
    }
}