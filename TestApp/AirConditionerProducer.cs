using System;
using System.Text;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {
    internal class AirConditionerProducer {
        private MqttClient _mqttClient;
        private string _mqttBrokerIp = "172.16.0.3";
        private string _mqttClientGuid = Guid.NewGuid().ToString();

        private HostDevice _hostDevice;
        private HostFloatProperty _targetAirTemperature;
        private HostFloatProperty _actualAirTemperature;
        private HostBooleanProperty _onOffSwitch;
        private HostIntegerProperty _ventilationLevel;

        private double _simulatedAirTemperature = 20;
        private double _simulatedTransientTargetTemperature = 21;

        public AirConditionerProducer() {
            _mqttClient = new MqttClient(_mqttBrokerIp);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
        }

        public void Initialize() {
            _mqttClient.Connect(_mqttClientGuid);

            _hostDevice = DeviceFactory.CreateHostDevice("air-conditioner", "Air conditioning unit");

            _hostDevice.UpdateNodeInfo("general", "General information and properties", "no-type");
            _hostDevice.UpdateNodeInfo("ventilation", "Ventilation information and properties", "no-type");

            _actualAirTemperature = _hostDevice.CreateHostFloatProperty(PropertyType.State, "general", "actual-air-temperature", "Actual measured air temperature", "°C");
            _targetAirTemperature = _hostDevice.CreateHostFloatProperty(PropertyType.Parameter, "general", "target-air-temperature", "Target air temperature", "°C");
            _onOffSwitch = _hostDevice.CreateHostBooleanProperty(PropertyType.Command, "general", "turn-on-off", "Turn device on or off");
            _ventilationLevel = _hostDevice.CreateHostIntegerProperty(PropertyType.Parameter, "ventilation", "level", "Level of ventilation", "%");

            _hostDevice.Initialize((topic, value, qosLevel, isRetained) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value), 1, true);

            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 1 });
            });

            _targetAirTemperature.Value = 23;
            _ventilationLevel.Value = 50;

            Task.Run(async () => {
                var localTargetTemperature = 0.0f;

                while (true) {
                    _simulatedAirTemperature = _simulatedTransientTargetTemperature + 0.3 * Math.Sin(DateTime.Now.Second);

                    if (_onOffSwitch.Value == true) {
                        localTargetTemperature = _targetAirTemperature.Value;
                    }
                    else {
                        localTargetTemperature = 25;
                    }

                    if (localTargetTemperature - _simulatedTransientTargetTemperature > 0.1) {
                        _simulatedTransientTargetTemperature += 0.1 * _ventilationLevel.Value / 100;
                    }
                    if (localTargetTemperature - _simulatedTransientTargetTemperature < -0.1) {
                        _simulatedTransientTargetTemperature -= 0.1 * _ventilationLevel.Value / 100;
                    }

                    await Task.Delay(1000);

                    _actualAirTemperature.Value = (float)_simulatedAirTemperature;
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
