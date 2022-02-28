using System;
using UnityEngine;
using AttributeInjector;

[Injection]
public class ComplexLog : Attribute
{   
    [Rise(When.OnEntry)]
    public void Begin([Argument(Method.Name)] string name)
    {
        Debug.LogError($"Entering method {name}");
    }
    
    [Rise(When.OnExit)]
    public void End([Argument(Method.Name)] string name, [Argument(Method.ReturnValue)] int value)
    {
        Debug.LogError($"Exiting method {name} with return value: {value.ToString()}");
    }
}