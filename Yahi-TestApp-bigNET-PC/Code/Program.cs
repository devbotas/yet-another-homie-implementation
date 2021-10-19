using System;
using System.Threading;
using DevBot9.Protocols.Homie;
using NLog;
using Tevux.Protocols.Mqtt;

namespace TestApp {
    internal class Program {

        private static void Main(string[] args) {
            // Configure NLog.
            var config = new NLog.Config.LoggingConfiguration();
            var logconsole = new NLog.Targets.ColoredConsoleTarget("console");
            var logdebug = new NLog.Targets.DebuggerTarget("debugger");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logdebug);
            LogManager.Configuration = config;

            ILogger _log = LogManager.GetCurrentClassLogger();

            var channelOptions = new ChannelConnectionOptions();
            channelOptions.SetHostname("172.16.0.2");

            DeviceFactory.Initialize("homie");

            _log.Info("");
            _log.Info("====================================================================");
            _log.Info("========= Creating AirConditionerConsumer ==========================");
            _log.Info("====================================================================");
            var airConditionerConsumer = new AirConditionerConsumer();
            airConditionerConsumer.Initialize(channelOptions);

            Thread.Sleep(2000);
            _log.Info("");
            _log.Info("====================================================================");
            _log.Info("========= Creating AirConditionerProducer ==========================");
            _log.Info("====================================================================");
            var airConditionerProducer = new AirConditionerProducer();
            airConditionerProducer.Initialize(channelOptions);

            Thread.Sleep(2000);
            _log.Info("");
            _log.Info("====================================================================");
            _log.Info("========= Creating LightbulbConsumer ===============================");
            _log.Info("====================================================================");
            var lightbulbConsumer = new LightbulbConsumer();
            lightbulbConsumer.Initialize(channelOptions);

            Thread.Sleep(2000);
            _log.Info("");
            _log.Info("====================================================================");
            _log.Info("========= Creating LightbulbProducer ===============================");
            _log.Info("====================================================================");
            var lightbulbProducer = new LightbulbProducer();
            lightbulbProducer.Initialize(channelOptions);

            Thread.Sleep(2000);
            _log.Info("");
            _log.Info("====================================================================");
            _log.Info("========= Parsing Homie tree for problems ==========================");
            _log.Info("====================================================================");
            // Note that Eclipse Mosquitto broker can transmit ~100 retained messages by default. Set max_queue_messages to 0 in mosquito.conf to remove this limit,
            // or otherwise parser will have a lot of problems.
            var homieFecther = new HomieTopicFetcher();
            homieFecther.Initialize(channelOptions);
            homieFecther.FetchDevices(DeviceFactory.BaseTopic, out var topicDump2);
            var homieTree = HomieTopicTreeParser.Parse(topicDump2, DeviceFactory.BaseTopic, out var errorList, out var warningList);
            if (errorList.Length + warningList.Length == 0) {
                _log.Info("TreeParser:tree looks ok.");
            }
            else {
                foreach (var problem in errorList) {
                    _log.Error("TreeParser:" + problem);
                }
                foreach (var problem in warningList) {
                    _log.Warn("TreeParser:" + problem);
                }
            }


            Thread.Sleep(2000);
            _log.Info("");
            _log.Info("====================================================================");
            _log.Info("========= Creating DynamicConsumer =================================");
            _log.Info("====================================================================");
            if (homieTree.Length == 0) {
                _log.Error("DynamicConsumer:no devices in the tree :(");
            }
            else {
                var dynamicConsumer = new DynamicConsumer();
                dynamicConsumer.Initialize(channelOptions, homieTree[0]);
            }

            _log.Info("");
            _log.Info("====================================================================");
            _log.Info("========= Initialization complete ==================================");
            _log.Info("====================================================================");
            Console.ReadLine();
        }
    }
}
