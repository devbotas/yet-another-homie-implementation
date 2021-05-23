using System;
using DevBot9.Protocols.Homie;

namespace TestApp {
    internal class Program {

        private static void Main(string[] args) {
            var brokerIp = "172.16.0.2";

            DeviceFactory.Initialize("homie");

            var recuperatorConsumer = new AirConditionerConsumer();
            recuperatorConsumer.Initialize(brokerIp);

            var airConditionerProducer = new AirConditionerProducer();
            airConditionerProducer.Initialize(brokerIp);

            var lightbulbConsumer = new LightbulbConsumer();
            lightbulbConsumer.Initialize(brokerIp);

            var lightbulbProducer = new LightbulbProducer();
            lightbulbProducer.Initialize(brokerIp);

            // Note that Eclipse Mosquitto broker can transmit ~100 retained messages by default. Set max_queue_messages to 0 in mosquito.conf to remove this limit,
            // or otherwise parser will have a lot of problems.
            var homieFecther = new HomieTopicFetcher();
            homieFecther.Initialize(brokerIp);
            homieFecther.FetchTopics(DeviceFactory.BaseTopic + "/#", out var topicDump2);

            //var dynamicConsumer = new DynamicConsumer();
            //dynamicConsumer.Initialize(brokerIp, topicDump2);

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
