namespace DevBot9.Protocols.Homie {
    public class ClientStateProperty : ClientPropertyBase {
        internal ClientStateProperty(string propertyId) : base(propertyId) {
            _propertyId = propertyId;
        }

        internal new void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }
    }
}
