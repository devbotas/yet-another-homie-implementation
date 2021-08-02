namespace DevBot9.Protocols.Homie {
    // There are no generics in nF (yet), so this is my workaround for Action<string>. Used for callbacks.
    public delegate void ActionStringDelegate(string parameter);
}
