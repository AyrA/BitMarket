using GenericHandler;
using System;
using CookComputing.XmlRpc;
using System.IO;
using System.Threading;

namespace Bitmessage
{
    public class BitmessageServer : GenericServer
    {
        private BitAPI BA;
        private string Addr;
        private Thread T;
        private bool cont;

        public event GenericMessageReceivedHandler GenericMessageReceived;
        public event GenericServerLogHandler GenericServerLog;

        public bool IsRunning
        {
            get
            {
                return T != null;
            }
        }

        public BitmessageServer()
        {
            GenericMessageReceived += new GenericMessageReceivedHandler(BitmessageServer_GenericMessageReceived);
            GenericServerLog += new GenericServerLogHandler(BitmessageServer_GenericServerLog);
            BA = null;
            Addr = null;
            T = null;
            GenericServerLog(this, GenericLogType.Info, false, "Bitmessage Server created");
        }

        void BitmessageServer_GenericServerLog(object sender, GenericLogType GLT, bool display, string Text)
        {
#if DEBUG
            File.AppendAllText("#DBG#-Bitmessage.log", string.Format("[{0}] [{1}] {2}\r\n", DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss"), GLT, Text));
#endif
        }

        void BitmessageServer_GenericMessageReceived(GenericMessage MSG)
        {
            //NOOP
        }

        public void Start()
        {
            if (T != null)
            {
                Stop();
            }

            GenericServerLog(this, GenericLogType.Info, false, "Starting Bitmessage server");

            //Check if API has been set
            if (!QuickSettings.Has("API-ADDR") || !QuickSettings.Has("API-NAME") || !QuickSettings.Has("API-PASS") || !QuickSettings.Has("BM-ADDR"))
            {
                if (!AskAPI())
                {
                    GenericServerLog(this, GenericLogType.Fatal, true, "API Settings for Bitmessage have not been made. Please configure Bitmessage Plugin");
                    throw new Exception("API Settings for Bitmessage have not been made. Please configure Bitmessage Plugin");
                }
            }

            Addr = QuickSettings.Get("BM-ADDR");
            BA = (BitAPI)XmlRpcProxyGen.Create(typeof(BitAPI));
            BA.Url = string.Format("http://{0}/", QuickSettings.Get("API-ADDR"));
            BA.Headers.Add("Authorization", "Basic " + JsonConverter.B64enc(string.Format("{0}:{1}", QuickSettings.Get("API-NAME"), QuickSettings.Get("API-PASS"))));

            try
            {
                if (BA.helloWorld("A", "B") != "A-B")
                {
                    GenericServerLog(this, GenericLogType.Error, true, "API Settings for Bitmessage are wrong. The API seems to answer, but the answer is incorrect. Please check settings");
                    throw new Exception("API Settings for Bitmessage are wrong. The API seems to answer, but the answer is incorrect. Please check settings");
                }
            }
            catch
            {
                GenericServerLog(this, GenericLogType.Error, true, "Cannot contact Bitmessage API. Verify the client is running, has the API enabled and the settings are correct");
                throw new Exception("Cannot contact Bitmessage API. Verify the client is running, has the API enabled and the settings are correct");
            }
            T = new Thread(run);
            T.IsBackground = true;
            T.Start();
        }

        public void Config(bool GUI)
        {
            if (AskAPI())
            {
                GenericServerLog(this, GenericLogType.Info, false, "API settings have changed. Restart the Bitmessage component for changes to take effect");
            }
            else
            {
                GenericServerLog(this, GenericLogType.Warning, true, "API settings have been set to invalid values and are not stored. Please try again");
            }
        }

        public void Stop()
        {
            if (T != null)
            {
                cont = false;
                T.Join(2000);
                if (T.IsAlive)
                {
                    try
                    {
                        T.Abort();
                    }
                    catch
                    {
                    }
                }
                T = null;
            }
        }

        public void DeleteMessage(GenericMessage MSG)
        {
            BA.trashMessage(MSG.Tag.ToString());
        }

        public void Send(GenericMessage MSG)
        {
            //We send Messages always from the "Addr" string variable.

            //check for broadcast receiver
            if (MSG.Receiver.StartsWith("[") && MSG.Receiver.EndsWith("]"))
            {
                Broadcast(MSG);
            }
            else
            {
                BA.sendMessage(MSG.Receiver, Addr, JsonConverter.B64enc(MSG.Command), JsonConverter.B64enc(MSG.RawContent));
                GenericServerLog(this, GenericLogType.Info, false, string.Format("Sending Message from {0} to {1}", Addr, MSG.Receiver));
            }
        }

        public void Broadcast(GenericMessage MSG)
        {
            BA.sendBroadcast(Addr, JsonConverter.B64enc(MSG.Command), JsonConverter.B64enc(MSG.RawContent));
            GenericServerLog(this, GenericLogType.Info, false, string.Format("Sending Broadcast from {0}", Addr));
        }

        public override string ToString()
        {
            return "PyBitmessage API Server";
        }

        private void run()
        {
            cont = true;
            while (cont)
            {
                //loop messages
                BitMsg[] MSGs = JsonConverter.getMessages(BA.getAllInboxMessages());

                foreach (BitMsg M in MSGs)
                {
                    if (M.toAddress == Addr)
                    {
                        GenericMessageReceived(new GenericMessage() { Server = this, Tag = M.msgid, Sender = M.fromAddress, Receiver = M.toAddress, RawContent = M.message, Command = M.subject });
                    }
                    else
                    {
                        GenericServerLog(this, GenericLogType.Debug, false, "Ignoring message to " + M.toAddress);
                    }
                }
                for (int i = 0; i < 20 && cont; i++)
                {
                    Thread.Sleep(500);
                }
            }
        }

        private bool AskAPI()
        {
            ushort i = 0;
            Console.WriteLine("Before we can begin, we need to set up the API params for bitmessage.");
            Console.WriteLine("You need to enable the API first, before you continue here.");
            Console.WriteLine("See this Article for help: https://bitmessage.org/wiki/API");
            Console.WriteLine();
            Console.WriteLine("Enter Bitmessage API Settings below:");
            Console.Write("IP Address: ");
            string IP = Console.ReadLine();
            Console.Write("Port: ");
            string Port = Console.ReadLine();
            Console.Write("Username: ");
            string UN = Console.ReadLine();
            Console.Write("Password: ");
            string PW = Console.ReadLine();
            Console.Write("Shop address: ");
            Addr = Console.ReadLine();

            //Verify values quick and dirty
            if (IP.Length > 0 && Port.Length > 0 && UN.Length > 0 && PW.Length > 0 && ushort.TryParse(Port, out i) && Addr.Length > 0)
            {
                QuickSettings.Set("API-ADDR", IP + ":" + Port);
                QuickSettings.Set("API-NAME", UN);
                QuickSettings.Set("API-PASS", PW);
                QuickSettings.Set("BM-ADDR", Addr);
                return true;
            }
            return false;
        }
    }
}
