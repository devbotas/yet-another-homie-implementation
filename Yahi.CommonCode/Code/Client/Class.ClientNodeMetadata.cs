using System.Collections;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// Properties of a Node. Yeah, very useful comment, I know.
    /// </summary>
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
