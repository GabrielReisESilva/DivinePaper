using System;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public DialogueBalloon dialogueBalloonPrefab;

    private int currentBalloon;
    private Vector3 anchor;
    private bool leftPivot; 
    private DialogueBalloon[] dialogueBalloons;
    private int[] pauses;
    private Action[] pauseCallback;
    private bool paused;

    public float Scale { get { return transform.localScale.x; } }
    // Start is called before the first frame update
    void Start()
    {
        //StartDialogue(transform.position, true, texts);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartDialogue(Vector3 facingTarget, Vector3 anchor, bool leftPivot, RichText[][] balloonTexts, int[] pauses = null, Action[] onPauseCallback = null)
    {
        transform.position = anchor;
        Quaternion rotation = Quaternion.FromToRotation(facingTarget, anchor);
        transform.LookAt(facingTarget);
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y + 180f, 0f);

        currentBalloon = 0;
        this.leftPivot = leftPivot;
        this.pauses = pauses;
        dialogueBalloons = new DialogueBalloon[balloonTexts.Length];
        for (int i = 0; i < dialogueBalloons.Length; i++)
        {
            dialogueBalloons[i] = Instantiate(dialogueBalloonPrefab, transform) as DialogueBalloon;
            dialogueBalloons[i].SetBalloon(this, anchor, leftPivot, balloonTexts[i], NextBalloon);
        }
        pauseCallback = onPauseCallback;
        if (pauses != null && onPauseCallback != null && onPauseCallback.Length != pauses.Length + 1)
            Debug.LogError("Number of callbacks don't match");

        ShowBalloon(0);
    }

    public void Resume(int index)
    {
        if (!paused) return;
        if (currentBalloon != index) return;

        if (currentBalloon >= dialogueBalloons.Length)
        {
            if(pauseCallback != null)
                pauseCallback[pauseCallback.Length-1]?.Invoke();
            return;
        }

        AnimateBalloons();
        ShowBalloon(currentBalloon);
    }

    private void ShowBalloon(int index)
    {
        dialogueBalloons[currentBalloon].ShowBalloon();
    }

    private void NextBalloon()
    {
        currentBalloon++;
        if (Contains(pauses, currentBalloon))
        {
            paused = true;
            return;
        }

        if (currentBalloon >= dialogueBalloons.Length)
        {
            pauseCallback[pauseCallback.Length - 1]?.Invoke();
            return;
        }

        //WAIT SOME TIME

        AnimateBalloons();
        ShowBalloon(currentBalloon);
    }

    private void AnimateBalloons()
    {
        float height = dialogueBalloons[currentBalloon].GetHeight();

        for (int i = 0; i < currentBalloon; i++)
        {
            if ((currentBalloon - 1 - i) % 2 == 0)
                dialogueBalloons[i].Move(new Vector3((leftPivot ? 1f : -1f) * 2.0f, height, 2f));
            else
                dialogueBalloons[i].Move(new Vector3((leftPivot ? 1f : -1f) * 3.0f, 0.8f * height, 2f));
            if (i == (currentBalloon - 4)) dialogueBalloons[i].Dissapear();
        }
    }

    private bool Contains(int[] array, int value)
    {
        if (pauses == null) return false;

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == value)
            {
                pauseCallback[i]();
                return true;
            }
        }
        return false;
    }
}
