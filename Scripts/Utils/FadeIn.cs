using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeIn : MonoBehaviour
{
    public float delay;
    public float duration;

    private float timer;
    private float pace;
    private Color color;
    private SpriteRenderer rend;
    // Start is called before the first frame update
    void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        timer = - delay / duration;
        pace = 1f / duration;
        color = rend.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 1)
        {
            timer += Time.deltaTime * pace;
            color.a = Mathf.Lerp(0f, 1f, timer);
            rend.color = color;
        }
        else
        {
            color.a = 1f;
            rend.color = color;
            enabled = false;
        }
    }
}
