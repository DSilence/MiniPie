using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiniPie.ClickOnceHelpers;
using MiniPie.Core;

namespace Uninstall
{
    static class Program
    {
        private static Mutex instanceMutex;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppContracts contracts = new AppContracts();
            bool createdNew;
            try
            {
                instanceMutex = new Mutex(true, @"Local\" + Assembly.GetExecutingAssembly().GetType().GUID,
                    out createdNew);
                if (!createdNew)
                {
                    instanceMutex = null;
                    return;
                }
                var clickOnceHelper = new ClickOnceHelper(contracts.PublisherName, contracts.ProductName);
                var form = new UninstallForm(contracts);
                Application.EnableVisualStyles();
                Application.Run(form);
                if (form.Uninstall)
                {
                    clickOnceHelper.Uninstall();
                }
            }
            finally
            {
                ReleaseMutex();
            }
        }

        private static void ReleaseMutex()
        {
            if (instanceMutex == null)
                return;
            instanceMutex.ReleaseMutex();
            instanceMutex.Close();
            instanceMutex = null;
        }
    }
}
