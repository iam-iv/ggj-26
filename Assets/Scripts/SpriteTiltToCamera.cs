using UnityEngine;

[ExecuteAlways]
public class SpriteTiltToCamera : MonoBehaviour
{
    public Camera targetCamera;
    public Vector3 eulerOffset = Vector3.zero;
    public float smooth = 10f;
    // If true, keep the object's Y (yaw) rotation unchanged.
    public bool preserveYaw = true;
    // If true, match the camera's X (pitch) and Z (roll) so the sprite plane is parallel to the camera.
    public bool matchCameraPitchAndRoll = true;

    void LateUpdate()
    {
        EnsureTargetCamera();
        if (targetCamera == null)
            return;

        if (matchCameraPitchAndRoll)
        {
            Vector3 camEuler = targetCamera.transform.eulerAngles;
            Vector3 cur = transform.eulerAngles;
            float t = Mathf.Clamp01(Time.deltaTime * smooth);

            float targetX = camEuler.x + eulerOffset.x;
            float targetZ = camEuler.z + eulerOffset.z;
            float newX = Mathf.LerpAngle(cur.x, targetX, t);
            float newZ = Mathf.LerpAngle(cur.z, targetZ, t);
            float newY = preserveYaw ? cur.y : Mathf.LerpAngle(cur.y, camEuler.y + eulerOffset.y, t);

            transform.rotation = Quaternion.Euler(newX, newY, newZ);
        }
        else
        {
            // Fallback: face camera while optionally preserving yaw
            Vector3 dir = targetCamera.transform.position - transform.position;
            if (dir.sqrMagnitude < 0.0001f)
                return;
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(eulerOffset);
            if (preserveYaw)
            {
                Vector3 trg = targetRot.eulerAngles;
                trg.y = transform.eulerAngles.y;
                targetRot = Quaternion.Euler(trg);
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Mathf.Clamp01(Time.deltaTime * smooth));
        }
    }

    void OnEnable()
    {
        // try to populate target camera earlier
        EnsureTargetCamera();
    }

    void EnsureTargetCamera()
    {
        if (targetCamera != null) return;

        // Runtime: prefer Camera.main
        if (Application.isPlaying)
        {
            if (Camera.main != null)
            {
                targetCamera = Camera.main;
                return;
            }

            // fallback: first enabled camera
            var cams = Camera.allCameras;
            if (cams != null && cams.Length > 0)
            {
                for (int i = 0; i < cams.Length; i++)
                {
                    if (cams[i] != null && cams[i].enabled)
                    {
                        targetCamera = cams[i];
                        return;
                    }
                }
            }
        }
        else
        {
            // Editor (not playing): try SceneView camera
#if UNITY_EDITOR
            var sv = UnityEditor.SceneView.lastActiveSceneView;
            if (sv != null && sv.camera != null)
            {
                targetCamera = sv.camera;
                return;
            }
#endif
            // fallback to Camera.main or first available camera in the scene
            if (Camera.main != null)
            {
                targetCamera = Camera.main;
                return;
            }
            var all = Camera.allCameras;
            if (all != null && all.Length > 0)
                targetCamera = all[0];
        }
    }
}
