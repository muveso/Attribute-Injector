using System;

namespace AttributeInjector
{
    /// <summary>
    /// Defines method as a special advice method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class Rise : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rise" /> class.
        /// </summary>
        /// <param name="when">Specifies when advice method should be called.</param>
        public Rise(When when)
        {
            When = when;
        }

        /// <summary>
        /// When of advice
        /// </summary>
        public When When { get; }
    }
}