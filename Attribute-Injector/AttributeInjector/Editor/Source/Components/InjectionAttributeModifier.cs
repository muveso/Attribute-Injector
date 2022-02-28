using System.Collections.Generic;
using System.Linq;
using AttributeInjector.Editor.Type_Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using FluentIL;
using FluentIL.Extensions;
using UnityEngine;

namespace AttributeInjector.Editor.Components
{
    public class InjectionAttributeModifier : InjectionComponent
    {
        private struct InjectionPointer
        {
            public readonly string FunctionName;
            public readonly When InjectionTime;
            public readonly List<Param> Parameters;

            public InjectionPointer(string functionName, When injectionTime, List<Param> parameters)
            {
                FunctionName = functionName;
                InjectionTime = injectionTime;
                Parameters = parameters;
            }
        }

        private struct Param
        {
            public readonly TypeReference Type;
            public readonly string ParameterName;
            public readonly Method ParameterValue;

            public Param(TypeReference type, string parameterName, Method parameterValue)
            {
                Type = type;
                ParameterName = parameterName;
                ParameterValue = parameterValue;
            }
        }

        public override void VisitType(TypeDefinition typeDefinition)
        {
            var customAttribute = typeDefinition.GetCustomAttribute<Injection>();
            if(customAttribute == null)
                return;
            typeDefinition.CustomAttributes.Remove(customAttribute);

            var injectables = new List<InjectionPointer>();
            
            //creating class static instace for ultimate references
            var instanceOfAttr = AttrExtentions.GetInstanceField(typeDefinition, ActiveModule);
            //find all methods which uses declaring attribute
            var targetMethods = SearchMethods(typeDefinition);
            
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var methodDefinition in typeDefinition.Methods)
            {
                var methodAttribute = methodDefinition.GetCustomAttribute<Rise>();
                if (methodAttribute == null) continue;
                
                methodDefinition.CustomAttributes.Remove(methodAttribute);
                
                var injectionTime = (When)methodAttribute.ConstructorArguments[0].Value;
                var functionName = methodDefinition.Name;

                var parameters = new List<Param>();
                foreach (var param in methodDefinition.Parameters)
                {
                    var argumentAttribute = param.GetCustomAttribute<Argument>();
                    param.CustomAttributes.Remove(argumentAttribute);
                    var argumentValue = (Method)argumentAttribute.ConstructorArguments[0].Value;

                    parameters.Add(new Param(param.ParameterType, param.Name, argumentValue));
                }
                
                injectables.Add(new InjectionPointer(functionName, injectionTime, parameters));
            }
            
            foreach (var targetMethod in targetMethods)
            {
                var body = targetMethod.Body;
                
                foreach (var injectable in injectables)
                {
                    var methodName = targetMethod.Name;
                    var callFunction = Import(typeDefinition.GetMethod(injectable.FunctionName));
                    
                    if(injectable.InjectionTime == When.OnEntry)
                    {
                        if (injectable.Parameters.Exists(a => a.ParameterValue == Method.ReturnValue))
                        {
                            Debug.LogError($"You can't use <color=#FF0000>When.OnEntry</color> and <color=#FF0000>Method.ReturnValue</color> together. It is impossible. Fix {callFunction.Name}() function on the {typeDefinition.Name} Attribute Class.");
                            return;
                        }
                        
                        body.AfterEntry((in Cut cut) =>
                        {
                            var ilc = cut;
                            ilc = ilc.ThisOrStatic();
                            ilc = ilc.Load(instanceOfAttr);
                            
                            ilc = injectable.Parameters.Aggregate(ilc, (current, source) => current.Value(methodName));
                        
                            ilc = ilc.Call(callFunction);
                            ilc = ilc.Write(OpCodes.Nop);
                            return ilc;
                        });
                    }
                    else
                    {
                        if (callFunction.Parameters.Count > 2)
                        {
                            Debug.LogError($"Too many useless parameters on {callFunction.Name}. Please use Sources only once.");
                            return;
                        }
                        else
                        {
                            if (injectable.Parameters.Exists(a => a.ParameterValue == Method.ReturnValue))
                            {
                                var returnParam = injectable.Parameters.FirstOrDefault(a => a.ParameterValue == Method.ReturnValue);
                                if(targetMethod.ReturnType.Match(targetMethod.Module.TypeSystem.Void))
                                {
                                    Debug.LogError($"{targetMethod.DeclaringType.Name}.{targetMethod.Name}() has no return value. Fix {typeDefinition.Name}.{callFunction.Name}() parameters on the \"{typeDefinition.Name}\" Attribute Class. <color=#FFFF00>Remove <color=#FF0000>[Argument(Method.ReturnValue)] {returnParam.Type.Name} {returnParam.ParameterName}</color> parameter.</color>");
                                    return;
                                }
                                else
                                {   
                                    if (!targetMethod.ReturnType.Match(returnParam.Type))
                                    {
                                        Debug.LogError($"{targetMethod.DeclaringType.Name}.{targetMethod.Name}() and {typeDefinition.Name}.{callFunction.Name}() are incompatible. Change \"{returnParam.ParameterName}\" parameter type of the {callFunction.Name}([Argument(Method.ReturnValue)] <color=#FF0000>{returnParam.Type.Name}</color> {returnParam.ParameterName}) to {callFunction.Name}([Argument(Method.ReturnValue)] <color=#00FF00>{targetMethod.ReturnType.Name}</color> {returnParam.ParameterName}).");
                                        return;
                                    }
                                }
                            }
                        }
                        
                        
                        body.BeforeExit((in Cut cut) =>
                        {
                            var ilc = cut;
                            var hasReturn = false;
                            VariableDefinition returnVal = null;
                            
                            if (injectable.Parameters.Exists(a => a.ParameterValue == Method.ReturnValue)
                                && !targetMethod.ReturnType.Match(targetMethod.Module.TypeSystem.Void))
                            {
                                returnVal = AttrExtentions.GetOrCreateRetVar(targetMethod);
                                ilc = ilc.Store(returnVal);
                                hasReturn = true;
                            }

                            ilc = ilc.ThisOrStatic();
                            ilc = ilc.Load(instanceOfAttr);

                            ilc = injectable.Parameters.Aggregate(ilc, 
                                (current, source) => 
                                    source.ParameterValue == Method.Name ? 
                                        current.Value(methodName) : 
                                        current.Load(returnVal));

                            ilc = ilc.Call(callFunction);
                            ilc = hasReturn ? ilc.Load(returnVal) : ilc.Write(OpCodes.Nop);
                            return ilc;
                        });
                    }
                }
            }

        }
        
        
        
        private IEnumerable<MethodDefinition> SearchMethods(IMetadataTokenProvider typeDefinition)
        {
            return (from moduleType in ActiveModule.Types 
                    from method in moduleType.Methods 
                    from attribute in method.CustomAttributes 
                    where attribute.AttributeType == typeDefinition 
                    select method)
                .ToList();
        }


    }
}
