using System;
using System.Threading;
using DevBot9.Protocols.Homie;
using Tevux.Protocols.Mqtt;
using Tevux.Protocols.Mqtt.Utility;

namespace TestApp {
    internal class Program {

        private static void Main(string[] args) {
            void AddToLog(string severity, string message) {
                Console.WriteLine($"{severity}:{message}");
            }

            var channelOptions = new ChannelConnectionOptions();
            channelOptions.SetHostname("172.16.0.2");


            DeviceFactory.Initialize("homie");

            AddToLog("Info", "");
            AddToLog("Info", "====================================================================");
            AddToLog("Info", "========= Creating AirConditionerConsumer ==========================");
            AddToLog("Info", "====================================================================");
            var airConditionerConsumer = new AirConditionerConsumer();
            airConditionerConsumer.Initialize(channelOptions, (severity, message) => AddToLog(severity, "AirConditionerConsumer:" + message));

            Thread.Sleep(2000);
            AddToLog("Info", "");
            AddToLog("Info", "====================================================================");
            AddToLog("Info", "========= Creating AirConditionerProducer ==========================");
            AddToLog("Info", "====================================================================");
            var airConditionerProducer = new AirConditionerProducer();
            airConditionerProducer.Initialize(channelOptions, (severity, message) => AddToLog(severity, "AirConditionerProducer:" + message));

            Thread.Sleep(2000);
            AddToLog("Info", "");
            AddToLog("Info", "====================================================================");
            AddToLog("Info", "========= Creating LightbulbConsumer ===============================");
            AddToLog("Info", "====================================================================");
            var lightbulbConsumer = new LightbulbConsumer();
            lightbulbConsumer.Initialize(channelOptions, (severity, message) => AddToLog(severity, "LightbulbConsumer:" + message));

            Thread.Sleep(2000);
            AddToLog("Info", "");
            AddToLog("Info", "====================================================================");
            AddToLog("Info", "========= Creating LightbulbProducer ===============================");
            AddToLog("Info", "====================================================================");
            var lightbulbProducer = new LightbulbProducer();
            lightbulbProducer.Initialize(channelOptions, (severity, message) => AddToLog(severity, "LightbulbProducer:" + message));

            Thread.Sleep(2000);
            AddToLog("Info", "");
            AddToLog("Info", "====================================================================");
            AddToLog("Info", "========= Parsing Homie tree for problems ==========================");
            AddToLog("Info", "====================================================================");
            // Note that Eclipse Mosquitto broker can transmit ~100 retained messages by default. Set max_queue_messages to 0 in mosquito.conf to remove this limit,
            // or otherwise parser will have a lot of problems.
            var homieFecther = new HomieTopicFetcher();
            homieFecther.Initialize(channelOptions);
            homieFecther.FetchTopics(DeviceFactory.BaseTopic + "/#", out var topicDump2);
            var homieTree = HomieTopicTreeParser.Parse(topicDump2, DeviceFactory.BaseTopic, out var errorList, out var warningList);
            if (errorList.Length + warningList.Length == 0) {
                AddToLog("Info", "TreeParser:tree looks ok.");
            }
            else {
                foreach (var problem in errorList) {
                    AddToLog("Error", "TreeParser:" + problem);
                }
                foreach (var problem in warningList) {
                    AddToLog("Warning", "TreeParser:" + problem);
                }
            }


            Thread.Sleep(2000);
            AddToLog("Info", "");
            AddToLog("Info", "====================================================================");
            AddToLog("Info", "========= Creating DynamicConsumer =================================");
            AddToLog("Info", "====================================================================");
            if (homieTree.Length == 0) {
                AddToLog("Error", "DynamicConsumer:no devices in the tree :(");
            }
            else {
                var dynamicConsumer = new DynamicConsumer();
                dynamicConsumer.Initialize(channelOptions, homieTree[0], (severity, message) => AddToLog(severity, "DynamicConsumer:" + message));
            }

            AddToLog("Info", "");
            AddToLog("Info", "====================================================================");
            AddToLog("Info", "========= Initialization complete ==================================");
            AddToLog("Info", "====================================================================");
            Console.ReadLine();
        }
    }
}
