namespace DevBot9.Protocols.Homie {
    public class ClientCommandProperty : ClientPropertyBase {
        public ClientCommandProperty(string propertyId) : base(propertyId) {
            _propertyId = propertyId;
        }

        internal new void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        public void SetValue(string valueToSet) {
            _parentDevice.InternalPropertyPublish($"{_propertyId}", valueToSet);
        }
    }
}
