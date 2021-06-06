namespace DevBot9.Protocols.Homie {
    public class ClientNode {
        public string Name { get; internal set; } = "";
        public string Type { get; internal set; } = "";
        public ClientPropertyBase[] Properties { get; internal set; }
    }
}
