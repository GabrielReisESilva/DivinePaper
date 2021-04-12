using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour {

    private const int SUN_INDEX = 0;
    private const int TREE_INDEX = 1;
    private const int CLOUD_INDEX = 2;
    private const int WIND_INDEX = 3;

    private static readonly Vector3 DRAW_OFFSET = Vector3.up * 2.56f;

    public Transform vrCamera;
    public MagicPaper magicPaperPrefab;
    public GameObject genericDrawPrefab;
    public GameObject[] characterPrefabs;
    public Transform[] characterPosition;
    public Sprite[] characterSprite;
    public GameObject title;
    public SpriteRenderer feedback;
    public Animator kiki;
    public GameObject footprints;
    public GameObject[] otherElements;
    public Material skyboxMaterial;

    private Dialogue dialogue;
    private PaintCanvas canvas;
    private Brush brush;
    private Bezier bezier;
    private bool pickingPosition;
    private Texture2D previewTexture;
    private RichText[][] firstDialogue;
    private int[] pauses;
    private Action[] onPause;
    private bool partI;
    private bool partII;
    // Use this for initialization
    void Start () {
        canvas = FindObjectOfType<PaintCanvas>();
        brush = FindObjectOfType<Brush>();
        bezier = FindObjectOfType<Bezier>();
        dialogue = FindObjectOfType<Dialogue>();

        previewTexture = new Texture2D(canvas.Texture.width, canvas.Texture.height);

        firstDialogue = new RichText[][]
        {
            new RichText[]{"Hello, are you there?"},
            new RichText[]{"Can you help me?"},
            new RichText[]{"You're my only hope..."},
            new RichText[]{"This land was taken by the darkness"},
            new RichText[]{"It has passed months since I last see the", new RichText("SUN", RichText.WHITE, true)},//pause
            new RichText[]{"I only see death and destruction"},
            new RichText[]{"If only the", new RichText("TREES", RichText.WHITE, true), "could bloom once more..."}, //pause
            new RichText[]{"There are no", new RichText("CLOUDS", RichText.WHITE, true), "in this empty sky"}, //pause
            new RichText[]{"And I miss the soft touch from the", new RichText("WIND...", RichText.WHITE, true)},             //pause
            new RichText[]{"What is happening?"},
            new RichText[]{"OH MY GOD!"}
        };

        pauses = new int[] { 5, 7, 8, 9 };
        onPause = new Action[]
        {
            ()=>{ characterPosition[SUN_INDEX].gameObject.SetActive(true); },
            ()=>{ characterPosition[TREE_INDEX].gameObject.SetActive(true); },
            ()=>{ characterPosition[CLOUD_INDEX].gameObject.SetActive(true); },
            ()=>{ characterPosition[WIND_INDEX].gameObject.SetActive(true); },
            ()=>{ Debug.Log("Finish"); }
        };

        RenderSettings.skybox.SetColor("_Tint", Color.white * 0.3f);
        StartCoroutine(PlayPartI());
    }
	
	// Update is called once per frame
	void LateUpdate () {
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            canvas.ResetCanvas();
        }

        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            previewTexture.SetPixels(canvas.InkLayer);
            previewTexture.Apply();
            bezier.ToggleDraw(true, previewTexture);
            pickingPosition = true;
        }
        else if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            pickingPosition = false;
            bezier.ToggleDraw(false);
            //MagicPaper paper = Instantiate(magicPaperPrefab) as MagicPaper;
            //paper.MoveTo(canvas, canvas.Texture, bezier.EndPoint, (Vector3 pos) => { CreateGeneric(genericDrawPrefab, pos, canvas.Texture); });
        }
    }

    public void OnCharacterDrawn(Character original)
    {
        //characterAnimation.SetTrigger(original.name);
        HandleCharacter(original.mean);

        brush.ResetStrokes();
        canvas.ResetCanvas();
    }

    private void HandleCharacter(string mean)
    {
        /*
        if(mean == "null")
        {
            if(!pickingPosition)
                OnNotRecognize();
            return;
        }
        */
        MagicPaper paper = Instantiate(magicPaperPrefab) as MagicPaper;
        Texture2D paperTexture = new Texture2D(canvas.Texture.width, canvas.Texture.height);
        paperTexture.SetPixels(canvas.Texture.GetPixels());
        paperTexture.Apply();
        switch (mean)
        {
            case "tree":
                paper.MoveTo(canvas, paperTexture, characterPosition[TREE_INDEX].position, (Vector3 pos) => 
                {
                    Create(characterPrefabs[TREE_INDEX], pos, () =>
                    {
                        dialogue.Resume(7);
                        otherElements[TREE_INDEX].SetActive(true);
                    });
                });
                characterPosition[TREE_INDEX].gameObject.SetActive(false);
                ShowFeedback(characterSprite[TREE_INDEX]);
                break;
            case "sun":
                paper.MoveTo(canvas, paperTexture, characterPosition[SUN_INDEX].position, (Vector3 pos) => 
                {
                    Create(characterPrefabs[SUN_INDEX], pos, () =>
                    {
                        dialogue.Resume(5);
                        StartCoroutine(ShineSun());
                    });
                });
                characterPosition[SUN_INDEX].gameObject.SetActive(false);
                ShowFeedback(characterSprite[SUN_INDEX]);
                break;
            case "cloud":
                paper.MoveTo(canvas, paperTexture, characterPosition[CLOUD_INDEX].position, (Vector3 pos) => 
                {
                    Create(characterPrefabs[CLOUD_INDEX], pos, ()=>
                    {
                        dialogue.Resume(8);
                    });
                });
                characterPosition[CLOUD_INDEX].gameObject.SetActive(false);
                ShowFeedback(characterSprite[CLOUD_INDEX]);
                break;
            case "moon":
                break;
            case "bow":
                break;
            case "woman":
                break;
            case "person":
                break;
            case "rain":
                break;
            case "river":
                break;
            case "wind":
                paper.MoveTo(canvas, paperTexture, characterPosition[WIND_INDEX].position, (Vector3 pos) => 
                {
                    Create(characterPrefabs[WIND_INDEX], pos, ()=> 
                    {
                        if (!partI && !partII) StartCoroutine(PlayPartII());
                    });
                });
                characterPosition[WIND_INDEX].gameObject.SetActive(false);
                ShowFeedback(characterSprite[WIND_INDEX]);
                break;
            default:
                //Destroy(paper.gameObject);
                Texture2D inkLayerTexture = new Texture2D(canvas.Texture.width, canvas.Texture.height);
                inkLayerTexture.SetPixels(canvas.InkLayer);
                inkLayerTexture.Apply();
                paper.MoveTo(canvas, paperTexture, bezier.EndPoint/* + DRAW_OFFSET*/, (Vector3 pos) => { CreateGeneric(genericDrawPrefab, pos, inkLayerTexture); });
                OnNotRecognize();
                break;
        }
        canvas.MakeItDissapear();
    }

    public void OnNotRecognize()
    {
        return;
        bezier.ToggleDraw(true);
        pickingPosition = true;
    }

    public void ShowFeedback(Sprite sprite)
    {
        feedback.sprite = sprite;
        feedback.GetComponent<Animator>().SetTrigger("Fade");
    }

    public void Create(GameObject prefab, Vector3 position, Action onFinish)
    {
        Quaternion rotation = Quaternion.LookRotation(position - vrCamera.position, Vector3.up);
        GameObject character = Instantiate(prefab, position, rotation, transform) as GameObject;
        character.GetComponentInChildren<CharacterObject>().FinishCallback = onFinish;
    }

    public void CreateGeneric(GameObject prefab, Vector3 position, Texture2D texture)
    {
        Quaternion rotation = Quaternion.LookRotation(vrCamera.position - position, Vector3.up);
        GameObject genericDraw = Instantiate(prefab, position, rotation, transform) as GameObject;
        genericDraw.GetComponentInChildren<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f);
    }

    private IEnumerator PlayPartI()
    {
        partI = true;
        yield return new WaitForSeconds(3f);
        //SHOW GAME TITLE
        title.SetActive(true);
        yield return new WaitForSeconds(6f);
        //SHOW FOOTPRINTS
        footprints.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        //KIKI: SIGN
        kiki.SetTrigger("Sign");
        yield return new WaitForSeconds(2f);
        //KIKI: PRAY
        //START DIALOGUE
        kiki.SetTrigger("Pray");
        yield return new WaitForSeconds(2f);
        dialogue.StartDialogue(vrCamera.position, kiki.transform.position, false, firstDialogue, pauses, onPause);
        partI = false;
    }

    private IEnumerator PlayPartII()
    {
        partII = true;
        yield return new WaitForSeconds(5f);
        //KIKI: WINDY
        kiki.SetTrigger("Windy");
        yield return new WaitForSeconds(1f);
        dialogue.Resume(9);
        yield return new WaitForSeconds(3f);
        //KIKI: SURPRISE
        kiki.SetTrigger("Surprised");
        yield return new WaitForSeconds(1f);
        //OVER
        partII = false;
    }

    private IEnumerator ShineSun()
    {
        float duration = 2f;
        float timer = 0f;
        float colorValue = 0.5f;
        Color color = Color.white * colorValue;
        while(timer < 1f)
        {
            timer += Time.deltaTime / duration;
            colorValue = Mathf.Lerp(0.3f, 0.6f, timer * timer);
            color = Color.white * colorValue;
            RenderSettings.skybox.SetColor("_Tint", color);
            yield return 0;
        }
        color = Color.white * 0.6f;
        RenderSettings.skybox.SetColor("_Tint", color);
        yield return 0;
    }
}

/*SIDE NOTES:
 * IF NECESSARY TO CHANGE THE SIZE OF THE DRAWINGS
 * - Change (Prefab) Generic Draw > Draw
 * - Change OVRCameraRig > ... > Bezier Curve > ... > Preview > Plane
 * - Change this > HandleCharacter > default
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */

