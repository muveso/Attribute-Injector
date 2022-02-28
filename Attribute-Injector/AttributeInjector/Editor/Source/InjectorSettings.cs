using System;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AttributeInjector.Editor.AssemblyHolders;
using AttributeInjector.Editor.Components;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AttributeInjector.Editor
{
    [CreateAssetMenu(menuName = "Attribute Injector/Settings", fileName = "Injector Settings")]
    public class InjectorSettings : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private ComponentController mComponents;
        
        [SerializeField, HideInInspector]
        private List<InjectedAssembly> mWeavedAssemblies;
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Instance();
        }

        /// <summary>
        /// Gets the instance of our Settings if it exists. Returns null
        /// if no instance was created. 
        /// </summary>
        private static void Instance()
        {
            // Find all settings
            var guids = AssetDatabase.FindAssets("t:InjectorSettings");
            // Load them all
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.LoadAssetAtPath<InjectorSettings>(assetPath);
            }
        }

        [PostProcessScene]
        public static void PostprocessScene()
        {
            if (!BuildPipeline.isBuildingPlayer) return;
            
            var scene = SceneManager.GetActiveScene();
            
            if (!scene.IsValid() || scene.buildIndex != 0) return;
            
            var guids = AssetDatabase.FindAssets("t:InjectorSettings");
            
            if (guids.Length <= 0) return;
                    
            var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var settings = AssetDatabase.LoadAssetAtPath<InjectorSettings>(assetPath);
            settings.WeaveModifiedAssemblies();
        }

        /// <summary>
        /// Invoked when our module is first created and turned on
        /// </summary>
        [UsedImplicitly]
        private void OnEnable()
        {
            AssemblyUtility.PopulateAssemblyCache();

            mComponents ??= new ComponentController();
            mWeavedAssemblies ??= new List<InjectedAssembly>();
            mComponents.SetOwner(this);

            CheckValues();
            
            foreach (var asm in mWeavedAssemblies)
                asm.OnEnable();

            AssemblyUtility.PopulateAssemblyCache();

#if UNITY_2019_1_OR_NEWER
            CompilationPipeline.assemblyCompilationFinished += ComplicationComplete;
#elif UNITY_2017_1_OR_NEWER
            AssemblyReloadEvents.beforeAssemblyReload += WeaveModifiedAssemblies;
#else
            Debug.Warning("Dynamic Assembly Reload not support until Unity 2017. Enter play mode to reload assemblies to see the effects of Weaving.");
#endif
        }

#if UNITY_2019_1_OR_NEWER
        /// <summary>
        /// Invoked whenever one of our assemblies has completed compiling.  
        /// </summary>
        private void ComplicationComplete(string assemblyPath, CompilerMessage[] compilerMessages)
        {
            WeaveAssembly(assemblyPath);
        }
#endif

        /// <summary>
        /// Loops over all changed assemblies and starts the weaving process for each. 
        /// </summary>
        private void WeaveModifiedAssemblies()
        {
            IList<string> assemblies = mWeavedAssemblies
                     .Where(a => a.HasChanges())
                     .Where(a => a.IsActive)
                     .Select(a => a.relativePath)
                     .ToArray();

            foreach (var assembly in assemblies) WeaveAssembly(assembly);
        }


        /// <summary>
        /// Returns back an instance of our symbol reader for 
        /// </summary>
        /// <returns></returns>
        private static ReaderParameters GetReaderParameters()
        {
            var parameters = new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                ReadWrite = true,
                AssemblyResolver = new AssemblyResolver(),
                ReadSymbols = true,
                SymbolReaderProvider = new PdbReaderProvider()
            };
            return parameters;
        }

        /// <summary>
        /// Returns back the instance of the symbol writer provide.
        /// </summary>
        private static WriterParameters GetWriterParameters()
        {
            var parameters = new WriterParameters
            {
                WriteSymbols = true,
                SymbolWriterProvider = new PdbWriterProvider()
            };
            return parameters;
        }

        /// <summary>
        /// Invoked for each assembly that has been compiled. 
        /// </summary>
        private void WeaveAssembly(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
                return;

            var filePath = Path.Combine(Constants.ProjectRoot, assemblyPath);
            if (!File.Exists(filePath))
                return;

            using (var assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var moduleDefinition = ModuleDefinition.ReadModule(assemblyStream, GetReaderParameters()))
                {
                    mComponents.Initialize(this);
                    mComponents.VisitModule(moduleDefinition);
                    moduleDefinition.Write(GetWriterParameters());
                }
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
#if UNITY_EDITOR
        private void CheckValues()
        {
            if (mComponents.Count != 0) return;
            
            EditorApplication.update -= DelayedInit;
            EditorApplication.update += DelayedInit;
        }

        private void OnDestroy()
        {
            EditorApplication.update -= DelayedInit;
        }

        private void DelayedInit()
        {
            if (!AssetDatabase.Contains(this))
                return;

            EditorApplication.update -= DelayedInit;
            mComponents.Add<InjectionAttributeModifier>();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}