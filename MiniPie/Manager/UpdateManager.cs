using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using MiniPie.Core;
using MiniPie.Core.Enums;
using Squirrel;

namespace MiniPie.Manager
{
    public class UpdateManager: IDisposable
    {
        private readonly ILog _log;
#pragma warning disable 649
        private Timer _timer;
#pragma warning restore 649
        private static readonly Dictionary<UpdatePreference, string> UpdateUris = new Dictionary<UpdatePreference, string>
        {
            {UpdatePreference.Developer, "http://minipie.blob.core.windows.net/installerdevelop"},
            {UpdatePreference.Stable, "http://minipie.blob.core.windows.net/installermaster"}
        };

        public UpdateManager(ILog log)
        {
            _log = log;
        }

        public void Initialize()
        {
#if !DEBUG
            _timer = new Timer(Callback, null, TimeSpan.FromSeconds(0.3), TimeSpan.FromMinutes(120));
#endif
        }

        private async void Callback(object state)
        {
            Squirrel.UpdateManager manager = null;
            try
            {
                //workaround since update manager apparently sucks
                manager = new Squirrel.UpdateManager(UpdateUris[UpdatePreference.Developer]);
                await manager.UpdateApp().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.FatalException("Failed to update app " + e.Message, e);
            }
            finally
            {
                if (manager != null)
                {
                    manager.Dispose();
                }
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
    }
}