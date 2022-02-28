using UnityEditor;
using UnityEngine;

namespace AttributeInjector.Editor
{
    [InitializeOnLoad]
    internal class Constants
    {
        /// <summary>
        /// The root path to the project not including the '/Assets' part ending with a slash
        /// </summary>
        public static readonly string ProjectRoot; 

        static Constants()
        {
            var dataPath = Application.dataPath;
            ProjectRoot = dataPath.Substring(0, dataPath.Length - 6);
        }
    }
}