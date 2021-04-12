using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Symbols/Character", order = 1)]
public class Character : ScriptableObject
{
    private const float WIDTH = 1f;
    private const float HEIGHT = 1f;
    public Stroke[] strokes;
    public string originalCharacter;
    public string mean;

    public int NumberOfStrokes { get { return strokes.Length; } }

    public static Stroke[] Normalize(Stroke[] strokes)
    {
        float bottom = float.MaxValue;
        float top = float.MinValue;
        float left = float.MaxValue;
        float right = float.MinValue; ;
        Stroke[] normalized = new Stroke[strokes.Length];

        for (int i = 0; i < strokes.Length; i++)
        {
            left = Mathf.Min(left, strokes[i].start.x, strokes[i].end.x);
            bottom = Mathf.Min(bottom, strokes[i].start.y, strokes[i].end.y);
            right = Mathf.Max(right, strokes[i].start.x, strokes[i].end.x);
            top = Mathf.Max(top, strokes[i].start.y, strokes[i].end.y);
        }
        Debug.Log(string.Format("L: {0}, R: {1}, B: {2}, T: {3}", left, right, bottom, top));

        Vector2 offset = new Vector2(left, bottom);
        Vector2 scale = new Vector2(WIDTH / (right - left), HEIGHT / (top - bottom));

        for (int i = 0; i < normalized.Length; i++)
        {
            normalized[i] = strokes[i].Normalize(offset, scale);
        }

        return normalized;
    }

    public static void CreateCharacterReference(Stroke[] strokes)
    {
#if UNITY_EDITOR
        Character asset = ScriptableObject.CreateInstance<Character>();
        asset.strokes = strokes;

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/NewCharacter.asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
#endif
    }
}

[System.Serializable]
public class Stroke
{
    public Vector2 start, end;

    public Vector2 Direction { get { return (end - start).normalized; } }
    public float Length { get { return Vector2.Distance(end, start); } }

    public Stroke(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }

    public Stroke Normalize(Vector2 zeroPointOffset, Vector2 scale)
    {
        Vector2 newStart = Vector2.Scale(start - zeroPointOffset, scale);
        Vector2 newEnd = Vector2.Scale(end - zeroPointOffset, scale);
        return new Stroke(newStart, newEnd);
    }

    public override string ToString()
    {
        return string.Format("[Stroke - start : {0},{1} | end : {2},{3}", start.x, start.y, end.x, end.y);
    }
}
