using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBalloon : MonoBehaviour
{
    private const float SHAKE_TIME = 0.15f;
    
    private const int SMALL_LINE_LIMIT = 2;
    private const int MEDIUM_LINE_LIMIT = 3;
    private const int BIG_LINE_LIMIT = 4;

    private const float SMALL_HEIGHT = 1.5f;
    private const float MEDIUM_HEIGHT = 2.0f;
    private const float BIG_HEIGHT = 2.5f;

    private readonly Vector3 SMALL_ANCHOR = new Vector3(0.3f + 0.0f, 1.1f + 0.08f, -0.1f);
    private readonly Vector3 MEDIUM_ANCHOR = new Vector3(0.3f + 0.15f, 1.6f + 0.05f, -0.1f);
    private readonly Vector3 BIG_ANCHOR = new Vector3(0.3f + 0.25f, 2.1f, -0.1f);

    public Sprite smallBallonSprite;
    public Sprite mediumBallonSprite;
    public Sprite bigBallonSprite;

    private Transform firstChild;
    private Dialogue dialogue;
    private DialogueText dialogueText;
    private SpriteRenderer spriteRenderer;
    private RichText[] text;
    private Action onTextEnd;

    private enum Size { SMALL, MEDIUM, BIG};
    private Size size;
    // Start is called before the first frame update
    void Awake()
    {
        firstChild = transform.GetChild(0);
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        dialogueText = GetComponentInChildren<DialogueText>();
        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetBalloon(Dialogue dialogue, Vector3 anchor, bool leftPivot, RichText[] richText, Action onEnd)
    {
        this.dialogue = dialogue;

        //transform.position = anchor;
        //CHANGING TO RIGHT PIVOT
        //- Change texture pivot: Bottom Left > Bottom Right
        //- Add an offset position to transform
        transform.localScale = Vector3.one;
        spriteRenderer.transform.localScale = new Vector3((leftPivot ? 1f : -1f), 1f, 1f);
        if(!leftPivot)
        {
            transform.position += -transform.right * 4f;
        }

        dialogueText.SetUpText(richText);
        size = GetSize(dialogueText.LineCount);
        spriteRenderer.sprite = GetSprite(size);
        dialogueText.transform.localPosition = GetAnchor(size);

        text = richText;
        onTextEnd = onEnd;

        StartCoroutine(ShakeBalloon());
    }

    public void ShowBalloon()
    {
        StartCoroutine(Alpha(0f, 0.9f, 1f, ()=> { dialogueText.ShowText(onEnd: onTextEnd); }));
    }

    public float GetHeight()
    {
        switch (size)
        {
            case Size.SMALL:
                return SMALL_HEIGHT * dialogue.Scale;
            case Size.MEDIUM:
                return MEDIUM_HEIGHT * dialogue.Scale;
            case Size.BIG:
                return BIG_HEIGHT * dialogue.Scale;
            default:
                return BIG_HEIGHT * dialogue.Scale;
        }
    }

    public void Move(Vector3 offsetPosition)
    {
        StartCoroutine(Move(transform.localPosition, transform.localPosition + offsetPosition, 1f));
    }

    public void Dissapear()
    {
        Debug.Log("Dissapear");
        StartCoroutine(Alpha(1f, 0f, 1f));
        dialogueText.Dissapear();
    }

    private IEnumerator ShakeBalloon()
    {
        while (true)
        {
            Vector3 newPos = UnityEngine.Random.insideUnitCircle * dialogue.Scale * 0.02f;
            firstChild.localPosition = newPos;
            yield return new WaitForSeconds(SHAKE_TIME);
        }
    }

    private IEnumerator Move(Vector3 from, Vector3 to, float duration)
    {
        float timer = 0f;
        float pace = 1f / duration;

        while (timer < 1)
        {
            timer += Time.deltaTime * pace;
            transform.localPosition = Vector3.Lerp(from, to, timer);
            yield return 0f;
        }

        transform.localPosition = to;
        yield return 0f;
    }

    private IEnumerator Alpha(float from, float to, float duration, Action doAfter = null)
    {
        float timer = 0f;
        float pace = 1f / duration;
        Color color = spriteRenderer.color;

        while(timer < 1)
        {
            timer += Time.deltaTime * pace;
            color.a = Mathf.Lerp(from, to, timer);
            spriteRenderer.color = color;
            yield return 0f;
        }

        color.a = to;
        spriteRenderer.color = color;
        doAfter?.Invoke();
        yield return 0f;
    }

    private Size GetSize(int lineCount)
    {
        if (lineCount <= SMALL_LINE_LIMIT)
            return Size.SMALL;
        if (lineCount <= MEDIUM_LINE_LIMIT)
            return Size.MEDIUM;
        else
            return Size.BIG;
    }

    private Sprite GetSprite(Size size)
    {
        switch (size)
        {
            case Size.SMALL:
                return smallBallonSprite;
            case Size.MEDIUM:
                return mediumBallonSprite;
            case Size.BIG:
                return bigBallonSprite;
            default:
                return bigBallonSprite;
        }
    }

    private Vector3 GetAnchor(Size size)
    {
        switch (size)
        {
            case Size.SMALL:
                return SMALL_ANCHOR;
            case Size.MEDIUM:
                return MEDIUM_ANCHOR;
            case Size.BIG:
                return BIG_ANCHOR;
            default:
                return BIG_ANCHOR;
        }
    }
}
