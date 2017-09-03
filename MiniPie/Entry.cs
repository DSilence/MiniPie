using System;
using MiniPie.Manager;

namespace MiniPie
{
    public static class Entry
    {
        [STAThread]
        public static void Main(string[] args)
        {
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(args);
        }
    }
}
