using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {
    internal class RecuperatorProducer {
        private MqttClient _mqttClient;
        private string _mqttBrokerIp = "172.16.0.3";
        private string _mqttClientGuid = Guid.NewGuid().ToString();


        private HostDevice _hostDevice;
        private HostFloatProperty _inletTemperature;
        private HostBooleanProperty _turnOnOff;
        private HostFloatProperty _power;
        private HostIntegerProperty _actualPower;
        private HostEnumProperty _actualState;


        public RecuperatorProducer() {
            _mqttClient = new MqttClient(_mqttBrokerIp);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
        }

        public void Initialize() {
            _mqttClient.Connect(_mqttClientGuid);


            _hostDevice = DeviceFactory.CreateHostDevice("recuperator", "Recuperator");

            _hostDevice.UpdateNodeInfo("general", "General information", "no-type");
            _hostDevice.UpdateNodeInfo("ventilation", "Ventilation related properties", "no-type");


            _inletTemperature = _hostDevice.CreateHostFloatProperty(PropertyType.State, "ventilation", "inlet-temperature", "Inlet sensor", "°C");
            _actualPower = _hostDevice.CreateHostIntegerProperty(PropertyType.State, "general", "actual-power", "Actual power", "%");
            _turnOnOff = _hostDevice.CreateHostBooleanProperty(PropertyType.Command, "general", "self-destruct", "On/off switch");
            _turnOnOff.PropertyChanged += (sender, e) => {
                Debug.WriteLine($"Beginning self-destruct in {_turnOnOff.Value}");
            };
            _power = _hostDevice.CreateHostFloatProperty(PropertyType.Parameter, "ventilation", "ventilation-power", "Ventilation power", "%");
            _power.PropertyChanged += (sender, e) => {
                Debug.WriteLine($"Ventilation power set to {_power.Value}");
                Task.Run(async () => {
                    _actualPower.Value = 10;
                    await Task.Delay(1000);
                    _actualPower.Value = 20;
                    await Task.Delay(1000);
                    _actualPower.Value = 30;
                });
            };

            _actualState = _hostDevice.CreateHostEnumProperty(PropertyType.State, "general", "actual-state", "Actual State", new string[] { "Good", "Average", "Bad" });

            _hostDevice.Initialize((topic, value, qosLevel, isRetained) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value), qosLevel, isRetained);

            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 1 });
            });


            Task.Run(async () => {
                var states = new[] { "Good", "Average", "Bad" };

                while (true) {
                    _inletTemperature.Value = (float)(new Random().Next(1000, 3000) / 100.0);

                    _actualState.Value = states[new Random().Next(0, 3)];

                    await Task.Delay(1000);
                }
            });

            Console.WriteLine("Full list of producer's published topics:");
            foreach (var topic in _hostDevice.GetAllPublishedTopics()) {
                Console.WriteLine(topic);
            }
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            _hostDevice.HandlePublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message));
        }
    }
}
