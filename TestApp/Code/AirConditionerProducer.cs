using System;
using System.Threading;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;
using Tevux.Protocols.Mqtt;

namespace TestApp {
    internal class AirConditionerProducer {
        private readonly NLog.ILogger _log = NLog.LogManager.GetCurrentClassLogger();
        private readonly YahiTevuxHostConnection _broker = new YahiTevuxHostConnection();

        private HostDevice _hostDevice;
        private HostNumberProperty _targetAirTemperature;
        private HostNumberProperty _actualAirTemperature;
        private HostChoiceProperty _onOffSwitch;
        private HostChoiceProperty _actualState;
        private HostNumberProperty _ventilationLevel;
        private HostDateTimeProperty _previousServiceDate;
        private HostDateTimeProperty _nextServiceDate;
        private HostChoiceProperty _performServiceCommand;
        private HostNumberProperty _systemUptime;

        public AirConditionerProducer() { }

        public void Initialize(ChannelConnectionOptions channelOptions) {
            _hostDevice = DeviceFactory.CreateHostDevice("air-conditioner", "Air conditioning unit");

            #region General node

            _hostDevice.UpdateNodeInfo("general", "General information and properties", "no-type");

            // Temperatures. These are pretty self-explanatory, right?
            _actualAirTemperature = _hostDevice.CreateHostNumberProperty(PropertyType.State, "general", "actual-air-temperature", "Actual measured air temperature", 18, "°C");
            _targetAirTemperature = _hostDevice.CreateHostNumberProperty(PropertyType.Parameter, "general", "target-air-temperature", "Target air temperature", 23, "°C");

            // Besides obvious ON and OFF states, there's a transient STARTING state. This simulated the non-instant startup of the device.
            _actualState = _hostDevice.CreateHostChoiceProperty(PropertyType.State, "general", "actual-state", "Actual power state", new[] { "ON", "OFF", "STARTING" }, "OFF");

            // Creating a switch. It also simulates startup sequence ON -> STARTING -> OFF. Shutdown sequence is instant ON -> OFF.
            _onOffSwitch = _hostDevice.CreateHostChoiceProperty(PropertyType.Command, "general", "turn-on-off", "Turn device on or off", new[] { "ON", "OFF" });
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
                _log.Info($"Actual state changed to: {_actualState.Value}");
            };


            #endregion

            #region Ventilation node

            _hostDevice.UpdateNodeInfo("ventilation", "Ventilation information and properties", "no-type");

            // Allows user to set ventilation level (or power). This will actually be used in simulation loop.
            _ventilationLevel = _hostDevice.CreateHostNumberProperty(PropertyType.Parameter, "ventilation", "level", "Level of ventilation", 50, "%");

            #endregion

            #region Service node

            _hostDevice.UpdateNodeInfo("service", "Service related properties", "no-type");

            // Previous and next service dates. These are read-only properties. They are changed by "perform service" command.
            _previousServiceDate = _hostDevice.CreateHostDateTimeProperty(PropertyType.State, "service", "previous-service-date", "Date of the last service", DateTime.Now.AddMonths(3));
            _nextServiceDate = _hostDevice.CreateHostDateTimeProperty(PropertyType.State, "service", "next-service-date", "Date for the next service", DateTime.Now);

            // This is a write-only property, that is — a command. It sets last service date to actual datetime, and also sets next service date to some time in the future.
            // Popular automation software have lots of problems with this kind of workflow, when there's a command that doesn't really have a state register.
            // As of 2021-03-12, openHAB and HomeAssistant and HoDD can't really deal with it. Which is a bummer, as it a pretty common workflow in real life! 
            _performServiceCommand = _hostDevice.CreateHostChoiceProperty(PropertyType.Command, "service", "perform-service", "Perform service", new[] { "STOP", "START" });
            _performServiceCommand.PropertyChanged += (sender, e) => {
                if (_performServiceCommand.Value == "START") {
                    _previousServiceDate.Value = _nextServiceDate.Value;
                    _nextServiceDate.Value = DateTime.Now.AddMinutes(1);
                }
            };

            // Knowing system uptime is always useful.
            _systemUptime = _hostDevice.CreateHostNumberProperty(PropertyType.State, "service", "system-uptime", "Uptime", 0, "h", 3);

            #endregion

            // This builds topic trees and subscribes to everything.
            _broker.Initialize(channelOptions);
            _hostDevice.Initialize(_broker);

            // Finally, running the simulation loop. We're good to go!
            Task.Run(async () => await RunSimulationLoopContinuously(new CancellationToken()));
        }

        private async Task RunSimulationLoopContinuously(CancellationToken cancellationToken) {
            var simulatedTransientTargetTemperature = 23.0;
            var startTime = DateTime.Now;

            while (cancellationToken.IsCancellationRequested == false) {
                var simulatedAirTemperature = simulatedTransientTargetTemperature + 0.3 * Math.Sin(DateTime.Now.Second);

                double localTargetTemperature;
                if (_actualState.Value == "ON") {
                    localTargetTemperature = _targetAirTemperature.Value;
                }
                else {
                    localTargetTemperature = 25;
                }

                if (localTargetTemperature - simulatedTransientTargetTemperature > 0.1) {
                    simulatedTransientTargetTemperature += 0.1 * _ventilationLevel.Value / 100;
                }
                if (localTargetTemperature - simulatedTransientTargetTemperature < -0.1) {
                    simulatedTransientTargetTemperature -= 0.1 * _ventilationLevel.Value / 100;
                }

                await Task.Delay(1000);

                _actualAirTemperature.Value = simulatedAirTemperature;

                _systemUptime.Value = (DateTime.Now - startTime).TotalHours;
            }
        }
    }
}
