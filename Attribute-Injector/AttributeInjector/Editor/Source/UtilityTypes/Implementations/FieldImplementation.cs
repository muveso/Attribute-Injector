
using Mono.Cecil;

namespace AttributeInjector.Editor.UtilityTypes.Implementations
{
    public struct FieldImplementation
    {
        public FieldReference reference;
        public FieldDefinition definition;
        private readonly ModuleDefinition m_Module;

        public FieldImplementation(ModuleDefinition module, FieldDefinition fieldDefinition)
        {
            m_Module = module;
            reference = m_Module.ImportReference(fieldDefinition);
            definition = reference.Resolve();
        }
    }
}
