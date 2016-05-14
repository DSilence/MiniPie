using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace MiniPie
{
    public static class UriProtocolManager
    {
        public const string UrlProtocol = "Software\\Classes\\minipie";
        public static void RegisterUrlProtocol()
        {
            UnregisterUrlProtocol();

            RegistryKey rKey = Registry.CurrentUser.OpenSubKey(UrlProtocol, true);
            if (rKey == null)
            {
                using (rKey = Registry.CurrentUser.CreateSubKey(UrlProtocol))
                {
                    rKey.SetValue("", "URL:MiniPie Protocol");
                    rKey.SetValue("URL Protocol", "");

                    rKey = rKey.CreateSubKey(@"shell\open\command");
                    rKey.SetValue("", "\"" + Process.GetCurrentProcess().MainModule.FileName + "\" %1");
                }
            }
            else
            {
                rKey.Close();
            }
        }

        public static void UnregisterUrlProtocol()
        {
            RegistryKey rKey = Registry.CurrentUser.OpenSubKey(UrlProtocol, true);
            if (rKey != null)
            {
                Registry.CurrentUser.DeleteSubKeyTree(UrlProtocol);
                rKey.Close();
            }
            
        }

    }
}
