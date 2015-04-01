using System;
using System.IO;
using System.Timers;
using Newtonsoft.Json;

namespace MiniPie.Core {
    public sealed class JsonPersister<T> : IDisposable where T : class {
        
        private T _Instance;
        private readonly Timer _Persistor;
        private readonly object _SyncLock;

        public JsonPersister(string path) {
            Path = path;
            _SyncLock = new object();
            _Persistor = new Timer(new TimeSpan(0, 0, 1, 0).TotalMilliseconds) {AutoReset = true, Enabled = true};
            _Persistor.Elapsed += (o, e) => Persist();
        }

        public string Path { get; private set; }

        public T Instance {
            get {
                try {
                    return _Instance ?? (_Instance = File.Exists(Path)
                                                         ? JsonConvert.DeserializeObject<T>(File.ReadAllText(Path))
                                                         : Activator.CreateInstance<T>());
                }
                catch {
                    return (_Instance = Activator.CreateInstance<T>());
                }
            }
        }

        public void Persist() {
            lock (_SyncLock)
            {
				File.WriteAllText(Path, JsonConvert.SerializeObject(_Instance));
            }
        }

        public void Dispose() {
            _Persistor.Dispose();
            Persist();
        }
    }
}