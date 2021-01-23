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
        private HostStringProperty _turnOnOff;
        private HostFloatProperty _power;
        private HostIntegerProperty _actualPower;
        private HostStringProperty _actualState;


        public RecuperatorProducer() {
            _mqttClient = new MqttClient(_mqttBrokerIp);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
        }

        public void Initialize() {
            _mqttClient.Connect(_mqttClientGuid);


            _hostDevice = DeviceFactory.CreateHostDevice("temp", "recuperator", "Recuperator");
            _inletTemperature = _hostDevice.CreateHostFloatProperty(PropertyType.State, "inlet-temperature", "Inlet sensor", "°C");
            _actualPower = _hostDevice.CreateHostIntegerProperty(PropertyType.State, "actual-power", "Actual power", "%");
            _turnOnOff = _hostDevice.CreateHostStringProperty(PropertyType.Command, "self-destruct", "On/off switch", "");
            _turnOnOff.PropertyChanged += (sender, e) => {
                Debug.WriteLine($"Beginning self-destruct in {_turnOnOff.Value}");
            };
            _power = _hostDevice.CreateHostFloatProperty(PropertyType.Parameter, "ventilation-power", "Ventilation power", "%");
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

            _actualState = _hostDevice.CreateHostStringProperty(PropertyType.State, "actual-state", "Actual State", "");

            _hostDevice.Initialize((topic, value) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value));

            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 2 });
            });


            Task.Run(async () => {
                var states = new[] { "Good", "Average", "Bad" };

                while (true) {
                    // _inletTemperature.SetValue(new Random().Next(10, 30) - 0.1);
                    _inletTemperature.Value = (float)(new Random().Next(1000, 3000) / 100.0);

                    _actualState.Value = states[new Random().Next(0, 3)];

                    await Task.Delay(1000);
                }
            });
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            _hostDevice.HandlePublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message));
        }
    }
}
