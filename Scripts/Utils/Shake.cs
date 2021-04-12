using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    public float delay;
    public float duration;
    public float amplitude;
    public float semicycles;

    private float timer;
    private float pace;
    private float deltaX;
    // Start is called before the first frame update
    void Awake()
    {
        pace = 1f / duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 1)
        {
            timer += Time.deltaTime * pace;
            if(timer > 0)
            {
                deltaX = amplitude * Mathf.Sin(semicycles * Mathf.PI * timer);
                transform.position += deltaX * transform.right;
            }
        }
        else
        {
            enabled = false;
        }
    }

    void OnEnable()
    {
        timer = - delay / duration;
    }
}
