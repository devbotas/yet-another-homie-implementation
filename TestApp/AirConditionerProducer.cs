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
        private HostEnumProperty _onOffSwitch;
        private HostEnumProperty _actualState;
        private HostEnumProperty _onOffSwitch2;
        private HostIntegerProperty _ventilationLevel;
        private HostDateTimeProperty _previousServiceDate;
        private HostDateTimeProperty _nextServiceDate;
        private HostBooleanProperty _performServiceCommand;


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
            _hostDevice.UpdateNodeInfo("service", "Service ralated properties", "no-type");

            _actualAirTemperature = _hostDevice.CreateHostFloatProperty(PropertyType.State, "general", "actual-air-temperature", "Actual measured air temperature", "°C");
            _targetAirTemperature = _hostDevice.CreateHostFloatProperty(PropertyType.Parameter, "general", "target-air-temperature", "Target air temperature", "°C");
            _onOffSwitch = _hostDevice.CreateHostEnumProperty(PropertyType.Command, "general", "turn-on-off", "Turn device on or off", new[] { "ON", "OFF" });
            _onOffSwitch.PropertyChanged += (sender, e) => {
                if ((_actualState.Value == "ON") && (_onOffSwitch.Value == "OFF")) {
                    _actualState.Value = "OFF";
                }
                if ((_actualState.Value == "OFF") && (_onOffSwitch.Value == "ON")) {
                    _actualState.Value = "STARTING";
                    Task.Run(async () => {
                        await Task.Delay(3000);
                        _actualState.Value = "ON";
                    });
                }
            };
            _actualState = _hostDevice.CreateHostEnumProperty(PropertyType.State, "general", "actual-state", "Actual power state", new[] { "ON", "OFF", "STARTING" });
            _onOffSwitch2 = _hostDevice.CreateHostEnumProperty(PropertyType.Parameter, "general", "turn-on-off-2", "Turn device on or off 2", new[] { "ON", "OFF" });
            _ventilationLevel = _hostDevice.CreateHostIntegerProperty(PropertyType.Parameter, "ventilation", "level", "Level of ventilation", "%");
            _previousServiceDate = _hostDevice.CreateHostDateTimeProperty(PropertyType.State, "service", "previous-service-date", "Date of the last service");
            _nextServiceDate = _hostDevice.CreateHostDateTimeProperty(PropertyType.State, "service", "next-service-date", "Date for the next service");
            _performServiceCommand = _hostDevice.CreateHostBooleanProperty(PropertyType.Command, "service", "perform-service", "Perform service");
            _performServiceCommand.PropertyChanged += (sender, e) => {
                if (_performServiceCommand.Value == true) {
                    _previousServiceDate.Value = _nextServiceDate.Value;
                    _nextServiceDate.Value = DateTime.Now.AddMinutes(1);
                }
            };

            _hostDevice.Initialize((topic, value, qosLevel, isRetained) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value), 1, true);

            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 1 });
            });

            _actualState.Value = "OFF";
            _targetAirTemperature.Value = 23;
            _ventilationLevel.Value = 50;
            _nextServiceDate.Value = DateTime.Now.AddMonths(3);

            Task.Run(async () => {
                var localTargetTemperature = 0.0f;

                while (true) {
                    _simulatedAirTemperature = _simulatedTransientTargetTemperature + 0.3 * Math.Sin(DateTime.Now.Second);

                    if (_actualState.Value == "ON") {
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
