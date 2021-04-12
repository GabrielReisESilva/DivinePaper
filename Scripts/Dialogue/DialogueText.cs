using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueText : MonoBehaviour
{
    private const string DEFAULT_COLOR = RichText.DARK_RED;

    private const float CHARACTER_TIME = 0.2f;
    private const float CHARACTER_TIME_STEP = 1f / CHARACTER_TIME;
    public const int LINE_MAX_CHARACTERS = 14;

    public TextMesh textMesh;
    public TextMesh textMeshStatic;
    private Renderer renderer;
    private Renderer staticRenderer;
    private float timer;
    private Color textColor;
    private RichText[] richTexts;
    private List<int> lineStartIndexes;

    public int LineCount { get { return lineStartIndexes.Count; } }

    // Start is called before the first frame update
    void Start()
    {
        renderer = textMesh.GetComponent<Renderer>();
        staticRenderer = textMeshStatic.GetComponent<Renderer>();
        textColor = renderer.material.color;
        //TEST
        RichText richText = new RichText("Hello God, can you hear me?");
        RichText richText2 = new RichText("I really need your help", RichText.BLACK, true);
        RichText richText3 = new RichText("Please........ Only....... Only you can help me", RichText.DARK_RED, false, true);

        //ShowText(richText, richText2, "I can't take this anymore", richText3);
        //END TEST
    }

    // Update is called once per frame
    void Update()
    {
        if(timer < 1)
        {
            timer += Time.deltaTime * CHARACTER_TIME_STEP;
            textColor.a = timer;
            renderer.material.color = textColor;
        }
    }

    public void SetUpText(RichText[] richTexts)
    {
        if (richTexts == null) Debug.LogError("TEXT IS NULL");

        this.richTexts = richTexts;
        string fullText = richTexts[0].Text;
        for (int i = 1; i < richTexts.Length; i++)
        {
            fullText += " " + richTexts[i].Text;
        }

        string[] words = fullText.Split(' ');

        lineStartIndexes = new List<int>();
        lineStartIndexes.Add(0);
        int length = words[0].Length;
        for (int i = 1; i < words.Length; i++)
        {
            length += 1 + words[i].Length; //1 is the space
            if (length > LINE_MAX_CHARACTERS)
            {
                lineStartIndexes.Add(i);
                length = words[i].Length;
            }
        }
    }

    public void ShowText(RichText[] texts = null, System.Action onEnd = null)
    {
        if (lineStartIndexes == null) SetUpText(texts);
        StartCoroutine(ShowCharacters(richTexts, lineStartIndexes, onEnd));
    }

    public void Dissapear()
    {
        StartCoroutine(Alpha(1f, 0f, 1f));
    }

    private IEnumerator ShowCharacters(RichText[] richTexts, List<int> lineStartIndexes, System.Action onEnd)
    {
        string fullText = "";
        string text = "";
        int lineCount = 1;
        int wordCount = 0;
        //TODO ANIMATE BALLOON
        for (int i = 0; i < richTexts.Length; i++)
        {
            text = "";
            for (int j = 0; j < richTexts[i].Text.Length; j++)
            {
                if(richTexts[i].Text[j] == ' ')
                {
                    wordCount++;
                    if(lineCount < lineStartIndexes.Count && wordCount == lineStartIndexes[lineCount])
                    {
                        Debug.Log(lineStartIndexes[lineCount]);
                        lineCount++;
                        richTexts[i].ReplaceCharacter(j,'\n');
                    }
                }

                textMeshStatic.text = fullText + text;
                text = richTexts[i].GetRichText(j+1);
                timer = 0f;
                textColor.a = timer;
                renderer.material.color = textColor;
                textMesh.text = fullText + text;

                yield return new WaitForSeconds(CHARACTER_TIME);
            }

            wordCount++;
            if (lineCount < lineStartIndexes.Count && wordCount == lineStartIndexes[lineCount])
            {
                lineCount++;
                fullText += richTexts[i].GetRichText() + "\n";
            }
            else
            {
                fullText += richTexts[i].GetRichText() + " ";
            }
                
        }
        onEnd();
        yield return 0;
    }

    private IEnumerator Alpha(float from, float to, float duration)
    {
        float timer = 0f;
        float pace = 1f / duration;
        Color color = renderer.material.color;

        while (timer < 1)
        {
            timer += Time.deltaTime * pace;
            color.a = Mathf.Lerp(from, to, timer);
            renderer.material.color = color;
            staticRenderer.material.color = color;
            yield return 0f;
        }

        color.a = to;
        renderer.material.color = color;
        staticRenderer.material.color = color;
        yield return 0f;
    }
}

public class RichText
{
    public const string GREEN = "008000ff";
    public const string DARK_GREEN = "003000ff";
    public const string DARK_GRAY = "202020ff";
    public const string DARK_RED = "301010ff";
    public const string BLACK = "000000ff";
    public const string YELLOW = "AAAA00ff";
    public const string GOLDEN = "AA8800ff";
    public const string WHITE = "ffffffff";

    string text;
    string colorCode;
    bool isBold;
    bool isItalic;

    public string Text { get { return text; } }

    public RichText(string text, string colorCode = DARK_RED, bool bold = false, bool italic = false)
    {
        this.text = text;
        this.colorCode = colorCode;
        this.isBold = bold;
        this.isItalic = italic;
    }

    public string GetRichText()
    {
        return GetHeaderStart() + text + GetHeaderEnd();
    }

    public string GetRichText(int length)
    {
        return GetHeaderStart() + text.Substring(0, length) + GetHeaderEnd();
    }

    public void ReplaceCharacter(int index, char character)
    {
        text = text.Substring(0, index) + character + (index+1 < text.Length ? text.Substring(index+1, text.Length-index-1) : "");
    }

    public string GetHeaderStart()
    {
        return "<color=#"+colorCode+">" + (isBold?"<b>":"") + (isItalic ? "<i>" : "");
    }

    public string GetHeaderEnd()
    {
        return (isItalic ? "</i>" : "") + (isBold ? "</b>" : "") + "</color>";
    }

    public static implicit operator RichText(string text) => new RichText(text);

    public static implicit operator string(RichText richText) => richText.GetRichText();
}
