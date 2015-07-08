using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiniPie.Core;

namespace Uninstall
{
    public partial class UninstallForm : Form
    {
        private readonly string _settingsLocation;

        private AppSettings settings;
        public UninstallForm(AppContracts contracts)
        {
            InitializeComponent();

            InfoLabel.Text = Properties.Resources.RemoveMessage;
            PreserveCache.Text = Properties.Resources.KeepCache;
            PreserveSettings.Text = Properties.Resources.KeepSettings;

            Continue.Text = Properties.Resources.Continue;
            Cancel.Text = Properties.Resources.Cancel;

            _settingsLocation = contracts.SettingsLocation;
            var _settingsPersistor = new JsonPersister<AppSettings>(_settingsLocation);
            settings = _settingsPersistor.Instance;
        }

        private void Continue_Click(object sender, EventArgs e)
        {
            if (!this.PreserveCache.Checked)
            {
                string baseCacheFolder = string.IsNullOrEmpty(settings.CacheFolder)
                    ? Directory.GetCurrentDirectory()
                    : settings.CacheFolder;
                DirectoryInfo cacheDirectory = new DirectoryInfo(Path.Combine(baseCacheFolder, "CoverCache"));
                if (cacheDirectory.Exists)
                {
                    cacheDirectory.Delete(true);
                }
            }
            if (!this.PreserveCache.Checked)
            {
                DirectoryInfo settingsLocation = new DirectoryInfo(_settingsLocation);
                if (settingsLocation.Exists)
                {
                    settingsLocation.Delete(true);
                }
            }
            Uninstall = true;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public bool Uninstall = false;
    }
}
