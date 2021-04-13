using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TextRecognition : MonoBehaviour
{
    private const string URL = "";
    private const string API_KEY = "";

    //public int requestedWidth = 640;
    //public int requestedHeight = 480;
    public Texture2D sample;
    private FeatureType featureType = FeatureType.TEXT_DETECTION;
    private int maxResults = 10;

    Dictionary<string, string> headers;

    [System.Serializable]
    public class TextRecognitionRequests
    {
        public List<TextRecognitionRequest> requests;
    }

    [System.Serializable]
    public class TextRecognitionRequest
    {
        public Image image;
        public List<Feature> features;
        public ImageContext imageContext;
    }

    [System.Serializable]
    public class Image
    {
        public string content;
    }

    [System.Serializable]
    public class Feature
    {
        public string type;
        public int maxResults;
    }

    [System.Serializable]
    public class ImageContext
    {
        public List<string> languageHints;
    }

    [System.Serializable]
    public class TextRecognitionResponses
    {
        public List<TextRecognitionResponse> responses;
    }

    [System.Serializable]
    public class TextRecognitionResponse
    {
        public List<TextAnnotation> textAnnotations;
    }

    [System.Serializable]
    public class TextAnnotation
    {
        public string locale;
        public string description;
        public BoundingPoly boundingPoly;
    }

    [System.Serializable]
    public class BoundingPoly
    {
        public List<Vertex> vertices;
    }

    [System.Serializable]
    public class Vertex
    {
        public float x;
        public float y;
    }

    public enum FeatureType
    {
        TYPE_UNSPECIFIED,
        FACE_DETECTION,
        LANDMARK_DETECTION,
        LOGO_DETECTION,
        LABEL_DETECTION,
        TEXT_DETECTION,
        DOCUMENT_TEXT_DETECTION,
        SAFE_SEARCH_DETECTION,
        IMAGE_PROPERTIES
    }

    // Use this for initialization
    void Start()
    {
        headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json; charset=UTF-8" }
        };
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Recognize(Texture2D image, Action<string> serverCallback)
    {
        StartCoroutine(Capture(image, serverCallback));
    }

    private IEnumerator Capture(Texture2D image, Action<string> serverCallback)
    {
        if (sample != null)
            image = sample;
        byte[] jpg = image.EncodeToJPG();
        string base64 = Convert.ToBase64String(jpg);

        TextRecognitionRequests requests = new TextRecognitionRequests();
        requests.requests = new List<TextRecognitionRequest>();

        TextRecognitionRequest request = new TextRecognitionRequest();
        request.image = new Image();
        request.image.content = base64;
        request.features = new List<Feature>();
        request.imageContext = new ImageContext();
        request.imageContext.languageHints = new List<string>(){ "zh", "zh-CN", "ja"};

        Feature feature = new Feature();
        feature.type = this.featureType.ToString();
        feature.maxResults = this.maxResults;

        request.features.Add(feature);

        requests.requests.Add(request);

        string jsonData = JsonUtility.ToJson(requests, false);
        if (jsonData != string.Empty)
        {
            string url = URL + API_KEY;
            byte[] postData = System.Text.Encoding.Default.GetBytes(jsonData);
            using (WWW www = new WWW(url, postData, headers))
            {
                yield return www;
                if (string.IsNullOrEmpty(www.error))
                {
                    Debug.Log(www.text.Replace("\n", "").Replace(" ", ""));
                    TextRecognitionResponses responses = JsonUtility.FromJson<TextRecognitionResponses>(www.text);
                    // SendMessage, BroadcastMessage or someting like that.
                    if (responses.responses.Count > 0)
                        if(responses.responses[0].textAnnotations.Count > 0)
                            serverCallback(responses.responses[0].textAnnotations[0].description);
                }
                else
                {
                    Debug.Log("Error: " + www.error);
                }
            }
        }
    }
}
