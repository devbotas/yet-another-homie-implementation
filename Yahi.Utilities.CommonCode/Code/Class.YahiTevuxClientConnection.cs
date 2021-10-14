﻿#if !NANOFRAMEWORK_1_0
using System;
using System.Text;
using Tevux.Protocols.Mqtt;

namespace DevBot9.Protocols.Homie.Utilities {
    public class YahiTevuxClientConnection : MqttClient, IClientDeviceConnection {
        private ChannelConnectionOptions _channelConnectionOptions = new();
        protected MqttConnectionOptions _mqttConnectionOptions = new();

        public new event PublishReceivedDelegate PublishReceived = delegate { };
        public new EventHandler Connected;
        public new EventHandler Disconnected;

        public void Initialize(ChannelConnectionOptions channelConnectionOptions, AddToLogDelegate loggingFunction = null) {
            Initialize();

            _channelConnectionOptions = channelConnectionOptions;

            base.PublishReceived += (sender, e) => {
                PublishReceived(this, new DevBot9.Protocols.Homie.PublishReceivedEventArgs(e.Topic, Encoding.UTF8.GetString(e.Message)));
            };
        }

        void IBasicDeviceConnection.Connect() {
            Connect(_channelConnectionOptions, _mqttConnectionOptions);
        }

        public void Publish(string topic, string payload, byte qosLevel, bool isRetained) {
            Publish(topic, Encoding.UTF8.GetBytes(payload), (QosLevel)qosLevel, isRetained);
        }

        public void Subscribe(string topic) {
            Subscribe(topic, QosLevel.AtLeastOnce);
        }
    }

    public class YahiTevuxHostConnection : YahiTevuxClientConnection, IHostDeviceConnection {
        public void SetWill(string willTopic, string willPayload) {
            _mqttConnectionOptions.SetWill(willTopic, Encoding.UTF8.GetBytes(willPayload), QosLevel.AtLeastOnce, true);
        }
    }
}
#endif
