namespace DevBot9.Protocols.Homie {
    public static class DeviceFactory {
        public static string BaseTopic { get; private set; }

        public static void Initialize(string baseTopic) {
            BaseTopic = baseTopic;
        }

        public static ClientDevice CreateClientDevice(string baseTopic, string deviceId) {
            var returnDevice = new ClientDevice(baseTopic, deviceId);

            return returnDevice;
        }

        public static HostDevice CreateHostDevice(string baseTopic, string deviceId, string friendlyName) {
            var returnDevice = new HostDevice(baseTopic, deviceId, friendlyName);

            return returnDevice;
        }
    }
}
