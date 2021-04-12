using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brush : MonoBehaviour
{
    private const float TIP_SIZE = 0.1f;
    private const float MAX_INK = 1f;
    private int CANVAS_MASK;

    public Texture2D brush;
    public PaintCanvas canvas;
    public Transform controller;
    public Transform line;
    public Renderer lineRenderer;

    private List<Stroke> strokes;
    private Collider2D col;
    private Ray ray;
    private RaycastHit hit;
    private bool lastTouch;
    private Quaternion lastRotation;
    private float tipSize; //middleToTip
    private Vector2 startPosition;
    private Vector2 lastTouchPosition;
    private float brushDistance;
    private Color[][] brushSizes;

    public Stroke[] Strokes { get { return strokes.ToArray(); } }
    // Use this for initialization
    void Start()
    {
        col = GetComponent<Collider2D>();
        CANVAS_MASK = 1 << LayerMask.NameToLayer("Canvas");

        brushSizes = new Color[PaintCanvas.BRUSH_SIZE_MAX + 1][];// - PaintCanvas.BRUSH_SIZE_MIN + 1][];
        //canvas.BrushStyle = GetBrushStyle(Color.black);
        strokes = new List<Stroke>();

        for (int i = 0; i < brushSizes.Length; i++)
        {
            brushSizes[i] = CreateBrushStyle(i, Color.black);//PaintCanvas.BRUSH_SIZE_MIN + i, Color.black);
        }
        canvas.BrushSizes = brushSizes;
        canvas.AdjustBrushSize();
    }

    // Update is called once per frame
    void Update()
    {
        tipSize = TIP_SIZE;

        if (lastTouch) tipSize *= 1.1f; //more natural writing

        if(CastingOnCanvas())
        {
            canvas.HitDistance = hit.distance;
            lineRenderer.material.color = Color.red;
            transform.position = hit.point;
            if (PressingButton())
            {
                //HapticPulse
                brushDistance += Vector2.Distance(lastTouchPosition, hit.textureCoord);
                lastTouchPosition = hit.textureCoord;
                //canvas.BrushInk = brushDistance / MAX_INK;
                canvas.TouchPosition = lastTouchPosition;
                canvas.Touching = true;

                if (!lastTouch)
                {
                    startPosition = lastTouchPosition;
                    col.enabled = true;
                    lastTouch = true;
                    lastRotation = transform.localRotation;
                }
            }
            else
            {
                if (lastTouch)
                {
                    brushDistance = 0;
                    strokes.Add(new Stroke(startPosition, lastTouchPosition));
                    Debug.Log("Stroke Added!");
                    col.enabled = false;
                    canvas.Touching = false;

                    lastTouch = false;
                }
            }
            /*
            else
            {
                lastTouch = false;
                canvas.Touching = false;
            }
            */
        }
        else if(!PressingButton())
        {
            canvas.HitDistance = 10f;
            transform.position = Vector3.zero;
            lineRenderer.material.color = new Color(1f, 0f, 0f, 0.2f);
            if (lastTouch)
            {
                strokes.Add(new Stroke(startPosition, lastTouchPosition));
                Debug.Log("Stroke Added!");
                col.enabled = false;
                canvas.Touching = false;

                lastTouch = false;
            }
        }
        else
        {
            canvas.HitDistance = 10f;
            transform.position = Vector3.zero;
            lineRenderer.material.color = new Color(1f, 0f, 0f, 0.2f);
        }

        if (lastTouch)
            transform.localRotation = lastRotation;
	}

    private bool PressingButton()
    {
        //#if UNITY_EDITOR
        //return Input.GetMouseButton(0); //Delete this for VR
        //#else
        return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
//#endif
    }

    private bool CastingOnCanvas()
    {
//#if UNITY_EDITOR
        //ray = Camera.main.ScreenPointToRay(Input.mousePosition); //Delete this for VR
//#else
        ray = new Ray(controller.position, controller.forward);
//#endif
        return Physics.Raycast(ray, out hit, 20f, CANVAS_MASK); //Replace this with if(Physics.Raycast(brushTip.position, transform.up, out hit, tipSize, CANVAS_MASK))
    }

    private Color[] CreateBrushStyle(int brushSize, Color color)
    {
        if(brush)
            return brush.GetPixels();

        float half = brushSize * 0.5f;
        Color[] brushStyle = new Color[brushSize * brushSize];
        for (int i = 0; i < brushStyle.Length; i++)
        {
            float x = i / brushSize;
            float deltaX = x - half;
            float y = i % brushSize;
            float deltaY = y - half;
            float dist = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
            float alpha = Mathf.Lerp(0f, 1f, dist / ((float)brushSize * 0.5f));
            brushStyle[i] = (alpha < 0.8f) ? Color.black : new Color(0f, 0f, 0f, 0f);
            //brushStyle[i].a = alpha;
//            Debug.Log(alpha);
            /*
            if (dist < 5)
                brushStyle[i] = color;
            else
                brushStyle[i] = Color.white;
                */
        }
        return brushStyle;
    }

    public void ResetStrokes()
    {
        strokes.Clear();
    }
}
