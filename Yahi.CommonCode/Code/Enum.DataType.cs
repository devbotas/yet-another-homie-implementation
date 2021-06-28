namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// Possible data type formats of the Homie property.
    /// </summary>
    public enum DataType {
        Blank = -1, // <-- Blank is useful when creating client properties manually.
        String = 0,
        Integer,
        Float,
        // Percent, // <-- I'm starting to think that this is not a type actually. Confusing even if it is, so removing. We have percent in the units field, no need for a dedicated data type.
        Boolean = 4,
        Enum,
        Color,
        DateTime,
        Duration
    }
}
