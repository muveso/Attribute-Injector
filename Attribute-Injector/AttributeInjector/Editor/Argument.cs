using System;

namespace AttributeInjector
{
    /// <summary>
    /// Defines method parameter as a special advice parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class Argument : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Argument" /> class.
        /// </summary>
        /// <param name="method">Specifies method of advice argument.</param>
        public Argument(Method method)
        {
            Method = method;
        }

        /// <summary>
        /// Rise argument method used to populate method parameter.
        /// </summary>
        public Method Method { get; }
    }
}
