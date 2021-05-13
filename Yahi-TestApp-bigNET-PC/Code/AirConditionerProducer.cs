using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using uPLibrary.Networking.M2Mqtt;

namespace TestApp {
    internal class AirConditionerProducer {
        private MqttClient _mqttClient;
        private string _mqttClientGuid = Guid.NewGuid().ToString();

        private HostDevice _hostDevice;
        private HostFloatProperty _targetAirTemperature;
        private HostFloatProperty _actualAirTemperature;
        private HostEnumProperty _onOffSwitch;
        private HostEnumProperty _actualState;
        private HostIntegerProperty _ventilationLevel;
        private HostDateTimeProperty _previousServiceDate;
        private HostDateTimeProperty _nextServiceDate;
        private HostBooleanProperty _performServiceCommand;

        public AirConditionerProducer() { }

        public void Initialize(string mqttBrokerIpAddress) {
            _hostDevice = DeviceFactory.CreateHostDevice("air-conditioner", "Air conditioning unit");

            _mqttClient = new MqttClient(mqttBrokerIpAddress);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
            _mqttClient.Connect(_mqttClientGuid, "", "", true, 1, true, _hostDevice.WillTopic, _hostDevice.WillPayload, true, 10);


            #region General node

            _hostDevice.UpdateNodeInfo("general", "General information and properties", "no-type");

            // Temperatures. These are pretty self-explanatory, right?
            _actualAirTemperature = _hostDevice.CreateHostFloatProperty(PropertyType.State, "general", "actual-air-temperature", "Actual measured air temperature", 18, "°C");
            _targetAirTemperature = _hostDevice.CreateHostFloatProperty(PropertyType.Parameter, "general", "target-air-temperature", "Target air temperature", 23, "°C");

            // Besides obvious ON and OFF states, there's a transient STARTING state. This simulated the non-instant startup of the device.
            _actualState = _hostDevice.CreateHostEnumProperty(PropertyType.State, "general", "actual-state", "Actual power state", new[] { "ON", "OFF", "STARTING" }, "OFF");

            // Creating a switch. It also simulates startup sequence ON -> STARTING -> OFF. Shutdown sequence is instant ON -> OFF.
            _onOffSwitch = _hostDevice.CreateHostEnumProperty(PropertyType.Command, "general", "turn-on-off", "Turn device on or off", new[] { "ON", "OFF" });
            _onOffSwitch.PropertyChanged += (sender, e) => {
                if ((_actualState.Value == "ON") && (_onOffSwitch.Value == "OFF")) {
                    // This is the shutdown sequence.
                    _actualState.Value = "OFF";
                }
                if ((_actualState.Value == "OFF") && (_onOffSwitch.Value == "ON")) {
                    // This is the startup sequence.
                    _actualState.Value = "STARTING";
                    Task.Run(async () => {
                        await Task.Delay(3000);
                        _actualState.Value = "ON";
                    });
                }
            };


            #endregion


            #region Ventilation node

            _hostDevice.UpdateNodeInfo("ventilation", "Ventilation information and properties", "no-type");

            // Allows user to set ventilation level (or power). This will actually be used in simulation loop.
            _ventilationLevel = _hostDevice.CreateHostIntegerProperty(PropertyType.Parameter, "ventilation", "level", "Level of ventilation", 50, "%");

            #endregion


            #region Service node

            _hostDevice.UpdateNodeInfo("service", "Service related properties", "no-type");

            // Previous and next service dates. These are read-only properties. They are changed by "perform service" command.
            _previousServiceDate = _hostDevice.CreateHostDateTimeProperty(PropertyType.State, "service", "previous-service-date", "Date of the last service", DateTime.Now.AddMonths(3));
            _nextServiceDate = _hostDevice.CreateHostDateTimeProperty(PropertyType.State, "service", "next-service-date", "Date for the next service", DateTime.Now);

            // This is a write-only property, that is — a command. It sets last service date to actual datetime, and also sets next service date to some time in the future.
            // Popular automation software have lots of problems with this kind of workflow, when there's a command that doesn't really have a state register.
            // As of 2021-03-12, openHAB and HomeAssistant and HoDD can't really deal with it. Which is a bummer, as it a pretty common worklfow in real time! 
            _performServiceCommand = _hostDevice.CreateHostBooleanProperty(PropertyType.Command, "service", "perform-service", "Perform service");
            _performServiceCommand.PropertyChanged += (sender, e) => {
                if (_performServiceCommand.Value == true) {
                    _previousServiceDate.Value = _nextServiceDate.Value;
                    _nextServiceDate.Value = DateTime.Now.AddMinutes(1);
                }
            };

            #endregion

            // This builds topic trees and subscribes to everything.
            _hostDevice.Initialize((topic, value, qosLevel, isRetained) => {
                _mqttClient.Publish(topic, Encoding.UTF8.GetBytes(value), 1, true);

            }, topic => {
                _mqttClient.Subscribe(new string[] { topic }, new byte[] { 1 });
            });


            // finally, running the simulation loop. We're good to go!
            Task.Run(async () => await RunSimulationLoopContinuously(new CancellationToken()));
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            _hostDevice.HandlePublishReceived(e.Topic, Encoding.UTF8.GetString(e.Message));
        }

        private async Task RunSimulationLoopContinuously(CancellationToken cancellationToken) {
            var _simulatedTransientTargetTemperature = 23.0;

            while (cancellationToken.IsCancellationRequested == false) {
                var _simulatedAirTemperature = _simulatedTransientTargetTemperature + 0.3 * Math.Sin(DateTime.Now.Second);

                float localTargetTemperature;
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
        }
    }
}
