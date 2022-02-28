﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

namespace AttributeInjector.Editor.UtilityTypes
{
    /// <summary>
    /// A class used to make having subobjects very easy to do. With built
    /// in support for creating an destroying instances.
    /// </summary>
    /// <typeparam name="T">The scriptable object type that you want to use as a sub object</typeparam>
    [System.Serializable]
    public class SubObjectController<T> : IEnumerable<T>  where T : ScriptableObject
    {
        [SerializeField, HideInInspector]
        protected List<T> mSubObjects = new List<T>();
        [SerializeField, HideInInspector]
        protected Object mOwner; 

        public SubObjectController()
        { }

        /// <summary>
        /// Gets the sub object at the index sent in.
        /// </summary>
        public T this[int index]
        {
            get { return mSubObjects[index]; }
        }

        /// <summary>
        /// Gets the number of sub objects we are controlling. 
        /// </summary>
        public int Count => mSubObjects.Count;

        /// <summary>
        /// Adds a new instance as a sub object to the owner and returns
        /// that instance. 
        /// </summary>
        public T Add()
        {
            return Add(typeof(T));
        }

        /// <summary>
        /// Adds a new instance of the type or a class that
        /// inherits from type T. Returns back the new
        /// instance.
        /// </summary>
        public I Add<I>() where I : T
        {
            return (I)Add(typeof(I));
        }

        /// <summary>
        /// Sets the owner of this instance used for adding sub objects. 
        /// </summary>
        public void SetOwner(Object owner)
        {
            mOwner = owner;
        }

        /// <summary>
        /// Adds a new instance of the type sent in. If the type
        /// does not match or does not inherit from the base T
        /// and exception will be thrown. 
        /// </summary>
        public T Add(Type type)
        {
            if(typeof(T).IsAssignableFrom(type))
            {
                // Create a new instance
                T newInstance = (T)ScriptableObject.CreateInstance(type);
                // Set Name
                if (type.FullName != null) newInstance.name = type.Name;
                // Add to our array
                mSubObjects.Add(newInstance);
                // Add it as a sub object
                AssetDatabase.AddObjectToAsset(newInstance, mOwner);
                // Return the result
                return newInstance;
            }
            
            throw new System.Exception("The type " + type.FullName + " does not inherit from " + typeof(T).FullName);
        }

        /// <summary>
        /// Returns back if we have an instance of the type sent
        /// in as a sub object.
        /// </summary>
        public bool HasInstanceOfType<I>()
        {
            return HasInstanceOfType(typeof(I));
        }

        /// <summary>
        /// Returns back if we have an instance of the type sent
        /// in as a sub object.
        /// </summary>
        public bool HasInstanceOfType(Type type)
        {
            for (int i = 0; i < mSubObjects.Count; i++)
            {
                if (!type.IsInstanceOfType(mSubObjects[i]))
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the sub object at the sent in index.
        /// </summary>
        /// <param name="index">The index you want to remove. The range is protected.</param>
        /// <returns>True if an element was removed and false if not.</returns>
        public bool Remove(int index)
        {
            if(index < 0 || index >= mSubObjects.Count)
            {
                return false; 
            }
            else
            {
                return Remove(mSubObjects[index]);
            }
        }

        /// <summary>
        /// Removes the element from our list of sub objects and removes it from our
        /// owner. 
        /// </summary>
        /// <param name="element">The element you want to remove</param>
        /// <returns>True if an element was removed.</returns>
        public bool Remove(T element)
        {
            bool elementRemoved = false;
            for (int i = mSubObjects.Count - 1; i >= 0; i--)
            {
                if(mSubObjects[i] == element)
                {
                    // Flag that we removed something
                    elementRemoved = true;
                    // Destroy the element
                    Object.DestroyImmediate(mSubObjects[i], true);
                    // Remove the index
                    mSubObjects.RemoveAt(i); 
                }
            }
            return elementRemoved; 
        }

        /// <summary>
        /// Removes all our sub objects. 
        /// </summary>
        public void RemoveAll()
        {
            for (int i = mSubObjects.Count - 1; i >= 0; i--)
            {
                // Delete it
                Object.DestroyImmediate(mSubObjects[i], true);
            }
            // Clear our array
            mSubObjects.Clear(); 
        }

        /// <summary>
        /// Gets the generic IEnumerator to loop
        /// over this collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for(int i = 0; i < mSubObjects.Count; i++)
            {
                yield return mSubObjects[i];
            }
        }

        /// <summary>
        /// Gets the base for IEnumerable function. 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < mSubObjects.Count; i++)
            {
                yield return mSubObjects[i];
            }
        }
    }
}
