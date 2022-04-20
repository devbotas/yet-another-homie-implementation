using System.Collections.Generic;

namespace DevBot9.Protocols.Homie;

/// <summary>
/// Properties of a Node. Yeah, very useful comment, I know.
/// </summary>
public class ClientNodeMetadata {
    public string Id { get; internal set; } = "";
    public string NameAttribute { get; internal set; } = "";
    public string TypeAttribute { get; internal set; } = "";
    public ClientPropertyMetadata[] Properties { get; internal set; }
    public Dictionary<string, string> AllAttributes { get; internal set; } = new();
    public override string ToString() {
        return Id;
    }
}
