using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentIL;
using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UnityEditor;
using UnityEngine;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace AttributeInjector.Editor.Components
{
    public static class Utils {
        private static MethodInfo _clearConsoleMethod;

        private static MethodInfo ClearConsoleMethod {
            get {
                if (_clearConsoleMethod != null) return _clearConsoleMethod;
                var assembly = Assembly.GetAssembly (typeof(SceneView));
                var logEntries = assembly.GetType ("UnityEditor.LogEntries");
                _clearConsoleMethod = logEntries.GetMethod ("Clear");
                return _clearConsoleMethod;
            }
        }

        public static void ShowSource(this MethodBody body)
        {
            foreach (var bodyInstruction in body.Instructions)
            {
                Debug.LogError(bodyInstruction.ToString());
            }
        }

        public static void ClearLogConsole() 
        {
            ClearConsoleMethod.Invoke (new object (), null);
        }
    }
    
    public static class AttrExtentions
    {
        private static readonly StandardType DebuggerHiddenAttribute = new StandardType("System.Diagnostics", "DebuggerHiddenAttribute", assemblyHints: new[] { "System.Diagnostics.Debug" });
        private static readonly Dictionary<MethodBody, VariableDefinition> _returnVariablesCache = new Dictionary<MethodBody, VariableDefinition>();
        
        
        public static VariableDefinition GetOrCreateRetVar(MethodDefinition _method)
        {
            if (!_returnVariablesCache.TryGetValue(_method.Body, out var ret))
            {
                ret = new VariableDefinition(_method.ReturnType);
                _method.Body.Variables.Add(ret);
                _method.Body.InitLocals = true;
                
                _returnVariablesCache.Add(_method.Body, ret);
            }
            return ret;
        }
        
        public static FieldReference GetInstanceField(TypeDefinition source, ModuleDefinition activeModule)
        {
            const string fieldName = "instance_field";

            var field = FindField(source.MakeSelfReference(), fieldName);
            if (field != null) return field;
            
            var fieldDef = new FieldDefinition(fieldName, FieldAttributes.Public, activeModule.ImportReference(source));
            source.Fields.Add(fieldDef);

            field = fieldDef.MakeReference(source.MakeSelfReference());

            InjectInitialization(GetInstanсeInitializer(source), field);

            return field;
        }
        
        private static FieldReference FindField(TypeReference type, string name)
        {
            if (type == null)
                return null;

            var field = type.Resolve().Fields.FirstOrDefault(f => f.Name == name && (f.Attributes & FieldAttributes.Family) != 0);

            if (field == null)
            {
                var basetype = type.Resolve().BaseType;
                if (basetype is GenericInstanceType bgit)
                {

                    Func<TypeReference, TypeReference> resolveGenericArg = ga => type.Module.ImportReference(ga);

                    if (type is GenericInstanceType git)
                    {
                        var origResolveGenericArg = resolveGenericArg;
                        resolveGenericArg = ga => origResolveGenericArg(ga is GenericParameter gp ? git.GenericArguments[gp.Position] : ga);
                    }

                    var gparams = bgit.GenericArguments.Select(resolveGenericArg).ToArray();
                    basetype = type.Module.ImportReference(basetype.Resolve()).MakeGenericInstanceType(gparams);
                }

                return FindField(basetype, name);
            }
            else
                return field.MakeReference(type);

        }
        
        private static void InjectInitialization(MethodDefinition initMethod, FieldReference field)
        {
            initMethod.Body.AfterEntry(
                (in Cut e) => e
                    .IfEqual(
                        (in Cut l) => l.This().Load(field),
                        (in Cut r) => r.Null()
                    )
            );
        }

        private static MethodDefinition GetInstanсeInitializer(TypeDefinition type)
        {
            var hiddenInstance = type.Methods.FirstOrDefault(m => m.Name == "hidden_instance");

            if (hiddenInstance == null)
            {
                hiddenInstance = new MethodDefinition("hidden_instance",
                    MethodAttributes.Private | MethodAttributes.HideBySig, type.Module.TypeSystem.Void);

                type.Methods.Add(hiddenInstance);

                hiddenInstance.Body.Instead((in Cut i) => i.Return());
                hiddenInstance.Mark(type.Module.ImportStandardType(DebuggerHiddenAttribute));

                var constructors = type.Methods.Where(c => c.IsConstructor && !c.IsStatic).ToList();

                foreach (var ctor in constructors)
                    ctor.Body.AfterEntry((in Cut i) => i.This().Call(hiddenInstance.MakeReference(type.MakeSelfReference())));
            }

            return hiddenInstance;
        }
    }
}