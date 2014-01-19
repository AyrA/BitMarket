using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System;
using System.Text;

namespace BitMarket
{
    public static class Base
    {
        public const string DT_FORMAT = "yyyy-MM-dd hh:mm:ss";

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static bool compare(byte[] b1, byte[] b2)
        {
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        public static string AppPath
        {
            get
            {
                string name=Process.GetCurrentProcess().MainModule.FileName;
                return name.Substring(0, name.LastIndexOf('\\'));
            }
        }

        public static NameValueCollection ParseContent(string Content)
        {
            NameValueCollection NVC = new NameValueCollection();
            string[] Parts = Content.Split('\n');

            foreach (string Part in Parts)
            {
                if (Part.Contains("="))
                {
                    NVC.Add(Part.Substring(0, Part.IndexOf('=')).Trim(), Part.Substring(Part.IndexOf('=') + 1).Trim());
                }
            }

            return NVC;
        }

        public static string B64dec(string p)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(p));
        }

        public static string B64enc(string p)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(p));
        }
    }
}
