using System;

namespace AttributeInjector
{
    /// <summary>
    /// Marks attribute as an injection trigger.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class Injection : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Injection" /> class.
        /// </summary>
        public Injection()
        {
            
        }

    }
}
