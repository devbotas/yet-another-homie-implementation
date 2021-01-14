namespace DevBot9.Protocols.Homie {
    public class HostCommandProperty : HostPropertyBase {
        internal HostCommandProperty(string propertyId, string friendlyName, DataType dataType, string format, string unit) : base(propertyId, friendlyName, dataType, format, true, false, unit) {
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);

            _parentDevice.InternalPropertySubscribe($"{_propertyId}", (value) => {
                Value = value;
            });
        }
    }
}
