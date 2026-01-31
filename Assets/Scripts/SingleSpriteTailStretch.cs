using UnityEngine;

/// <summary>
/// Stretch a single sprite to simulate a tail stretching/contracting with movement.
/// IMPORTANT: For predictable results, make the sprite Transform a child of the attach point
/// and set the sprite's pivot where you want it anchored (e.g. bottom). The script provides
/// an `anchorPivot` (0=bottom, 0.5=center, 1=top) to offset the sprite so the chosen anchor
/// stays in place when scaling.
/// </summary>
[ExecuteAlways]
public class SingleSpriteTailStretch : MonoBehaviour
{
    [Tooltip("Point to attach the sprite to (usually a child/empty on the player)")]
    public Transform attachPoint;

    [Tooltip("The transform that has the SpriteRenderer to scale (should be child of attachPoint)")]
    public Transform spriteTransform;

    [Tooltip("How strongly the tail stretches with head speed")]
    public float stretchSpeedFactor = 0.01f;

    [Tooltip("Maximum extra stretch multiplier (1 = no extra) e.g. 1.5 = 50% longer)")]
    public float maxStretchMultiplier = 1.5f;

    [Tooltip("Smoothing speed for scale/position changes")]
    public float smooth = 8f;

    [Tooltip("Anchor pivot along sprite height: 0 = bottom anchored to attachPoint, 0.5 = center, 1 = top anchored")] 
    [Range(0f,1f)]
    public float anchorPivot = 0f;

    [Header("Oscillation")]
    [Tooltip("Enable cyclic oscillation on top of stretch")]
    public bool oscillate = false;

    [Tooltip("Relative amplitude of oscillation (e.g. 0.1 = ±10% scale) ")]
    public float oscAmplitude = 0.08f;

    [Tooltip("Oscillation frequency in Hz")]
    public float oscFrequency = 2f;

    [Tooltip("If true, oscillation amplitude scales with movement speed")]
    public bool oscScaleWithSpeed = true;

    float oscPhase = 0f;

    SpriteRenderer sr;
    Vector3 initialLocalScale;
    Vector3 initialLocalPos;
    float restLength;
    Vector3 lastAttachPos;

    void Start()
    {
        Initialize();
    }

    void OnValidate()
    {
        // keep inspector changes reflected
        if (Application.isPlaying) Initialize();
    }

    public void Initialize()
    {
        if (spriteTransform == null && attachPoint != null && attachPoint.childCount > 0)
            spriteTransform = attachPoint.GetChild(0);

        if (spriteTransform == null) return;
        sr = spriteTransform.GetComponent<SpriteRenderer>();
        initialLocalScale = spriteTransform.localScale;
        initialLocalPos = spriteTransform.localPosition;

        if (sr != null && sr.sprite != null)
        {
            restLength = sr.sprite.bounds.size.y * initialLocalScale.y;
        }
        else
        {
            // fallback: use world distance from sprite to attach point
            restLength = Vector3.Distance(spriteTransform.position, attachPoint != null ? attachPoint.position : transform.position);
        }

        lastAttachPos = attachPoint != null ? attachPoint.position : transform.position;
    }

    void LateUpdate()
    {
        if (spriteTransform == null || attachPoint == null) return;

        // compute attach point speed
        Vector3 attachPos = attachPoint.position;
        float speed = (attachPos - lastAttachPos).magnitude / Mathf.Max(1e-6f, Time.deltaTime);
        lastAttachPos = attachPos;

        float stretch = 1f + Mathf.Clamp(speed * stretchSpeedFactor, 0f, maxStretchMultiplier - 1f);

        // update oscillation phase
        if (oscillate)
        {
            oscPhase += (Mathf.PI * 2f) * oscFrequency * Time.deltaTime;
            if (oscPhase > Mathf.PI * 2f) oscPhase -= Mathf.PI * 2f;
        }

        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);

        // base scale along local Y
        float baseScaleY = initialLocalScale.y * stretch;

        // apply extra oscillation multiplier if enabled
        if (oscillate)
        {
            float amp = oscAmplitude;
            if (oscScaleWithSpeed)
                amp *= Mathf.Clamp01(speed * stretchSpeedFactor * 10f); // boost amp proportionally to speed
            float osc = 1f + amp * Mathf.Sin(oscPhase);
            baseScaleY *= osc;
        }

        Vector3 targetScale = new Vector3(initialLocalScale.x, baseScaleY, initialLocalScale.z);
        spriteTransform.localScale = Vector3.Lerp(spriteTransform.localScale, targetScale, t);

        // compute length delta in world units
        float curLength = (sr != null && sr.sprite != null) ? sr.sprite.bounds.size.y * spriteTransform.localScale.y : restLength * stretch;
        float delta = curLength - restLength;

        // move sprite so the anchorPivot point remains at the attachPoint
        // we assume spriteTransform is direct child of attachPoint for simpler math
        Vector3 localOffset = Vector3.up * (delta * anchorPivot);
        Vector3 targetLocalPos = initialLocalPos + localOffset;
        spriteTransform.localPosition = Vector3.Lerp(spriteTransform.localPosition, targetLocalPos, t);
    }
}
