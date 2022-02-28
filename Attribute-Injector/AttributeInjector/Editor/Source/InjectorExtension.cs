using System;
using System.Reflection;
using AttributeInjector.Editor.AssemblyHolders;
using Mono.Cecil;
using UnityEngine;

namespace AttributeInjector.Editor
{
    public abstract class InjectionComponent : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private ScriptingSymbols mRequiredScriptingSymbols;
        protected static ModuleDefinition ActiveModule { get; private set; }
        
        private void OnEnable()
        {
            mRequiredScriptingSymbols.ValidateSymbols();
        }

        /// <summary>
        /// Invoked whenever we start editing a module. Used to populate our
        /// helper functions 
        /// </summary>
        public void OnBeforeModuleEdited(ModuleDefinition moduleDefinition)
        {
            if (mRequiredScriptingSymbols.isActive)
            {
                ActiveModule = moduleDefinition;
            }
        }

        /// <summary>
        /// Invoked when we have finished editing a module
        /// </summary>
        public virtual void OnModuleEditComplete()
        {
            ActiveModule = null;
        }

        public virtual void VisitModule(ModuleDefinition moduleDefinition) { }
        public virtual void VisitType(TypeDefinition typeDefinition) { }
        public virtual void VisitMethod(MethodDefinition methodDefinition) { }
        public virtual void VisitField(FieldDefinition fieldDefinition) { }
        public virtual void VisitProperty(PropertyDefinition propertyDefinition) { }
        

        #region -= Import Methods =-
        protected static TypeReference Import(TypeReference type) => ActiveModule?.ImportReference(type);
        protected static TypeReference Import(Type type, IGenericParameterProvider context) => ActiveModule?.ImportReference(type, context);
        protected static FieldReference Import(FieldInfo field) => ActiveModule?.ImportReference(field);
        protected static FieldReference Import(FieldInfo field, IGenericParameterProvider context) => ActiveModule?.ImportReference(field, context);
        protected static MethodReference Import(MethodBase method) => ActiveModule?.ImportReference(method);
        protected static MethodReference Import(MethodBase method, IGenericParameterProvider context) => ActiveModule?.ImportReference(method, context);
        protected static TypeReference Import(TypeReference type, IGenericParameterProvider context) => ActiveModule?.ImportReference(type, context);
        protected static TypeReference Import(Type type) => ActiveModule?.ImportReference(type);
        protected static FieldReference Import(FieldReference field) => ActiveModule?.ImportReference(field);
        protected static MethodReference Import(MethodReference method) => ActiveModule?.ImportReference(method);
        protected static MethodReference Import(MethodReference method, IGenericParameterProvider context) => ActiveModule?.ImportReference(method, context);
        protected static FieldReference Import(FieldReference field, IGenericParameterProvider context) => ActiveModule?.ImportReference(field, context);
        #endregion
    }
}
