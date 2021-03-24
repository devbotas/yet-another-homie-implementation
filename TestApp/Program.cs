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

            var lightbulbProducer = new LightbulbProducer();
            lightbulbProducer.Initialize(brokerIp);

            var homieFecther = new HomieTopicFetcher();
            homieFecther.Initialize(brokerIp);
            homieFecther.FetchTopics(DeviceFactory.BaseTopic + "/#", out var topicDump2);

            var dynamicConsumer = new DynamicConsumer();
            dynamicConsumer.Initialize(brokerIp, topicDump2);

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
