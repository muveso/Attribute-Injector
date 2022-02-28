
namespace AttributeInjector
{
    /// <summary>
    /// Rise argument sources enumeration.
    /// </summary>
    public enum Method : byte
    {
        /// <summary>
        /// Target name.
        /// Should be of type <see cref="string" />.
        /// </summary>
        Name = 1,

        /// <summary>
        /// Target method result.
        /// Should be of type same type of return.
        /// Works only with <see cref="When.OnExit" />.
        /// </summary>
        ReturnValue = 2
    }
}
