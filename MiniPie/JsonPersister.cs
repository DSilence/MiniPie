using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace MiniPie {
    public class JsonPersister<T> : IDisposable where T : class {
        
        private T _instance;
        private readonly object _syncLock;

        /// <summary>
        /// For unit tests
        /// </summary>
        public JsonPersister()
        {

        }
        public JsonPersister(string path) {
            Path = path;
            _syncLock = new object();
        }

        public string Path { get; private set; }

        public virtual T Instance {
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
            T instance = _instance = File.Exists(Path)
                                                         ? JsonConvert.DeserializeObject<T>(File.ReadAllText(Path))
                                                         : Activator.CreateInstance<T>();
            return instance;
        }

        public void Persist() {
            lock (_syncLock)
            {
                File.WriteAllText(Path, JsonConvert.SerializeObject(_instance, Formatting.Indented));
            }
        }

        public void Dispose() {
            Persist();
        }
    }
}