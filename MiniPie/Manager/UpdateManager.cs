using System;
using System.Threading;
using MiniPie.Core;
using MiniPie.Core.Enums;
using Squirrel;

namespace MiniPie.Manager
{
    public class UpdateManager: IDisposable
    {
        private readonly ILog _log;
        private readonly AppSettings _settings;
#pragma warning disable 649
        private Timer _timer;
#pragma warning restore 649
        private const string GithubProjectUrl = "https://github.com/DSilence/MiniPie";

        public UpdateManager(ILog log, AppSettings settings)
        {
            _log = log;
            _settings = settings;
        }

        public void Initialize()
        {
#if RELEASE
            _timer = new Timer(Callback, null, TimeSpan.FromSeconds(0.3), TimeSpan.FromMinutes(60));
#endif
        }

        private async void Callback(object state)
        {
            Squirrel.UpdateManager manager = null;
            try
            {
                if (_settings.UpdatePreference == UpdatePreference.Developer)
                {
                    manager = await Squirrel.UpdateManager.GitHubUpdateManager(GithubProjectUrl, null, null, null, true);
                }
                else
                {
                    manager = await Squirrel.UpdateManager.GitHubUpdateManager(GithubProjectUrl);
                }
                //workaround since update manager apparently sucks
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