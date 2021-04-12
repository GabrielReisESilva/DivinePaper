using UnityEngine;
using System.Collections.Generic;
[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class Bezier : MonoBehaviour
{
    public Transform endPointMark;
    public Transform preview;

    public Vector3 EndPoint
    {
        get { return controlPoints[2]; }
    }

    public float ExtensionFactor
    {
        set { extensionFactor = value; }
    }

    private Renderer previewRenderer;
    private float extensionFactor;
    private Vector3[] controlPoints;
    private Vector3 cameraPosition;
    private LineRenderer lineRenderer;
    private float extendStep;
    private int SEGMENT_COUNT = 30;

    void Start()
    {
        controlPoints = new Vector3[3];
        lineRenderer = GetComponent<LineRenderer>();
        previewRenderer = preview.GetComponentInChildren<Renderer>();
        extendStep = 5f;
        extensionFactor = 0f;

        ToggleDraw(false);
    }

    void Update()
    {
        if (!lineRenderer.enabled)
            return;

        UpdateControlPoints();
        HandleExtension();
        DrawCurve();
        UpdatePreview();
        endPointMark.position = EndPoint;
        endPointMark.rotation = Quaternion.identity;
    }

    public void ToggleDraw(bool draw, Texture2D previewTexture = null, Vector3 cameraPosition = default(Vector3))
    {
        lineRenderer.enabled = draw;
        //If using another renderer, we can change only the texture
        if (draw)
        {
            previewRenderer.material.mainTexture = previewTexture;
            this.cameraPosition = cameraPosition;
            preview.rotation = Quaternion.LookRotation(cameraPosition - preview.position, Vector3.up);
        }
        endPointMark.gameObject.SetActive(draw);
    }

    void HandleExtension()
    {
        if (extensionFactor == 0f)
            return;

        float finalExtension = extendStep + Time.deltaTime * extensionFactor * 2f;
        extendStep = Mathf.Clamp(finalExtension, 2.5f, 7.5f);
    }

    // The first control is the remote. The second is a forward projection. The third is a forward and downward projection.
    void UpdateControlPoints()
    {
        float distance = Mathf.Pow(2f + 3f * gameObject.transform.forward.y, 2f); 
        controlPoints[0] = gameObject.transform.position; // Get Controller Position
        controlPoints[1] = controlPoints[0] + (gameObject.transform.forward * distance * 0.5f);
        controlPoints[2] = controlPoints[1] + (gameObject.transform.forward * distance); controlPoints[2].y = -2; //ground
        //controlPoints[1] = controlPoints[0] + (gameObject.transform.forward * extendStep * 2f / 5f);
        //controlPoints[2] = controlPoints[1] + (gameObject.transform.forward * extendStep * 3f / 5f) + Vector3.up * -1f;
    }


    // Draw the bezier curve.
    void DrawCurve()
    {
        if (!lineRenderer.enabled)
            return;
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, controlPoints[0]);

        Vector3 prevPosition = controlPoints[0];
        Vector3 nextPosition = prevPosition;
        for (int i = 1; i <= SEGMENT_COUNT; i++)
        {
            float t = i / (float)SEGMENT_COUNT;
            lineRenderer.positionCount = i + 1;

            if (i == SEGMENT_COUNT)
            { // For the last point, project out the curve two more meters.
                //Vector3 endDirection = Vector3.Normalize(prevPosition - lineRenderer.GetPosition(i - 2));
                nextPosition = controlPoints[2];
            }
            else
            {
                nextPosition = CalculateBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2]);
            }
            lineRenderer.SetPosition(i, nextPosition);
            prevPosition = nextPosition;
            /*
            if (CheckColliderIntersection(prevPosition, nextPosition))
            { // If the segment intersects a surface, draw the point and return.
                lineRenderer.SetPosition(i, endpoint);
                endPointDetected = true;
                return;
            }
            else
            { // If the point does not intersect, continue to draw the curve.
                lineRenderer.SetPosition(i, nextPosition);
                endPointDetected = false;
                prevPosition = nextPosition;
            }
            */
        }
    }
    /*
    // Check if the line between start and end intersect a collider.
    bool CheckColliderIntersection(Vector3 start, Vector3 end)
    {
        Ray r = new Ray(start, end - start);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, Vector3.Distance(start, end), 1 << LayerMask.NameToLayer("Ground")))
        {
            endpoint = hit.point;
            return true;
        }

        return false;
    }
    */

    void UpdatePreview()
    {
        preview.rotation = Quaternion.LookRotation(cameraPosition - preview.position, Vector3.up);
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return
            Mathf.Pow((1f - t), 2) * p0 +
            2f * (1f - t) * t * p1 +
            Mathf.Pow(t, 2) * p2;
    }
}