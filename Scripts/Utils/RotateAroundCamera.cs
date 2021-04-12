using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundCamera : MonoBehaviour
{
    private const float HALF_PI = Mathf.PI * 0.5f;

    public float delay;
    public float duration;

    private float timer;
    private float pace;
    private float totalAngle;
    private float angleStep;
    private Transform cameraTransform;
    private Vector3 direction;
    private Quaternion rotation;

    void Awake()
    {
        cameraTransform = FindObjectOfType<OVRCameraRig>().transform;
        timer = -delay / duration;
        pace = 1f / duration;
    }
    // Start is called before the first frame update
    void Start()
    {
        Shake[] shakes = FindObjectsOfType<Shake>();
        for (int i = 0; i < shakes.Length; i++)
        {
            shakes[i].enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 1)
        {
            timer += Time.deltaTime * pace;
            if(timer > 0f)
            {
                float mod = HALF_PI * Mathf.Sin(Mathf.PI * timer);
                angleStep = mod * Time.deltaTime * 360f / duration;
                totalAngle += angleStep;
                if(totalAngle < 360f)
                {
                    float deltaY = 0.05f * Mathf.Sin(12f * Mathf.PI * timer);
                    transform.position += deltaY * Vector3.up;
                    transform.RotateAround(cameraTransform.position, Vector3.up, angleStep);
                }
            }   
        }
        else
        {
            Debug.Log(totalAngle);
            enabled = false;
        }
    }
}
