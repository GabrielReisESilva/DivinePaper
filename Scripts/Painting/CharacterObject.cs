using System;
using UnityEngine;

public class CharacterObject : MonoBehaviour
{
    private Action onFinish;

    public Action FinishCallback { set { onFinish = value; } }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnFinish()
    {
        onFinish?.Invoke();
    }
}
