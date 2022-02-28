using System.Collections;
using System.Reflection;
using UnityEditor;

namespace AttributeInjector.Editor.UtilityTypes.Reflected_Members
{
    public class ReflectedField<T> 
    {
        private object _mTargetInstance;
        private FieldInfo _mFieldInfo;

        public ReflectedField(SerializedObject serializedObject, string propertyPath)
        {
            FindTarget(serializedObject, propertyPath);
        }

        private void FindTarget(SerializedObject serializedObject, string propertyPath)
        {
            _mTargetInstance = null;

            foreach (var obj in serializedObject.targetObjects)
            {
                _mTargetInstance = obj;
                var members = propertyPath.Split('.');

                for (var memberIndex = 0; memberIndex < members.Length; memberIndex++)
                {
                    var memberName = members[memberIndex];
                    var instanceType = _mTargetInstance.GetType();

                    if(string.CompareOrdinal("Array", memberName) == 0)
                    {
                        memberIndex++; 
                        var arrayPath = members[memberIndex];
                        var arrayIndex = ReflectedMembers.GetArrayIndexFromPropertyPath(arrayPath);
                        var asList = (IList) _mTargetInstance;
                        _mTargetInstance = asList[arrayIndex];
                    }
                    else if (memberIndex == members.Length - 1)
                    {
                        _mFieldInfo = instanceType.GetField(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    }
                    else
                    {
                        var fieldInfo = instanceType.GetField(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                        if (fieldInfo != null) _mTargetInstance = fieldInfo.GetValue(_mTargetInstance);
                    }
                }

                if (_mFieldInfo == null || !_mFieldInfo.IsStatic) continue;
                _mTargetInstance = null;
                break;
            }
        }


    }
}
