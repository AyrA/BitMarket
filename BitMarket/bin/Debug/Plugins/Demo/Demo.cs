using GenericHandler;
using System;
using System.IO;
using System.Threading;

namespace Demo
{
    public class Demo : GenericServer
    {
        private Thread T;
        private bool cont;

        public event GenericMessageReceivedHandler GenericMessageReceived;
        public event GenericServerLogHandler GenericServerLog;

        public Demo()
        {
            GenericMessageReceived += new GenericMessageReceivedHandler(DemoServer_GenericMessageReceived);
            GenericServerLog += new GenericServerLogHandler(DemoServer_GenericServerLog);
            T = null;
            cont = false;
            GenericServerLog(this, GenericLogType.Info, false, "Demo server created");
        }

        private void DemoServer_GenericServerLog(object sender, GenericLogType GLT, bool display, string Text)
        {
#if DEBUG
            File.AppendAllText("#DBG#-Demo.log", string.Format("[{0}]\t[{1}]\t{2}\r\n", DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss"), GLT, Text));
#endif
        }

        private void DemoServer_GenericMessageReceived(GenericMessage MSG)
        {
            //NOOP
        }

        public void Start()
        {
            if (T != null)
            {
                Stop();
            }

            GenericServerLog(this, GenericLogType.Info, false, "Starting Demo server");

            T = new Thread(run);
            T.IsBackground = true;
            T.Start();
        }

        public void Config(bool GUI)
        {
            GenericServerLog(this, GenericLogType.Info, false, "This plugin does not needs configuration");
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
            if(MSG.Server != this)
            {
                //defer message removal to owner
                MSG.Server.DeleteMessage(MSG);
            }
            else
            {
                GenericServerLog(this, GenericLogType.Info, false, "This plugin cannot delete messages");
            }
        }

        public void Send(GenericMessage MSG)
        {
            if(MSG.Server != this)
            {
                //defer sending to owner
                MSG.Server.Send(MSG);
            }
            else
            {
                GenericServerLog(this, GenericLogType.Info, false, "This plugin cannot send messages");
            }
        }

        public void Broadcast(GenericMessage MSG)
        {
            Send(MSG);
        }

        public override string ToString()
        {
            return "Demo Server";
        }

        private void run()
        {
            cont = true;
            while (cont)
            {
                //We do not get messages since this is a demo but we create log messages instead
                GenericServerLog(this, GenericLogType.Info, false, "Demo server loop still active");
                
                for (int i = 0; i < 20 && cont; i++)
                {
                    Thread.Sleep(500);
                }
            }
        }
    }
}
