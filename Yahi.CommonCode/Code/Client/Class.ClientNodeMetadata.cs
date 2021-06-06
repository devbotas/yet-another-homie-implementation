using System.Collections;

namespace DevBot9.Protocols.Homie {
    public class ClientNodeMetadata {
        public string Id { get; internal set; } = "";
        public string NameAttribute { get; internal set; } = "";
        public string TypeAttribute { get; internal set; } = "";
        public ClientPropertyMetadata[] Properties { get; internal set; }
        public Hashtable AllAttributes { get; internal set; } = new Hashtable();
        public override string ToString() {
            return Id;
        }
    }
}
