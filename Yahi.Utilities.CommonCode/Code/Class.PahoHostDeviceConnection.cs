namespace DevBot9.Protocols.Homie.Utilities {
    public class PahoHostDeviceConnection : PahoClientDeviceConnection, IHostDeviceConnection {
        public PahoHostDeviceConnection() {
            _isWillSet = false;
            _isHostDeviceConnector = true;
        }

        public void SetWill(string willTopic, string willMessage) {
            if ((string.IsNullOrEmpty(willTopic) == false) && (string.IsNullOrEmpty(willMessage) == false)) {
                _willTopic = willTopic;
                _willMessage = willMessage;

                _isWillSet = true;
            }
        }
    }
}
