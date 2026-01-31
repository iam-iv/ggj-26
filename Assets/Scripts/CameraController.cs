using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform of the player to follow (follows only X axis).")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [Tooltip("Higher = snappier following")]
    [SerializeField] private float followSpeed = 6f;

    [Header("Camera Setup")]
    [Tooltip("Fixed Y (height) of the camera in world units")]
    [SerializeField] private float fixedHeight = 10f;

    [Tooltip("Whether the script enforces orthographic and 45° X rotation on the camera")]
    [SerializeField] private bool enforceOrthoAndRotation = true;

    private Vector3 initialPosition;

    void Awake()
    {
        initialPosition = transform.position;

        if (enforceOrthoAndRotation)
        {
            var cam = GetComponent<Camera>();
            if (cam != null)
                cam.orthographic = true;

            transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        }

        // Auto-assign target if not set: look for a GameObject tagged "Player"
        if (target == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null)
                target = go.transform;
            else
                Debug.LogWarning("CameraController: no GameObject with tag 'Player' found to auto-assign target.");
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Desired position: follow only the target's X, keep fixed height and original Z
        Vector3 desired = new Vector3(target.position.x, fixedHeight, initialPosition.z);

        // Smoothly interpolate to the desired position
        transform.position = Vector3.Lerp(transform.position, desired, Mathf.Clamp01(followSpeed * Time.deltaTime));
    }
}
