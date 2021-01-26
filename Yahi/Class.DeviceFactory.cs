namespace DevBot9.Protocols.Homie {
    public static class DeviceFactory {
        public static string BaseTopic { get; private set; }

        public static void Initialize(string baseTopic) {
            BaseTopic = baseTopic;
        }

        public static ClientDevice CreateClientDevice(string deviceId) {
            var returnDevice = new ClientDevice(BaseTopic, deviceId);

            return returnDevice;
        }

        public static HostDevice CreateHostDevice(string deviceId, string friendlyName) {
            var returnDevice = new HostDevice(BaseTopic, deviceId, friendlyName);

            return returnDevice;
        }
    }
}
