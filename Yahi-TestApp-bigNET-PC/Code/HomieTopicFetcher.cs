﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tevux.Protocols.Mqtt;

namespace TestApp {
    public class HomieTopicFetcher {
        private MqttClient _mqttClient = new MqttClient();
        private Dictionary<string, string> _responses = new();
        private ChannelConnectionOptions _channelConnectionOptions;
        private DateTime _timeOfLastUniqueTopic = DateTime.Now;


        public void Initialize(ChannelConnectionOptions channelOptions) {
            _channelConnectionOptions = channelOptions;
            _mqttClient.Initialize();

            _mqttClient.PublishReceived += HandlePublishReceived;
        }

        public void FetchTopics(string filter, out string[] topics) {
            _responses.Clear();
            _mqttClient.ConnectAndWait(_channelConnectionOptions);
            while (_mqttClient.IsConnected == false) {
                Thread.Sleep(100);
            }

            _mqttClient.Subscribe(filter, QosLevel.AtLeastOnce);

            Thread.Sleep(2000);
            _mqttClient.Unsubscribe(filter);
            _mqttClient.DisconnectAndWait();

            topics = new string[_responses.Count];
            var i = 0;
            foreach (var response in _responses) {
                topics[i] = response.Key + ":" + response.Value;
                i++;
            }

            while (_mqttClient.IsConnected) { Thread.Sleep(100); }
        }

        public void FetchDevices(string baseTopic, out string[] topics) {
            var allTheTopics = new List<string>();

            _mqttClient.ConnectAndWait(_channelConnectionOptions);
            while (_mqttClient.IsConnected == false) {
                Thread.Sleep(100);
            }

            _responses.Clear();
            _mqttClient.SubscribeAndWait($"{baseTopic}/+/$homie", QosLevel.AtLeastOnce);
            Thread.Sleep(500);
            _mqttClient.Unsubscribe($"{baseTopic}/+/$homie");

            Console.WriteLine($"Found {_responses.Count} homie devices.");
            var devices = new List<string>();
            foreach (var deviceTopic in _responses) {
                var deviceName = deviceTopic.Key.Split('/')[1];
                devices.Add(deviceName);
                Console.Write(deviceName + " ");
            }
            Console.WriteLine();

            foreach (var device in devices) {
                _responses.Clear();

                _mqttClient.SubscribeAndWait($"{baseTopic}/{device}/#", QosLevel.AtLeastOnce);
                while ((DateTime.Now - _timeOfLastUniqueTopic).TotalMilliseconds < 500) {
                    Thread.Sleep(100);
                }
                _mqttClient.UnsubscribeAndWait($"{baseTopic}/{device}/#");

                Console.WriteLine($"{_responses.Count} topics for {device}.");
                foreach (var topic in _responses) {
                    allTheTopics.Add(topic.Key + ":" + topic.Value);
                }
            }

            _mqttClient.DisconnectAndWait();

            topics = allTheTopics.ToArray();

            while (_mqttClient.IsConnected) { Thread.Sleep(100); }
        }

        private void HandlePublishReceived(object sender, PublishReceivedEventArgs e) {
            var payload = Encoding.UTF8.GetString(e.Message);

            if (_responses.ContainsKey(e.Topic) == false) {
                _responses.Add(e.Topic, payload);
                _timeOfLastUniqueTopic = DateTime.Now;
            }
            else {
                _responses[e.Topic] = payload;
            }
        }
    }
}
