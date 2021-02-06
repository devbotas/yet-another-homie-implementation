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

        private ClientDevice _clientDevice;
        private ClientFloatProperty _inletTemperature;
        private ClientStringProperty _selfDestructCommandProperty;
        private ClientIntegerProperty _actualPower;
        private ClientStringProperty _actualState;

        public RecuperatorConsumer() {
            _mqttClient = new MqttClient(_mqttBrokerIp);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;

            _clientDevice = DeviceFactory.CreateClientDevice("recuperator");

            _inletTemperature = _clientDevice.CreateClientFloatProperty(PropertyType.State, "ventilation", "inlet-temperature");
            _inletTemperature.PropertyChanged += HandleInletTemperaturePropertyChanged;
            _actualPower = _clientDevice.CreateClientIntegerProperty(PropertyType.State, "general", "actual-power");
            _actualPower.PropertyChanged += (sender, e) => {
                Debug.WriteLine($"Actual power changed to: {_actualPower.Value}");
            };
            _selfDestructCommandProperty = _clientDevice.CreateClientStringProperty(PropertyType.Command, "general", "self -destruct");

            _actualState = _clientDevice.CreateClientStringProperty(PropertyType.State, "general", "actual-state");
            _actualState.PropertyChanged += (sender, e) => {
                Debug.WriteLine($"Actual state: {_actualState.Value}");
            };
        }

        private void HandleInletTemperaturePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(_inletTemperature.Value)) {
                Debug.WriteLine($"{e.PropertyName}: {_inletTemperature.Value}");
            }
        }

        public void Initialize() {
            _mqttClient.Connect(_mqttClientGuid);

            _clientDevice.Initialize((topic, value, qosLevel, isRetained) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value), qosLevel, isRetained);
            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 1 });
            });

            Task.Run(async () => {
                await Task.Delay(3000);
                _selfDestructCommandProperty.Value = "5";
            });
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            _clientDevice.HandlePublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message));
        }
    }
}
