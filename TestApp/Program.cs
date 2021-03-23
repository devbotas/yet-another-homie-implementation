using System;
using DevBot9.Protocols.Homie;

namespace TestApp {
    internal class Program {

        private static void Main(string[] args) {
            var brokerIp = "172.16.0.2";

            DeviceFactory.Initialize("homie");

            var recuperatorConsumer = new RecuperatorConsumer();
            recuperatorConsumer.Initialize();

            var airConditionerProducer = new AirConditionerProducer();
            airConditionerProducer.Initialize(brokerIp);

            var lightbulbProducer = new LightbulbProducer();
            lightbulbProducer.Initialize(brokerIp);

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
