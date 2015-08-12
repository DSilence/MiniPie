using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace MiniPie
{
    public static class UriProtocolManager
    {
        public const string UrlProtocol = "minipie";
        public static void RegisterUrlProtocol()
        {
            UnregisterUrlProtocol();

            RegistryKey rKey = Registry.ClassesRoot.OpenSubKey(UrlProtocol, true);
            if (rKey == null)
            {
                using (rKey = Registry.ClassesRoot.CreateSubKey(UrlProtocol))
                {
                    rKey.SetValue("", "URL:MiniPie Protocol");
                    rKey.SetValue("URL Protocol", "");

                    rKey = rKey.CreateSubKey(@"shell\open\command");
                    rKey.SetValue("", "\"" + Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MiniPie.exe") + "\" %1");
                }
            }
            else
            {
                rKey.Close();
            }
        }

        public static void UnregisterUrlProtocol()
        {
            RegistryKey rKey = Registry.ClassesRoot.OpenSubKey(UrlProtocol, true);
            if (rKey != null)
            {
                Registry.ClassesRoot.DeleteSubKeyTree(UrlProtocol);
                rKey.Close();
            }
            
        }

    }
}
