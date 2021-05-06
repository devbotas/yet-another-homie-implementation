namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// Possible data type formats of the Homie property.
    /// </summary>
    public enum DataType {
        Blank = -1,
        String = 0,
        Integer,
        Float,
        Percent,
        Boolean,
        Enum,
        Color,
        DateTime,
        Duration
    }
}
