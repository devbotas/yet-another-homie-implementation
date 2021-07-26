namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A node of a Device.
    /// </summary>
    public class ClientNode {
        /// <summary>
        /// ID of the node.
        /// </summary>
        public string NodeId { get; internal set; } = "";

        /// <summary>
        /// Name of the node, as defined by Homie convention.
        /// </summary>
        public string Name { get; internal set; } = "";

        /// <summary>
        /// Type of the node, as defined by homie convention.
        /// </summary>
        public string Type { get; internal set; } = "";

        /// <summary>
        /// Properties that node contains.
        /// </summary>
        public ClientPropertyBase[] Properties { get; internal set; } = new ClientPropertyBase[0];
    }
}
