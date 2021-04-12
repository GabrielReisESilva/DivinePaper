using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIdentifier : MonoBehaviour
{
    private const float SIMILARITY_RATE = 3f;
    private const float MIN_CHANCE = 0.75f;

    public bool learn;
    public Animator characterAnimation;
    public CanvasPoint pointPrefab;
    public Brush brush;
    public PaintCanvas canvas;
    public Renderer canvasRenderer;
    //public Texture2D context;
    public MagicPaper magicPaperPrefab;
    public Character[] database;
    public Character noCharacter;

    private StageManager stageManager;
    private Texture2D tex;
    private TextRecognition textRecognition;
    private CanvasPoint[][] points;

    // Use this for initialization
    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        textRecognition = GetComponent<TextRecognition>();
        Vector3 startPoint = canvasRenderer.bounds.min;
        Vector2 dimension = canvasRenderer.bounds.size;
        //BuildGrid(startPoint, dimension, gridSize, gridSize, out points);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Q))
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            float chance = 0;
            Character character = noCharacter;
            Stroke[] newStrokes = Character.Normalize(brush.Strokes);

            for (int i = 0; i < database.Length; i++)
            {
                float characterChance = CompareCharacter(database[i], brush.Strokes);
                if(characterChance > MIN_CHANCE && characterChance > chance)
                {
                    chance = characterChance;
                    character = database[i];
                }

                Debug.Log(database[i].originalCharacter +  " - Chance: " + characterChance);
            }
#if UNITY_EDITOR
            if(learn)
                Character.CreateCharacterReference(newStrokes);
#endif
            stageManager.OnCharacterDrawn(character);
        }
            //CompareImage();
	}

    private void BuildGrid(Vector3 startPoint, Vector2 dimension, int rowNumber, int colNumber, out CanvasPoint[][] points)
    {
        points = new CanvasPoint[rowNumber][];
        if (colNumber < 2 || rowNumber < 2)
        {
            Debug.LogError("Number of columns and rows must be more than 1");
            return;
        }
        float xStep = dimension.x / (colNumber - 1);
        float yStep = dimension.y / (rowNumber - 1);
        for (int i = 0; i < rowNumber; i++)
        {
            points[i] = new CanvasPoint[colNumber];
            for (int j = 0; j < colNumber; j++)
            {
                Vector3 pos = startPoint + new Vector3(j * xStep, i * yStep);
                points[i][j] = Instantiate(pointPrefab, pos, Quaternion.identity, transform);
            }
        }
    }

    private void HandleCharacter(Character original)
    {
        //MOVE EVERYTHING TO STAGE MANAGER
        Debug.Log(original.name);
 
    }

    private void CompareImage()
    {
        Texture canvasTexture = canvasRenderer.material.mainTexture;
        //Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGBA32, false);
        //tex.SetPixels(canvasTexture);
        //tex = canvasTexture.ToTexture2D();
        tex = canvas.Texture;
        //tex.SetPixels(0, 0, context.width, context.height, context.GetPixels());
        //tex.Apply();
        Debug.Log("tex: " + tex);
        textRecognition.Recognize(tex,(string s)=> { Debug.Log("Letter: " + s); });
    }

    private float CompareCharacter(Character character, Stroke[] strokes)
    {
        if (strokes.Length != character.NumberOfStrokes) return 0f;

        float score = 0;
        Stroke[] normalizedStrokes = Character.Normalize(strokes);
        for (int i = 0; i < character.NumberOfStrokes; i++)
        {
            score += CompareStroke(normalizedStrokes[i], character.strokes[i]);
        }

        return score / character.NumberOfStrokes;
    }

    private float CompareStroke(Stroke s1, Stroke s2)
    {
        return (ComparePoint(s1.start, s2.start) + ComparePoint(s1.end, s2.end)) * 0.5f;
    }

    private float ComparePoint(Vector2 p1, Vector2 p2)
    {
        float x = Vector2.Distance(p1, p2);
        float y = Mathf.Cos(10 * x / Mathf.PI) + 1;
        return Mathf.Pow(y * 0.5f, SIMILARITY_RATE);//SM = 3
        //return 1f / Mathf.Pow((1f + x),SIMILARITY_RATE);//SM = 2
    }

    private void OnDrawGizmos()
    {
        if (points == null) return;

        for (int i = 0; i < points.Length; i++)
        {
            for (int j = 0; j < points[i].Length; j++)
            {
                if (points[i][j].IsActive)
                    Gizmos.color = UnityEngine.Color.gray;
                else
                    Gizmos.color = UnityEngine.Color.green;

                Gizmos.DrawSphere(points[i][j].transform.position, 0.1f);
            }
        }
    }
}

public static class TextureExtentions
{
    public static Texture2D ToTexture2D(this Texture texture)
    {
        return Texture2D.CreateExternalTexture(
            texture.width,
            texture.height,
            TextureFormat.RGBA32,
            false, false,
            texture.GetNativeTexturePtr());
    }

    public static void PaintOver(this Texture2D texture, int x, int y, int brushSize, Color[] brushPixels, Texture2D reference, bool[] paintedPoints = null, float ink = 1f, Color[] copy = null)
    {
        for (int i = 0; i < brushSize; i++)
        {
            for (int j = 0; j < brushSize; j++)
            {
                if (reference != null && reference.GetPixel(x + i, y + j) == Color.white)
                    continue;
                if ((x + i) * texture.height + y + j >= paintedPoints.Length || (x + i) * texture.height + y + j < 0)
                    continue;
                if (paintedPoints[(x + i) * texture.height + y + j]) //x*texture.size + y..
                    continue;
                if (paintedPoints[(x + i) * texture.height + y + j]) //x*texture.size + y..
                    continue;

                Color originalPixel = texture.GetPixel(x + i, y + j);
                Color brushPixel = brushPixels[i * brushSize + j];
                if (brushPixel.a < 0.01f) continue; //INK TEST: it was 0.5 before. Don't waste time with invisible pixels...

                Color newPixel = Color.Lerp(originalPixel, brushPixel, ink);//(1f-ink) * originalPixel + ink * brushPixel + 0.00f * originalPixel * brushPixel;
                if(copy != null) copy[(x + i) * texture.height + y + j] = Color.Lerp(Color.clear, brushPixel, ink);
                newPixel.a = 1f;
                texture.SetPixel(x + i, y + j, newPixel);
                paintedPoints[(x + i) * texture.height + y + j] = true;
                //Debug.Log(newPixel);
            }
        }
    }
}