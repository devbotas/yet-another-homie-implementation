namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// State defined by the Homie convention.
    /// </summary>
    public class States {
        public static string Init => "init";
        public static string Ready => "ready";
        public static string Disconnected => "disconnected";
        public static string Sleeping => "sleeping";
        public static string Lost => "lost";
        public static string Alert => "alert";
    }
}
