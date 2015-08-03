using System;
using NLog;

namespace MiniPie.Core {
    public sealed class ProductionLogger : ILog {
        private readonly Logger _Logger;

        public ProductionLogger() {
            _Logger = LogManager.GetCurrentClassLogger();
            if (_Logger == null) {
                throw new Exception("Could not initialize NLog");
            }
        }

        public void Info(string message) {
            _Logger.Info(message);
        }

        public void InfoException(string message, Exception exception) {
            _Logger.Info(exception, message);
        }

        public void Warn(string message) {
            _Logger.Warn(message);
        }

        public void WarnException(string message, Exception exception) {
            _Logger.Warn(exception, message);
        }

        public void Fatal(string message) {
            _Logger.Fatal(message);
        }

        public void FatalException(string message, Exception exception) {
            _Logger.Fatal(exception, message);
        }
    }
}
