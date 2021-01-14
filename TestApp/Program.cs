using System;
using DevBot9.Protocols.Homie;

namespace TestApp {

    internal class Program {

        private static void Main(string[] args) {
            DeviceFactory.Initialize("homie-test");

            var recuperatorConsumer = new RecuperatorConsumer();
            recuperatorConsumer.Initialize();

            var recuperatorProducer = new RecuperatorProducer();
            recuperatorProducer.Initialize();

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
