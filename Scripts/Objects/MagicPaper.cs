using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MagicPaper : MonoBehaviour
{
    private const float MOVEMENT_AMPLITUDE = 0.5f;
    private const float MOVEMENT_CYCLES = 3f;
    private const float MOVEMENT_DURATION = 5f;
    private const float STEP = 1f / MOVEMENT_DURATION;
    private const float INITIALIZE_DURATION = 1f;
    private const float INITIALIZE_STEP = 1f / INITIALIZE_DURATION;

    public Renderer canvasRenderer;
    private Vector3 startPosition;
    private Vector3 targetPosition; //prv
    private Vector3 targetSpot;
    private float timer = -1f; //prv
    private Action<Vector3> Create;
    private bool initialize = false;
    private Quaternion movementRotation;
    private float paperXAngle;
	// Use this for initialization
	void Start () {
        paperXAngle = canvasRenderer.transform.localEulerAngles.x;
	}
	
	// Update is called once per frame
	void Update () {
        if (timer < -0.5f)
            return;
        if (timer < 1f)
        {
            if (initialize)
            {
                timer += INITIALIZE_STEP * Time.deltaTime;
                float timerMod = (Mathf.Sin((timer - 0.5f) * Mathf.PI) + 1f) / 2f;
                transform.position = Vector3.Lerp(transform.position, startPosition, timerMod);
                transform.rotation = Quaternion.Lerp(transform.rotation, movementRotation, timerMod);
            }
            else
            {
                timer += STEP * Time.deltaTime;
                float timeMod = timer * timer;
                targetSpot = Vector3.Lerp(startPosition, targetPosition, timeMod);
                targetSpot.y += MOVEMENT_AMPLITUDE * (1f - timeMod) * Mathf.Sin(2f * MOVEMENT_CYCLES * Mathf.PI * timeMod);
                //transform.up = (targetSpot - transform.position).normalized;
                float paperAngle = (timer > 0.9f) ? 0f : (targetSpot - transform.position).normalized.y * 90f - 90f;
                paperXAngle = Mathf.MoveTowards(paperXAngle, paperAngle, 180f * Time.deltaTime);
                canvasRenderer.transform.localEulerAngles = new Vector3(paperXAngle, 0f, 0f);//.MoveTowards(canvasRenderer.transform.localEulerAngles, paperAngles, 180f * Time.deltaTime);
                //canvasRenderer.transform.localEulerAngles = //new Vector3(canvasRenderer.transform.localEulerAngles.x, 0f, 0f);
                //Vector3.MoveTowards(transform.up, (targetSpot - transform.position).normalized, Time.deltaTime * (1f + 3f * timer));
                transform.position = targetSpot;
                transform.localScale = Vector3.one * (1 + timer);
            }
        }
        else
        {
            if (initialize)
            {
                initialize = false;
                //startPosition += Vector3.up * 0.2f;
                timer = 0f;
            }
            else
            {
                Create(transform.position);
                Destroy(gameObject);
            }
        }
	}

    public void MoveTo(PaintCanvas canvas, Texture2D image, Vector3 destination, Action<Vector3> create)
    {
        Create = create;
        canvasRenderer.material.SetTexture("_MainTex", image);
        transform.position = canvas.transform.position;
        transform.rotation = canvas.Rotation;
        transform.eulerAngles = transform.eulerAngles;
        startPosition = transform.position + Vector3.up * 0.2f;
        targetPosition = destination;
        canvasRenderer.gameObject.SetActive(true);
        timer = 0f;
        movementRotation = Quaternion.FromToRotation(startPosition, targetPosition);
        movementRotation.eulerAngles = new Vector3(0f, movementRotation.eulerAngles.y, 0f);
        initialize = true;
    }
}
