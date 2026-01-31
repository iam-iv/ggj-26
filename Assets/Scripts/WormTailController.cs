using UnityEngine;

/// <summary>
/// Simple procedural tail controller.
/// Assign the head transform and an ordered array of tail segment transforms (root->tip).
/// The script preserves initial distances and scales, and stretches the chain when the head moves faster.
/// </summary>
public class WormTailController : MonoBehaviour
{
    [Tooltip("Transform to follow (player head/body).")]
    public Transform head;

    [Tooltip("Ordered tail segments from nearest to head (index 0) to tip (last).")]
    public Transform[] segments;

    [Tooltip("Smoothing speed for segment movement (higher = snappier)")]
    public float smooth = 10f;

    [Tooltip("How strongly the tail stretches with head speed")]
    public float stretchSpeedFactor = 0.02f;

    [Tooltip("Maximum extra stretch multiplier (1 = no extra) e.g. 1.5 = 50% longer)")]
    public float maxStretchMultiplier = 1.5f;

    [Tooltip("If true, scale segments along their local Y with stretch amount")]
    public bool scaleSegments = true;

    float[] restDistances;
    Vector3[] initialScales;
    Vector3 lastHeadPos;

    void Start()
    {
        Initialize();
        if (head != null) lastHeadPos = head.position;
    }

    [ContextMenu("Init Tail")]
    public void Initialize()
    {
        if (segments == null || segments.Length == 0) return;

        restDistances = new float[segments.Length];
        initialScales = new Vector3[segments.Length];

        for (int i = 0; i < segments.Length; i++)
        {
            initialScales[i] = segments[i].localScale;
            Vector3 prevPos = (i == 0) ? (head != null ? head.position : segments[i].parent != null ? segments[i].parent.position : segments[i].position) : segments[i - 1].position;
            restDistances[i] = Vector3.Distance(segments[i].position, prevPos);
            if (restDistances[i] < 0.0001f) restDistances[i] = 0.1f;
        }
    }

    void LateUpdate()
    {
        if (segments == null || segments.Length == 0 || head == null) return;

        // compute head speed (world units per second)
        Vector3 headPos = head.position;
        float headSpeed = (headPos - lastHeadPos).magnitude / Mathf.Max(1e-6f, Time.deltaTime);
        lastHeadPos = headPos;

        // stretch multiplier based on speed
        float stretch = 1f + Mathf.Clamp(headSpeed * stretchSpeedFactor, 0f, maxStretchMultiplier - 1f);

        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime); // smoothing factor in [0,1]

        for (int i = 0; i < segments.Length; i++)
        {
            Vector3 prevPos = (i == 0) ? headPos : segments[i - 1].position;

            // direction from prev to this
            Vector3 dir = segments[i].position - prevPos;
            if (dir.sqrMagnitude < 1e-6f)
            {
                // if nearly coincident, pick a default backward direction in head space
                dir = -head.forward * restDistances[i];
            }

            Vector3 targetPos = prevPos + dir.normalized * (restDistances[i] * stretch);
            segments[i].position = Vector3.Lerp(segments[i].position, targetPos, t);

            // rotate segment to look toward previous (so visuals align)
            Vector3 toPrev = prevPos - segments[i].position;
            if (toPrev.sqrMagnitude > 1e-6f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPrev, Vector3.up);
                segments[i].rotation = Quaternion.Slerp(segments[i].rotation, targetRot, t);
            }

            // optionally scale segment along its local Y to emphasize stretching
            if (scaleSegments)
            {
                float curDist = Vector3.Distance(segments[i].position, prevPos);
                float scaleMult = curDist / restDistances[i];
                var s = initialScales[i];
                // apply scaleMult to Y (assumes segment sprite is oriented along Y)
                s.y = Mathf.Max(0.01f, initialScales[i].y * scaleMult);
                segments[i].localScale = s;
            }
        }
    }
}
