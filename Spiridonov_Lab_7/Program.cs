﻿using Newtonsoft.Json;
using System;
using Telegram.Bot;
using System.Globalization;


namespace Spiridonov_Lab_7
{
    class Program
    {
        static ITelegramBotClient botClient;
        static void Main(string[] args)
        {
            string TOKEN = "1477099741:AAGKyYi6B67EhyEA2Ib-_HE70W1hUMkF9JQ";

            botClient = new TelegramBotClient(TOKEN);

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name {me.FirstName}.");

            botClient.OnMessage += BotClient_OnMessage;
            botClient.StartReceiving();
            Console.ReadKey();

        }

        public interface IAPI
        {
            public string image { get; }
            public string type { get; }
        }

        public class Dog : IAPI
        {
            static string API_URL = "https://dog.ceo/api/breeds/image/random";
            private class JsonSchema
            {
                public string message { get; set; }
                public string status { get; set; }
            }

            public Dog()
            {
                var webClient = new System.Net.WebClient();
                var json = webClient.DownloadString(API_URL);
                webClient.Dispose();

                var data = JsonConvert.DeserializeObject<JsonSchema>(json);
                image = data.message;
            }

            public string image { get; }
            public string type
            {
                get
                {
                    return "Doggy!!!";
                }
            }
        }

        public class Cat : IAPI
        {
            static string API_URL = "https://api.thecatapi.com/v1/images/search";
            private class JsonSchema
            {
                public string id { get; set; }
                public string url { get; set; }
                public int width { get; set; }
                public int height { get; set; }

                [JsonIgnore]
                public string[] breeds { get; set; }
            }

            public Cat()
            {
                var webClient = new System.Net.WebClient();
                var json = webClient.DownloadString(API_URL);
                webClient.Dispose();

                var data = JsonConvert.DeserializeObject<JsonSchema[]>(json)[0];
                image = data.url;
            }
            public string image { get; }
            public string type
            {
                get
                {
                    return "Kitty!!!";
                }
            }
        }




        private static void BotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            bool needReverse = false;
            bool needUpper = false;
            var reverseCommand = "*reverse*";
            var upperCommand = "*upper*";
            var command = e.Message.Text;
            string timeInParis = (DateTime.UtcNow.AddHours(1).ToString("HH:mm:ss"));
            string todayString = DateTime.Today.ToLongDateString();

            var todayDayOfWeek = DateTime.Today.DayOfWeek;
            var todayDayOfWeekValue = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(todayDayOfWeek);

            string todayMessage = ($"{todayString} {todayDayOfWeekValue}");
            string welcomeMessage = "Привет, это тренировочный бот для лабороторной работы 7!\n" +
                "- Для получения фотографии котика введите Cat или cat\n" +
                "- Для получения фотографии песика введите Dog или dog\n" +
                "- Для получения развернутого текста впишите в начале или конце Вашего сообщения *reverse*\n" +
                "- Для получения сообщения в верхнем регистре введите *upper*\n" +
                "- Для получения времени в Париже введите *Time in Paris*\n" +
                "- Для получения сегодняшней даты введите *Today*";

            string reverseLetter = "";
            if (command.StartsWith(reverseCommand))
            {
                char[] sReverse = command.Remove(0, 10).ToCharArray();
                Array.Reverse(sReverse);
                reverseLetter = new string(sReverse);
                needReverse = true;
            }
            else if (command.EndsWith(reverseCommand))
            {
                char[] sReverse = command.ToCharArray();
                Array.Reverse(sReverse);
                reverseLetter = new string(sReverse);
                reverseLetter = reverseLetter.Remove(0, 10);
                needReverse = true;
            }

            string upperLetter = "";
            if (command.StartsWith(upperCommand))
            {
                upperLetter = e.Message.Text.Remove(0, 8).ToUpper();
                needUpper = true;
            }
            else if (command.EndsWith(upperCommand))
            {
                upperLetter = e.Message.Text.ToUpper();
                upperLetter = upperLetter.TrimEnd('*', 'U', 'P', 'E', 'R');
                needUpper = true;
            }

            if (command == "/start")
            {
                botClient.SendTextMessageAsync(e.Message.Chat, welcomeMessage);
            }
            else if (command != null)
            {

                IAPI animal;
                switch (command)
                {
                    case "cat":
                    case "Cat":
                        animal = new Cat();
                        break;
                    case "dog":
                    case "Dog":
                        animal = new Dog();
                        break;
                    default:
                        if (needReverse == true)
                        {
                            botClient.SendTextMessageAsync(e.Message.Chat, reverseLetter);
                        }
                        else if (needUpper == true)
                        {
                            botClient.SendTextMessageAsync(e.Message.Chat, upperLetter);
                        }
                        else if (command == "*Time in Paris*")
                        {
                            botClient.SendTextMessageAsync(e.Message.Chat, timeInParis);
                        }
                        else if (command == "*Today*")
                        {

                            botClient.SendTextMessageAsync(e.Message.Chat, todayMessage);
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(e.Message.Chat, "Непонятный запрос");
                        }
                        return;

                }
                botClient.SendPhotoAsync(e.Message.Chat, animal.image, animal.type);

            }
        }
    }
}
