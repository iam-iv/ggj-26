using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Masks
{
    [DisallowMultipleComponent]
    public class MaskVisualController : MonoBehaviour
    {
        [Tooltip("Reference to the PlayerMaskController to observe (auto-find if empty)")]
        public PlayerMaskController playerMaskController;

        [Tooltip("Transform where the mask visual will be attached; defaults to this GameObject")] 
        public Transform attachPoint;

        [Serializable]
        public struct MaskEntry
        {
            public MaskType type;
            public GameObject prefab;
            public Vector3 localPosition;
            public Vector3 localEuler;
            public Vector3 localScale;
        }

        [Tooltip("Map MaskType to a prefab visual (e.g. stick + sprite)")]
        public MaskEntry[] entries;

        [Tooltip("Optional: MaskDefinition to use for quick testing in the Inspector/ContextMenu")]
        public MaskDefinition testMask;

        [Header("Spawn Options")]
        [Tooltip("If set, assign this sorting layer name to spawned SpriteRenderers/Canvases (leave empty to keep prefab values)")]
        public string forceSortingLayerName = "";

        [Tooltip("Offset to add to spawned SpriteRenderer/Canvas sortingOrder (0 to keep prefab values)")]
        public int forceSortingOrderOffset = 0;

        [Tooltip("If true, set the spawned instance and its children to the same layer as the attachPoint")] 
        public bool forceLayerToAttach = true;

        [Tooltip("Minimum local scale applied to spawned visuals to avoid invisible zero-scale prefabs")] 
        public Vector3 minScale = new Vector3(0.01f, 0.01f, 0.01f);

        GameObject currentInstance;
        MaskType currentType = MaskType.None;

        // Cache instantiated visuals so we can reuse (activate/deactivate) instead of destroying them
        Dictionary<MaskType, GameObject> pooledInstances = new Dictionary<MaskType, GameObject>();

        void Awake()
        {
            if (playerMaskController == null)
                playerMaskController = GetComponentInParent<PlayerMaskController>();

            if (attachPoint == null)
                attachPoint = transform;

            if (playerMaskController != null)
            {
                playerMaskController.OnMaskEquipped += HandleEquipped;
                playerMaskController.OnMaskUnequipped += HandleUnequipped;
                playerMaskController.OnMaskExpired += HandleUnequipped;
            }
        }

        // void OnDestroy()
        // {
        //     if (playerMaskController != null)
        //     {
        //         playerMaskController.OnMaskEquipped -= HandleEquipped;
        //         playerMaskController.OnMaskUnequipped -= HandleUnequipped;
        //         playerMaskController.OnMaskExpired -= HandleUnequipped;
        //     }
        // }

        void HandleEquipped(MaskDefinition def)
        {
            SpawnVisualFor(def);
        }

        void HandleUnequipped(MaskDefinition def)
        {
            RemoveVisual();
        }

        void SpawnVisualFor(MaskDefinition def)
        {
            RemoveVisual();
            if (def == null)
            {
                Debug.Log("MaskVisualController: SpawnVisualFor called with null definition", this);
                return;
            }

            Debug.Log($"MaskVisualController: Spawning visual for mask {def.type}", this);

            MaskEntry? entry = FindEntry(def.type);
            if (entry == null)
            {
                Debug.LogWarning($"MaskVisualController: No entry found for mask type {def.type}", this);
                return;
            }

            if (entry.Value.prefab == null)
            {
                Debug.LogWarning($"MaskVisualController: Entry for {def.type} has no prefab assigned", this);
                return;
            }

            currentInstance = GetOrCreateInstance(entry.Value, def.type);
    #if UNITY_EDITOR
                // In edit mode register undo so the spawned object can be undone
                if (!Application.isPlaying && currentInstance != null)
                {
                    try
                    {
                        UnityEditor.Undo.RegisterCreatedObjectUndo(currentInstance, "Spawn Mask Visual");
                        UnityEditor.EditorUtility.SetDirty(currentInstance);
                    }
                    catch { }
                }
    #endif
            if (currentInstance == null)
            {
                Debug.LogError($"MaskVisualController: failed to create visual for {def.type}", this);
                return;
            }

            currentInstance.transform.SetParent(attachPoint, false);
            currentInstance.transform.localPosition = entry.Value.localPosition;
            currentInstance.transform.localEulerAngles = entry.Value.localEuler;
            currentInstance.transform.localScale = entry.Value.localScale;
            // Ensure scale isn't effectively zero
            var ls = currentInstance.transform.localScale;
            ls.x = Mathf.Abs(ls.x) < minScale.x ? minScale.x : ls.x;
            ls.y = Mathf.Abs(ls.y) < minScale.y ? minScale.y : ls.y;
            ls.z = Mathf.Abs(ls.z) < minScale.z ? minScale.z : ls.z;
            currentInstance.transform.localScale = ls;
            currentInstance.SetActive(true);

            // (GetOrCreateInstance already disabled pickup/colliders when first created)

            // Force layer to attachPoint layer if requested
            if (forceLayerToAttach && attachPoint != null)
            {
                var layer = attachPoint.gameObject.layer;
                var allTransforms = currentInstance.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTransforms)
                    t.gameObject.layer = layer;
            }

            var cols2D = currentInstance.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in cols2D)
                c.enabled = false;


            // Apply sorting layer/order overrides for SpriteRenderers
            var srs = currentInstance.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in srs)
            {
                if (!string.IsNullOrEmpty(forceSortingLayerName))
                    sr.sortingLayerName = forceSortingLayerName;
                if (forceSortingOrderOffset != 0)
                    sr.sortingOrder += forceSortingOrderOffset;
                sr.enabled = true;
            }
            // Ensure renderers inside are enabled so they become visible
            var rends = currentInstance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
                r.enabled = true;

            // Apply overrides and enable canvases
            var canvases = currentInstance.GetComponentsInChildren<Canvas>(true);
            foreach (var c in canvases)
            {
                if (!string.IsNullOrEmpty(forceSortingLayerName))
                    c.sortingLayerName = forceSortingLayerName;
                if (forceSortingOrderOffset != 0)
                    c.sortingOrder += forceSortingOrderOffset;
                c.overrideSorting = true;
                c.enabled = true;
            }

            Debug.Log($"MaskVisualController: Spawned instance '{currentInstance.name}' for {def.type}", currentInstance);

            currentType = def.type;
        }

        void RemoveVisual()
        {
            if (currentInstance == null) return;

            Debug.Log($"MaskVisualController: Removing visual '{currentInstance.name}'", this);
            // Instead of destroying, just deactivate the visual so it can be reused later.
            currentInstance.SetActive(false);
            currentInstance.transform.SetParent(null);
            currentInstance = null;
            currentType = MaskType.None;
        }

        GameObject GetOrCreateInstance(MaskEntry entry, MaskType type)
        {
            if (pooledInstances.TryGetValue(type, out var inst) && inst != null)
            {
                inst.SetActive(true);
                return inst;
            }

            var go = Instantiate(entry.prefab, attachPoint, false);
            if (go == null) return null;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                try
                {
                    UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Spawn Mask Visual");
                    UnityEditor.EditorUtility.SetDirty(go);
                }
                catch { }
            }
