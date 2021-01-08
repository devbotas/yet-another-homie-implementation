using System;

namespace DevBot.Homie {
    public interface IBroker {
        void Subscribe(string topic, Action<string, string> handler);
        void Publish(string topic, string data);
    }
}
