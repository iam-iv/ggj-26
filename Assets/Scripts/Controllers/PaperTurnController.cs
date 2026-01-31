using System.Collections;
using UnityEngine;

public class PaperTurnController : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Total duration of the flip animation in seconds")]
    [SerializeField] private float flipDuration = 0.22f;

    [Header("Visuals")]
    [Tooltip("Amount of vertical squash during the flip (0 = none, 0.2 = 20% taller)")]
    [SerializeField] private float squashAmount = 0.15f;

    [Tooltip("Easing curve for the flip (evaluated 0..1). Default is ease-in-out.")]
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool facingRight = true;
    private Coroutine flipRoutine;
    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
        if (Mathf.Approximately(baseScale.x, 0f))
            baseScale.x = 1f;
    }

    public void SetFacing(bool faceRight)
    {
        if (faceRight == facingRight)
            return;
        facingRight = faceRight;
        StartFlip();
    }

    public void ToggleFacing()
    {
        SetFacing(!facingRight);
    }

    private void StartFlip()
    {
        if (flipRoutine != null)
            StopCoroutine(flipRoutine);
        flipRoutine = StartCoroutine(FlipCoroutine());
    }

    private IEnumerator FlipCoroutine()
    {
        float half = Mathf.Max(0.001f, flipDuration * 0.5f);
        Vector3 s = baseScale;

        // First half: X -> 0, Y -> taller (squash effect)
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float k = ease.Evaluate(Mathf.Clamp01(t / half));
            float newX = Mathf.Lerp(s.x, 0f, k);
            float newY = Mathf.Lerp(s.y, s.y * (1f + squashAmount), k);
            transform.localScale = new Vector3(newX, newY, s.z);
            yield return null;
        }

        // Snap X to 0 before expanding back and flip sign
        float targetX = Mathf.Abs(baseScale.x) * (facingRight ? 1f : -1f);
        transform.localScale = new Vector3(0f, s.y * (1f + squashAmount), s.z);

        // Second half: X 0 -> targetX, Y -> base
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float k = ease.Evaluate(Mathf.Clamp01(t / half));
            float newX = Mathf.Lerp(0f, targetX, k);
            float newY = Mathf.Lerp(s.y * (1f + squashAmount), baseScale.y, k);
            transform.localScale = new Vector3(newX, newY, s.z);
            yield return null;
        }

        transform.localScale = new Vector3(targetX, baseScale.y, s.z);
        flipRoutine = null;
    }
}
