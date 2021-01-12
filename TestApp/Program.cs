using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {

    internal class Program {

        private static void Main(string[] args) {
            DeviceFactory.Initialize("homie-test");

            var mySimpleBroker = new MySimpleBroker();

            var recuperatorConsumer = new RecuperatorConsumer();
            recuperatorConsumer.Initialize(mySimpleBroker);


            var recuperatorProducer = new RecuperatorProducer();
            recuperatorProducer.Initialize(mySimpleBroker);

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }

    internal class RecuperatorConsumer {
        private IBroker _broker;
        private ClientStateProperty _inletTemperature;
        private ClientCommandProperty _selfDestructCommandProperty;
        private ClientDevice _clientDevice;
        private ClientStateProperty _actualPower;

        public RecuperatorConsumer() {
            _clientDevice = DeviceFactory.CreateClientDevice("temp", "recuperator");

            _inletTemperature = _clientDevice.CreateClientStateProperty("inlet-temperature");
            _inletTemperature.PropertyChanged += HandleInletTemperaturePropertyChanged;
            _actualPower = _clientDevice.CreateClientStateProperty("actual-power");
            _actualPower.PropertyChanged += (sender, e) => {
                Debug.WriteLine($"Actual power changed to: {_actualPower.Value}");
            };
            _selfDestructCommandProperty = _clientDevice.CreateClientCommandProperty("self-destruct");

        }

        private void HandleInletTemperaturePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            Debug.WriteLine($"{e.PropertyName}: {_inletTemperature.Value}");
        }

        public void Initialize(IBroker broker) {
            _broker = broker;

            _clientDevice.Initialize(_broker);

            Task.Run(async () => {
                await Task.Delay(3000);
                _selfDestructCommandProperty.SetValue("5");
            });
        }
    }

    internal class RecuperatorProducer {
        private HostDevice _device;
        private HostStateProperty _inletTemperature;
        private HostCommandProperty _turnOnOff;
        private HostParameterProperty _power;
        private HostStateProperty _actualPower;

        private IBroker _client;

        public RecuperatorProducer() {
        }

        public void Initialize(IBroker client) {
            _client = client;

            _device = DeviceFactory.CreateHostDevice("temp", "recuperator", "Recuperator");
            _inletTemperature = _device.CreateHostStateProperty("inlet-temperature", "Inlet sensor", DataType.Float, "°C");
            _actualPower = _device.CreateHostStateProperty("actual-power", "Actual power", DataType.String, "%");
            _turnOnOff = _device.CreateHostCommandProperty("self-destruct", "On/off switch", DataType.String, "");
            _turnOnOff.PropertyChanged += (sender, e) => {
                Debug.WriteLine($"Beginning self-destruct in {_turnOnOff.Value}");
            };
            _power = _device.CreateHostParameterProperty("ventilation-power", "Ventilation power", DataType.String, "%");
            _power.PropertyChanged += (sender, e) => {
                Debug.WriteLine($"Ventilation power set to {_power.Value}");
                Task.Run(async () => {
                    _actualPower.SetValue("10");
                    await Task.Delay(1000);
                    _actualPower.SetValue("20");
                    await Task.Delay(1000);
                    _actualPower.SetValue("30");

                });
            };

            _device.Initialize(_client);

            Task.Run(async () => {
                while (true) {
                    _inletTemperature.SetValue(new Random().Next(10, 30).ToString("F2"));
                    await Task.Delay(1000);
                }
            });

        }
    }

    internal class MySimpleBroker : IBroker {
        private static MqttClient _mqttClient;
        private static string _mqttBrokerIp = "172.16.0.3";
        private static string _mqttClientGuid = "814600E9-2EA7-4A29-9B62-D3F9B32F5F9C";

        private Dictionary<string, List<Action<string, string>>> _topicHandlerMap = new Dictionary<string, List<Action<string, string>>>();

        public MySimpleBroker() {
            _mqttClient = new MqttClient(_mqttBrokerIp);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
            _mqttClient.Connect(_mqttClientGuid);
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            if (_topicHandlerMap.ContainsKey(e.Topic)) {
                foreach (var handler in _topicHandlerMap[e.Topic]) {
                    handler(e.Topic, Encoding.UTF8.GetString(e.Message));
                }
            }
        }

        public void Publish(string topic, string data) {
            _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(data));
        }

        public void Subscribe(string topic, Action<string, string> handler) {
            if (_topicHandlerMap.ContainsKey(topic) == false) {
                _topicHandlerMap.Add(topic, new List<Action<string, string>>());
            }

            _topicHandlerMap[topic].Add(handler);

            _mqttClient.Subscribe(new[] { topic }, new byte[] { 0 });
        }
    }
}
