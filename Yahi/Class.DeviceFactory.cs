namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This static class is used to create Client and Host Devices using a factory patttern.
    /// </summary>
    public static class DeviceFactory {
        /// <summary>
        /// Base topic that will be added by default when creating Devices.
        /// </summary>
        public static string BaseTopic { get; private set; } = "homie";

        /// <summary>
        /// Initializes the device factory.
        /// </summary>
        /// <param name="baseTopic"></param>
        public static void Initialize(string baseTopic = "homie") {
            BaseTopic = baseTopic;
        }

        /// <summary>
        /// Creates a Client Device.
        /// </summary>
        public static ClientDevice CreateClientDevice(string deviceId) {
            var returnDevice = new ClientDevice(BaseTopic, deviceId);

            return returnDevice;
        }

        /// <summary>
        /// Creates a Host Device.
        /// </summary>
        public static HostDevice CreateHostDevice(string deviceId, string friendlyName) {
            var returnDevice = new HostDevice(BaseTopic, deviceId, friendlyName);

            return returnDevice;
        }
    }
}
