namespace DevBot9.Protocols.Homie;

public class PublishReceivedEventArgs {
    public string Topic { get; private set; }
    public string Payload { get; private set; }

    public PublishReceivedEventArgs(string topic, string payload) {
        Topic = topic;
        Payload = payload;
    }
}
