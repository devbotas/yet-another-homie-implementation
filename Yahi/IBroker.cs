using System;

namespace DevBot9.Protocols.Homie {
    public interface IBroker {
        void Subscribe(string topic, Action<string, string> handler);
        void Publish(string topic, string data);
    }
}
