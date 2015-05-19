using System;
using System.ComponentModel;
using System.IO;
using System.Timers;
using Newtonsoft.Json;

namespace MiniPie.Core {
    public sealed class JsonPersister<T> : IDisposable where T : class, INotifyPropertyChanged {
        
        private T _instance;
        //private readonly Timer _Persistor;
        private readonly object _syncLock;

        public JsonPersister(string path) {
            Path = path;
            _syncLock = new object();
            //_Persistor = new Timer(new TimeSpan(0, 0, 1, 0).TotalMilliseconds) {AutoReset = true, Enabled = true};
            //_Persistor.Elapsed += (o, e) => Persist();
        }

        public string Path { get; private set; }

        public T Instance {
            get {
                try {
                    return _instance ?? CreateInstance();
                }
                catch {
                    return (_instance = Activator.CreateInstance<T>());
                }
            }
        }

        private T CreateInstance()
        {
            if (_instance != null)
            {
                _instance.PropertyChanged -= OnInstanceOnPropertyChanged;
            }
            T instance = _instance = File.Exists(Path)
                                                         ? JsonConvert.DeserializeObject<T>(File.ReadAllText(Path))
                                                         : Activator.CreateInstance<T>();
            if (instance != null)
            {
                instance.PropertyChanged += OnInstanceOnPropertyChanged;
            }
            return instance;
        }

        private void OnInstanceOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            Persist();
        }

        public void Persist() {
            lock (_syncLock)
            {
				File.WriteAllText(Path, JsonConvert.SerializeObject(_instance));
            }
        }

        public void Dispose() {
            if (_instance != null)
            {
                _instance.PropertyChanged -= OnInstanceOnPropertyChanged;
            }
            //_Persistor.Dispose();
            Persist();
        }
    }
}