#endif

            // Disable pickup/colliders on the spawned visual so it doesn't retrigger pickups
            var pickupsChild = go.GetComponentsInChildren<MaskPickup>(true);
            foreach (var p in pickupsChild)
                p.enabled = false;

            var cols = go.GetComponentsInChildren<Collider>(true);
            foreach (var c in cols)
                c.enabled = false;

            var cols2D = go.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in cols2D)
                c.enabled = false;

            pooledInstances[type] = go;
            return go;
        }

        [ContextMenu("Spawn Current Mask Visual")]
        void ContextSpawnCurrent()
        {
            if (playerMaskController == null)
                playerMaskController = GetComponentInParent<PlayerMaskController>();

            if (playerMaskController == null)
            {
                Debug.LogWarning("MaskVisualController: no PlayerMaskController found to spawn from", this);
                return;
            }

            var def = playerMaskController.CurrentMaskDefinition;
            if (def == null)
            {
                if (testMask != null)
                {
                    Debug.Log("MaskVisualController: no current mask — spawning testMask", this);
                    SpawnVisualFor(testMask);
                }
                else
                {
                    Debug.LogWarning("MaskVisualController: no mask equipped and no testMask assigned", this);
                }
            }
            else
            {
                SpawnVisualFor(def);
            }
        }

        MaskEntry? FindEntry(MaskType type)
        {
            if (entries == null) return null;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].type == type)
                    return entries[i];
            }
            return null;
        }
    }
}
