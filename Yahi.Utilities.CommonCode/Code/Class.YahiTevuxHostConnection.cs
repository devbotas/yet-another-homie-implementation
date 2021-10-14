#if !NANOFRAMEWORK_1_0
using System;
using System.Text;
using Tevux.Protocols.Mqtt;

namespace DevBot9.Protocols.Homie.Utilities {
    public class YahiTevuxHostConnection : YahiTevuxClientConnection, IHostDeviceConnection {
        public void SetWill(string willTopic, string willPayload) {
            _mqttConnectionOptions.SetWill(willTopic, Encoding.UTF8.GetBytes(willPayload), QosLevel.AtLeastOnce, true);
        }
    }
}
#endif
