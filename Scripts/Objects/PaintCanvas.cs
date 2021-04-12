using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PaintCanvas : MonoBehaviour
{
    public const int BRUSH_SIZE_MIN = 10;
    public const int BRUSH_SIZE_MAX = 30;
    private const float INTERPOLATE_STEP = 0.01f;
    private const float BRUSH_TIP_TOUCH_DISTANCE = 0.005f;
    private const float BRUSH_SIZE_TO_POINTER_SCALE = 0.008f / 20f;
    private const float INK_DURATION = 2f; //deprecated
    private const float INK_DESCREASE_PACE = 1f / INK_DURATION; //deprecated
    private const float INK_DESCREASE_RATE = 1f / 900f; //OLD: 600

    private int textureSize = 512;
    public int brushSize = 10;
    
    public Transform leftController;
    private Transform anchor;

    public Transform brushTip;
    public Transform brushPointer;
    private Texture2D texture;
    private Texture2D mask;
    private Color[] brushPixels;
    private Color[][] brushSizes;
    private Color[] originalTexture;
    private Color[] inkLayer;
    private bool[] paintedPoints;

    private bool paintOver = true;
    private bool touching, touchingLast;
    private float posX, posY;
    private float lastX, lastY;
    private float hitDistance;
    private float ink;
    private Renderer rend;

    public float HitDistance { set => hitDistance = value; }
    public Color[] InkLayer { get => inkLayer; }
    public Color[] BrushStyle { set => brushPixels = value; }
    public Color[][] BrushSizes { set => brushSizes = value; }
    public bool Touching { set => touching = value; }
    public Vector2 TouchPosition { set { posX = value.x; posY = value.y; }}
    public Texture2D Texture { get => texture; }
    public int BrushSize { get => brushSize; }
    public Quaternion Rotation { get => anchor.rotation; }

    // Use this for initialization
    void Start () {
        rend = GetComponent<Renderer>();
        originalTexture = ((Texture2D)rend.material.mainTexture).GetPixels(0,0,512,512);

        anchor = transform.parent.parent;
        ResetCanvas();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            //Debug.Log("Canvas as active? " + gameObject.activeInHierarchy);
            if (!rend.enabled)
                rend.enabled = true;
            anchor.position = leftController.position;
            anchor.rotation = leftController.rotation;
        }

        AdjustBrushSize();

        if (touchingLast && brushSize > 0)
        {
            int x = (int)(posX * textureSize - brushSize * 0.5f);
            int y = (int)(posY * textureSize - brushSize * 0.5f);

            ink -= (Mathf.Abs(x - lastX) + Mathf.Abs(y - lastY)) * INK_DESCREASE_RATE; //TODO descrease faster at first

            //paintedPoints = new bool[textureSize * textureSize];
            if (paintOver)
                texture.PaintOver(x, y, brushSize, brushPixels, mask, paintedPoints, ink, inkLayer);
            else
                texture.SetPixels(x, y, brushSize, brushSize, brushPixels);
            //MEMORY WASTE, BUT QUICK, SOLUTION
            for (float i = INTERPOLATE_STEP; i < 1f; i += INTERPOLATE_STEP)
            {
                int lerpX = (int)Mathf.Lerp(lastX, (float)x, i);
                int lerpY = (int)Mathf.Lerp(lastY, (float)y, i);
                float newInk = ink + (Mathf.Abs(x - lerpX) + Mathf.Abs(y - lerpY)) * INK_DESCREASE_RATE;

                //float modX = Mathf.Abs(lastX - lerpX); //get smaller as approach destiny
                //int newSize = (int)Mathf.Lerp(0, brushSize, 0.2f*modX);

                if (paintOver)
                    texture.PaintOver(lerpX, lerpY, brushSize, brushPixels, mask, paintedPoints, newInk, inkLayer);
                else
                    texture.SetPixels(lerpX, lerpY, brushSize, brushSize, brushPixels);
            }

            texture.Apply();

            lastX = (float)x;
            lastY = (float)y;
        }
        else
        {
            if(ink != 1f)
                paintedPoints = new bool[textureSize * textureSize];

            ink = 1f;
            lastX = (posX * textureSize - brushSize * 0.5f);
            lastY = (posY * textureSize - brushSize * 0.5f);
        }


        touchingLast = touching;
    }
    public void ResetCanvas()
    {
        inkLayer = new Color[originalTexture.Length];
        //Renderer rend = GetComponent<Renderer>();
        texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        //Color[] whiteSquare = Enumerable.Repeat<Color>(Color.white, textureSize * textureSize).ToArray<Color>();
        //Color[] whiteSquare = Enumerable.Repeat<Color>(new Color(0f,0f,0f,0f), textureSize * textureSize).ToArray<Color>();
        texture.SetPixels(0, 0, textureSize, textureSize, originalTexture);
        texture.Apply();
        rend.material.mainTexture = texture;
    }

    public void SetColor(Color color)
    {
        brushPixels = Enumerable.Repeat<Color>(color, brushSize * brushSize).ToArray<Color>();
    }

    public void AdjustBrushSize()
    {
        brushSize = GetBrushSize();
        brushPixels = brushSizes[brushSize];// - BRUSH_SIZE_MIN];
        brushPointer.localScale = Vector3.one * brushSize * BRUSH_SIZE_TO_POINTER_SCALE;
    }

    public void MakeItDissapear()
    {
        rend.enabled = false;
    }

    private int GetBrushSize()
    {
        float brushDelta = hitDistance - 0.03f;
        //Debug.Log(brushDelta);
        if (brushDelta > BRUSH_TIP_TOUCH_DISTANCE) return (int)(0.5f + (float)BRUSH_SIZE_MIN * 2f * BRUSH_TIP_TOUCH_DISTANCE / brushDelta);
        else if (brushDelta < 0) return BRUSH_SIZE_MAX;
        else
        {
            return BRUSH_SIZE_MIN + (int)(0.5f + (BRUSH_SIZE_MAX - BRUSH_SIZE_MIN) * (BRUSH_TIP_TOUCH_DISTANCE - brushDelta) / BRUSH_TIP_TOUCH_DISTANCE);
        }
    }
}
