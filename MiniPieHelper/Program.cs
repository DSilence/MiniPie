using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPieHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg == "registerUri")
                {
                    UriProtocolManager.RegisterUrlProtocol();
                }
                else if (arg == "unregisterUri")
                {
                    UriProtocolManager.UnregisterUrlProtocol();
                }
            }
        }
    }
}
