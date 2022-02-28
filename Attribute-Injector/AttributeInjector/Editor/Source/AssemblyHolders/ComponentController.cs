using AttributeInjector.Editor.UtilityTypes;
using Mono.Cecil;
using Mono.Collections.Generic;
using UnityEngine;

namespace AttributeInjector.Editor.AssemblyHolders
{
    [System.Serializable]
    public class ComponentController: SubObjectController<InjectionComponent>
    {
        /// <summary>
        /// Sets up the component controller. 
        /// </summary>
        public void Initialize(Object owner)
        {
            mOwner = owner;
        }
        
        /// <summary>
        /// Takes in a module and invokes <see cref="InjectionComponent.VisitModule(ModuleDefinition)"/> 
        /// on all components. 
        /// </summary>
        public void VisitModule(ModuleDefinition moduleCollection)
        {
            // Loop over all sub objects
            for (var componentIndex = mSubObjects.Count - 1; componentIndex >= 0; componentIndex--)
            {
                // Assign our type system
                mSubObjects[componentIndex].OnBeforeModuleEdited(moduleCollection);
                mSubObjects[componentIndex].VisitModule(moduleCollection);
            }
            // Visit Types
            VisitTypes(moduleCollection.Types);
            // Loop over all components and invoke our on complete event
            for (var componentIndex = mSubObjects.Count - 1; componentIndex >= 0; componentIndex--)
                mSubObjects[componentIndex].OnModuleEditComplete();
        }

        /// <summary>
        /// Takes in a collection of types and invokes <see cref="InjectionComponent.VisitType(TypeDefinition)"/> 
        /// on all components. 
        /// </summary>
        protected void VisitTypes(Collection<TypeDefinition> typeCollection)
        {
            // We only don't have to visit types if nobody visits properties, methods, or fields. 
            for (var typeIndex = typeCollection.Count - 1; typeIndex >= 0; typeIndex--)
            {
                for (var componentIndex = mSubObjects.Count - 1; componentIndex >= 0; componentIndex--)
                {
                    mSubObjects[componentIndex].VisitType(typeCollection[typeIndex]);
                }
                // visit Methods
                VisitMethods(typeCollection[typeIndex].Methods);
                // visit Fields
                VisitFields(typeCollection[typeIndex].Fields);
                // visit Properties
                VisitProperties(typeCollection[typeIndex].Properties);
            }
        }

        /// <summary>
        /// Takes in a collection of types and invokes <see cref="InjectionComponent.VisitMethod(MethodDefinition)"/> 
        /// on all components. 
        /// </summary>
        protected void VisitMethods(Collection<MethodDefinition> methodCollection)
        {
            // Only visit methods if we have any components that modify them.
            for (var methodIndex = methodCollection.Count - 1; methodIndex >= 0; methodIndex--)
            {
                for (var componentIndex = mSubObjects.Count - 1; componentIndex >= 0; componentIndex--)
                {
                    mSubObjects[componentIndex].VisitMethod(methodCollection[methodIndex]);
                }
            }
        }

        /// <summary>
        /// Takes in a collection of types and invokes <see cref="InjectionComponent.VisitField(FieldDefinition)"/> 
        /// on all components. 
        /// </summary>
        protected void VisitFields(Collection<FieldDefinition> fieldCollection)
        {
            // Only visit fields if we have any components that modify them.
            for (var fieldIndex = fieldCollection.Count - 1; fieldIndex >= 0; fieldIndex--)
            {
                for (var componentIndex = mSubObjects.Count - 1; componentIndex >= 0; componentIndex--)
                {
                    mSubObjects[componentIndex].VisitField(fieldCollection[fieldIndex]);
                }
            }
        }

        /// <summary>
        /// Takes in a collection of types and invokes <see cref="InjectionComponent.VisitField(FieldDefinition)"/> 
        /// on all components. 
        /// </summary>
        protected void VisitProperties(Collection<PropertyDefinition> propertyCollection)
        {
            // Only visit properties if we have any components that modify them.
            for (var propertyIndex = propertyCollection.Count - 1; propertyIndex >= 0; propertyIndex--)
            {
                for (var componentIndex = mSubObjects.Count - 1; componentIndex >= 0; componentIndex--)
                {
                    mSubObjects[componentIndex].VisitProperty(propertyCollection[propertyIndex]);
                }
            }
        }
    }
}
