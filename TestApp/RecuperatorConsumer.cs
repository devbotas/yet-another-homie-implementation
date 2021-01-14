using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {
    internal class RecuperatorConsumer {
        private MqttClient _mqttClient;
        private string _mqttBrokerIp = "172.16.0.3";
        private string _mqttClientGuid = Guid.NewGuid().ToString();

        private ClientStateProperty _inletTemperature;
        private ClientCommandProperty _selfDestructCommandProperty;
        private ClientDevice _clientDevice;
        private ClientStateProperty _actualPower;

        public RecuperatorConsumer() {
            _mqttClient = new MqttClient(_mqttBrokerIp);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;

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

        public void Initialize() {
            _mqttClient.Connect(_mqttClientGuid);

            _clientDevice.Initialize((topic, value) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value));

            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 2 });
            });

            Task.Run(async () => {
                await Task.Delay(3000);
                _selfDestructCommandProperty.SetValue("5");
            });
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            _clientDevice.HandlePublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message));
        }
    }
}
