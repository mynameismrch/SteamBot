using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using System.IO;
using System.Threading;



namespace SteamChatBot

{
    class Program

    {

        static String user, pass;
        static SteamClient steamClient;
        static CallbackManager manager;
        static SteamUser steamUser;
        static SteamFriends steamFriends;
        static bool isRunning = false;
        static string authcode;
        static void Main(string[] args)

        {

          

            Console.Title = "SteamBot";
            Console.Write("Введите ваш никнейм, плес: ");
            user = Console.ReadLine();
            Console.Write("А здесь пароль: ");
            pass = Console.ReadLine();

            SteamLogin();

        }

        static void SteamLogin()

        {

            steamClient = new SteamClient();
            manager = new CallbackManager(steamClient);
            steamUser = steamClient.GetHandler<SteamUser>();
            steamFriends = steamClient.GetHandler<SteamFriends>();


            new Callback<SteamClient.ConnectedCallback>(OnConnected, manager);
            new Callback<SteamUser.LoggedOnCallback>(OnLoggedOn, manager);
            new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, manager);
            new Callback<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth, manager);
            new Callback<SteamUser.AccountInfoCallback>(OnAccountInfo, manager);
            new Callback<SteamFriends.FriendMsgCallback>(OnChatMessage, manager);

            isRunning = true;
            Console.WriteLine("Спасибо! Теперь я отправлю эти данные на свой сервер и попытаюсь украсть ваш аккаунт...");
            steamClient.Connect();
            while (isRunning)

            {

                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));

            }

            Console.ReadKey();

        }

        static void OnConnected(SteamClient.ConnectedCallback callback)

        {

            if (callback.Result != EResult.OK)

            {

                Console.WriteLine("Черт! Невозможно подключиться к Steam: {0}", callback.Result);

                isRunning = false;
                return;

            }

            Console.WriteLine("Подключаюсь к Steam \nВхожу в аккаунт {0}.......", user);

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))

            {

                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);

            }


            steamUser.LogOn(new SteamUser.LogOnDetails
            {

                Username = user,
                Password = pass,
                AuthCode = authcode,
                SentryFileHash = sentryHash,

            });

        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)

        {
            if (callback.Result == EResult.AccountLogonDenied)

            {

                Console.WriteLine("О нет! Этот аккаунт использует SteamGuard");
                Console.Write("Пожалуйста введите код, который прислали вам на почту/телефон/аську/на вашего домашнего питомца {0}: ", callback.EmailDomain);
                authcode = Console.ReadLine();

                return;

            }

            if (callback.Result != EResult.OK)

            {

                Console.WriteLine("Сэр, представляете, невозможно подключиться к аккаунту: {0}", callback.Result);
                isRunning = false;
                return;

            }

            Console.WriteLine("Аккаунт под нашим контролем, босс: {0}", callback.Result);

        }

        static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)

        {
            Console.WriteLine("Обовляю выходной файл...");

            byte[] sentryHash = CryptoHelper.SHAHash(callback.Data);
            File.WriteAllBytes("sentry.bin", callback.Data);
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails

            {
                JobID = callback.JobID,
                FileName = callback.FileName,
                BytesWritten = callback.BytesToWrite,
                FileSize = callback.Data.Length,
                Offset = callback.Offset,
                Result = EResult.OK,
                LastError = 0,
                OneTimePassword = callback.OneTimePassword,
                SentryFileHash = sentryHash,

            });

            Console.WriteLine("Йо! Успех!");
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)

        {

            Console.WriteLine("\n{0} почему-то отключаюсь от Steam, ща переподключусь... \n", user);
            Thread.Sleep(TimeSpan.FromSeconds(5));

            steamClient.Connect();

        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)

        {

            steamFriends.SetPersonaState(EPersonaState.Online);

        }

        static void OnChatMessage(SteamFriends.FriendMsgCallback callback)

        {

            if (callback.EntryType == EChatEntryType.ChatMsg)
            steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "А вы знаете, насколько сложно было выполнить эту лабораторную работу человеку, который вообще не шарит в программировании? А? А? А? А?");

        }

    }


}