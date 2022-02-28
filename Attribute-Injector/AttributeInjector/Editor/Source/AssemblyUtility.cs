using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AttributeInjector.Editor
{
    public static class AssemblyUtility
    {
        private static IList<Assembly> _mAssemblies;
        
        /// <summary>
        /// Populates our list of loaded assemblies
        /// </summary>
        public static void PopulateAssemblyCache()
        {
            var assemblyPaths = GetUserAssemblyPaths();
            _mAssemblies = new Assembly[assemblyPaths.Count];
            for(var i = 0;  i < assemblyPaths.Count; i++)
            {
                _mAssemblies[i] = Assembly.LoadFile(assemblyPaths[i]);
            }
        }

        private const BindingFlags StaticBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Forces Unity to recompile all scripts and then refresh. 
        /// </summary>
        /// 
        [MenuItem("CONTEXT/InjectorSettings/Refresh Assemblies")]
        public static void DirtyAllScripts()
        {
            var editorAssembly = typeof(UnityEditor.Editor).Assembly;
            var compilationInterface = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
            if (compilationInterface != null)
            {
                var dirtyAllScriptsMethod = compilationInterface.GetMethod("DirtyAllScripts", StaticBindingFlags);
                if (dirtyAllScriptsMethod != null) dirtyAllScriptsMethod.Invoke(null, null);
            }
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Returns back true if the dll at the path is a managed dll.
        /// </summary>
        /// <param name="systemPath">The full system path to the dll.</param>
        /// <returns>True if a managed dll and false if not. </returns>
        private static bool IsManagedAssembly(string systemPath)
        {
            var dllType = InternalEditorUtility.DetectDotNetDll(systemPath);
            return dllType != DllType.Unknown && dllType != DllType.Native;
        }

        /// <summary>
        /// Returns back all the user assemblies define in the unity project. 
        /// </summary>
        /// <returns></returns>
        private static IList<string> GetUserAssemblyPaths()
        {
            var assemblies = new List<string>(20);
            FindAssemblies(Application.dataPath, 120, assemblies);
            FindAssemblies(Application.dataPath + "/../Library/ScriptAssemblies/", 2, assemblies);
            return assemblies;
        }

        /// <summary>
        /// Finds all the managed assemblies at the give path. It will look into sub folders
        /// up until the max depth. 
        /// </summary>
        private static void FindAssemblies(string systemPath, int maxDepth, List<string> result)
        {
            if (maxDepth <= 0) return;
            try
            {
                if (!Directory.Exists(systemPath)) return;
                var directorInfo = new DirectoryInfo(systemPath);
                
                result.AddRange(from file in directorInfo.GetFiles()
                    where IsManagedAssembly(file.FullName)
                    select file.FullName);
                var directories = directorInfo.GetDirectories();
                foreach (var current in directories)
                {
                    FindAssemblies(current.FullName, maxDepth - 1, result);
                }
            }
            catch
            {
                // Nothing to do here
            }
        }
    }
}
