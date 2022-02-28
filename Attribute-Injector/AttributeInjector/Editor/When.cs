namespace AttributeInjector
{
    /// <summary>
    /// Rise method injection points enumeration.
    /// </summary>
    public enum When : byte
    {
        /// <summary>
        /// Rise method is called before target method body.
        /// </summary>
        OnEntry = 1,

        /// <summary>
        /// Rise method is called after target method body.
        /// </summary>
        OnExit = 2
    }
}
