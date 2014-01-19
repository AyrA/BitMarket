using System;
using System.Threading;
using System.IO;
using System.Text;
using GenericHandler;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BitMarket
{
    class Program
    {
        private static bool cont;

        static int Main(string[] args)
        {
            cont = true;

            loadPlugins();

            if (AddonManager.Servers!=null && AddonManager.Processors!=null &&
                AddonManager.Servers.Count > 0 && AddonManager.Processors.Count>0)
            {
                //Everything OK, start market

                //Bring up processors
                for (int i = 0; i < AddonManager.Processors.Count; i++)
                {
                    AddonManager.Processors[i].GenericServerLog += new GenericServerLogHandler(serverMessage);
                    AddonManager.Processors[i].Start();
                }
                //Bring up servers
                for (int i = 0; i < AddonManager.Servers.Count; i++)
                {
                    AddonManager.Servers[i].GenericMessageReceived += new GenericMessageReceivedHandler(processMessage);
                    AddonManager.Servers[i].GenericServerLog += new GenericServerLogHandler(serverMessage);
                    AddonManager.Servers[i].Start();
                }

                while (cont)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.Escape:
                            cont = false;
                            break;
                    }
                }
                Console.WriteLine("Closing Market...");

                //stop servers
                for (int i = 0; i < AddonManager.Servers.Count; i++)
                {
                    AddonManager.Servers[i].Stop();
                }

                //stop processors
                for (int i = 0; i < AddonManager.Processors.Count; i++)
                {
                    AddonManager.Processors[i].Stop();
                }

                return 0;
            }
            else
            {
                Console.WriteLine("You need at least 1 server and 1 message processor!");
                Console.ReadKey(true);
                return 1;
            }
        }

        /// <summary>
        /// Loads all existing Plugins
        /// </summary>
        private static void loadPlugins()
        {
            string Dir = Base.AppPath+"\\Plugins";
            if (Directory.Exists(Dir))
            {
                foreach (string D in Directory.GetDirectories(Dir))
                {
                    if (File.Exists(Path.Combine(D, "info.txt")))
                    {
                        loadModule(D);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(Dir);
            }
        }

        /// <summary>
        /// Loads Plugins (Servers and Processors) from a Directory
        /// </summary>
        /// <param name="D">Plugin directory with info.txt file in it</param>
        private static void loadModule(string D)
        {
            NameValueCollection NVC = Base.ParseContent(File.ReadAllText(Path.Combine(D, "info.txt")));

            if (!string.IsNullOrEmpty(NVC["FILE"]) && !string.IsNullOrEmpty(NVC["TYPE"]))
            {
                if (Contains(NVC["TYPE"].ToLower().Split(','), "genericprocessor"))
                {
                    if (NVC["FILE"].Contains("."))
                    {
                        try
                        {
                            switch (NVC["FILE"].Substring(NVC["FILE"].LastIndexOf('.') + 1).ToUpper())
                            {
                                case "CS":
                                    AddonManager.LoadProcessor(Path.Combine(D, NVC["FILE"]));
                                    break;
                                case "DLL":
                                    AddonManager.LoadProcessorLib(Path.Combine(D, NVC["FILE"]));
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG(string.Format("Cannot load server module {0}", NVC["NAME"]));
                            LOG(string.Format(ex.Message));
                            if (AddonManager.LastErrors != null)
                            {
                                foreach (var e in AddonManager.LastErrors)
                                {
                                    LOG(e.ToString());
                                }
                                AddonManager.LastErrors = null;
                            }
                        }
                    }
                }
            }

            //Load Servers
            if (!string.IsNullOrEmpty(NVC["FILE"]) && !string.IsNullOrEmpty(NVC["TYPE"]))
            {
                if (Contains(NVC["TYPE"].ToLower().Split(','), "genericserver"))
                {
                    if (NVC["FILE"].Contains("."))
                    {
                        try
                        {
                            switch (NVC["FILE"].Substring(NVC["FILE"].LastIndexOf('.') + 1).ToUpper())
                            {
                                case "CS":
                                    AddonManager.LoadServer(Path.Combine(D, NVC["FILE"]));
                                    break;
                                case "DLL":
                                    AddonManager.LoadServerLib(Path.Combine(D, NVC["FILE"]));
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG(string.Format("Cannot load server module {0}", NVC["NAME"]));
                            LOG(string.Format(ex.Message));
                            if (AddonManager.LastErrors != null)
                            {
                                foreach (var e in AddonManager.LastErrors)
                                {
                                    LOG(e.ToString());
                                }
                                AddonManager.LastErrors = null;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// checks if a string array contains a specific value
        /// </summary>
        /// <remarks>only for loadModule, not case sensitive</remarks>
        /// <param name="arr">array</param>
        /// <param name="p">value to look for</param>
        /// <returns>true if found</returns>
        private static bool Contains(string[] arr, string p)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Trim() == p.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Executed when a Server or Processor Message arrives
        /// </summary>
        /// <param name="sender">sender of message</param>
        /// <param name="GLT">Log type</param>
        /// <param name="display">force it to be displayed in a message box</param>
        /// <param name="Text">Text to show</param>
        static void serverMessage(object sender, GenericLogType GLT, bool display, string Text)
        {
#if !DEBUG
            if (GLT != GenericLogType.Debug)
            {
#endif
                Console.WriteLine("{3}:\t[{0}]\t[{1}]\t{2}", DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss"), GLT, Text, sender);
#if !DEBUG
            }
#endif
        }

        /// <summary>
        /// Called upon an incoming message. Message is removed after processing
        /// </summary>
        /// <param name="M">GenericMessage</param>
        private static void processMessage(GenericMessage M)
        {
            foreach (GenericProcessor GP in AddonManager.Processors)
            {
                if (M != null)
                {
                    M = GP.Process(M);
                }
            }
            if (M != null)
            {
                M.Server.DeleteMessage(M);
            }
        }

        /// <summary>
        /// Logs a message with a timestamp to the console
        /// </summary>
        /// <param name="p">message (without timestamp)</param>
        internal static void LOG(string p)
        {
            Console.WriteLine("[{1}] {0}",p,DateTime.Now.ToString(Base.DT_FORMAT));
        }
    }
}
