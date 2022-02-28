using UnityEngine;

public class TestMono : MonoBehaviour
{
    private void Start()
    {
        InvokeRepeating(nameof(DoSomething), 1, 1);
    }
    
    [ComplexLog]
    private int DoSomething()
    {
        var x = Random.Range(1, 1000);
        Debug.Log("DoSomething body");
        return x;
    }
}