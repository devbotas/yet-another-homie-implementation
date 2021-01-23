using System;
using System.Collections.Generic;

namespace DevBot9.Protocols.Homie {
    public class ClientDevice : Device {
        internal ClientDevice(string baseTopic, string id) {
            _baseTopic = baseTopic;
            _deviceId = id;
        }

        public new void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            base.Initialize(publishToTopicDelegate, subscribeToTopicDelegate);

            var homieTopic = $"{_baseTopic}/{_deviceId}/$homie";
            _topicHandlerMap.Add(homieTopic, new List<Action<string>>());
            _topicHandlerMap[homieTopic].Add((value) => {
                // Debug.WriteLine($"{_baseTopic}/{_deviceId}/$homie: {value}");
                HomieVersion = value;
            });
            _subscribeToTopicDelegate(homieTopic);

            var nameTopic = $"{_baseTopic}/{_deviceId}/$name";
            _topicHandlerMap.Add(nameTopic, new List<Action<string>>());
            _topicHandlerMap[nameTopic].Add((value) => {
                //Debug.WriteLine($"{_baseTopic}/{_deviceId}/$name: {value}");
                Name = value;
            });
            _subscribeToTopicDelegate(nameTopic);

            var stateTopic = $"{_baseTopic}/{_deviceId}/$state";
            _topicHandlerMap.Add(stateTopic, new List<Action<string>>());
            _topicHandlerMap[stateTopic].Add((value) => {
                //Debug.WriteLine($"{_baseTopic}/{_deviceId}/$state: {value}");
                State = value;
            });
            _subscribeToTopicDelegate(stateTopic);
        }


        public ClientStringProperty CreateClientStringProperty(PropertyType propertyType, string propertyId) {
            var createdProperty = new ClientStringProperty(propertyType, propertyId);

            _stateProperties.Add(createdProperty);

            return createdProperty;
        }
    }
}
