using Mono.Cecil;

namespace AttributeInjector.Editor.UtilityTypes.Implementations
{
    public struct MethodImplementation
    {
        public MethodReference reference;
        public MethodDefinition definition;
        private readonly ModuleDefinition m_Module;

        public MethodImplementation(ModuleDefinition module, MethodDefinition methodDefinition)
        {
            m_Module = module;
            reference = m_Module.ImportReference(methodDefinition);
            definition = reference.Resolve();
        }
    }
}