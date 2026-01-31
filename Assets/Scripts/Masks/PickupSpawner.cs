using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Masks
{
    /// <summary>
    /// Simple spawner for mask pickups. Supports optional pooling.
    /// Configure mapping of MaskType -> prefab in the Inspector.
    /// </summary>
    public class PickupSpawner : MonoBehaviour
    {
        public static PickupSpawner Instance { get; private set; }

        [System.Serializable]
        public struct Entry { public MaskType type; public GameObject prefab; }

        [Tooltip("Map MaskType to pickup prefab (asset from Project)")]
        public Entry[] entries;

        [Tooltip("If true, pickups will be pooled (SetActive(false)/reuse). If false, they will be instantiated and destroyed normally.")]
        public bool usePooling = false;

        // internal pools per type
        Dictionary<MaskType, Queue<GameObject>> pools = new Dictionary<MaskType, Queue<GameObject>>();
        Dictionary<MaskType, GameObject> prefabLookup = new Dictionary<MaskType, GameObject>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildLookup();
        }

        void OnValidate() => BuildLookup();

        void BuildLookup()
        {
            prefabLookup.Clear();
            if (entries == null) return;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].prefab != null)
                    prefabLookup[entries[i].type] = entries[i].prefab;
            }
        }

        public bool HasPrefabFor(MaskType type) => prefabLookup.ContainsKey(type) && prefabLookup[type] != null;

        public GameObject Spawn(MaskType type, Vector3 position, Quaternion rotation)
        {
            if (!HasPrefabFor(type)) return null;

            if (usePooling)
            {
                if (!pools.TryGetValue(type, out var q))
                {
                    q = new Queue<GameObject>();
                    pools[type] = q;
                }

                if (q.Count > 0)
                {
                    var go = q.Dequeue();
                    go.transform.SetPositionAndRotation(position, rotation);
                    go.SetActive(true);
                    return go;
                }
            }

            var prefab = prefabLookup[type];
            var obj = Instantiate(prefab, position, rotation);
            return obj;
        }

        /// <summary>
        /// Return a spawned pickup to the pool (if pooling enabled) or destroy it.
        /// Call this from pickup logic instead of Destroy(gameObject).
        /// </summary>
        public void Return(GameObject pickup, MaskType? type = null)
        {
            if (pickup == null) return;

            var t = type ?? GetMaskTypeFromPickup(pickup);
            if (usePooling && t != null && HasPrefabFor(t.Value))
            {
                pickup.SetActive(false);
                if (!pools.TryGetValue(t.Value, out var q))
                {
                    q = new Queue<GameObject>();
                    pools[t.Value] = q;
                }
                q.Enqueue(pickup);
            }
            else
            {
                Destroy(pickup);
            }
        }

        MaskType? GetMaskTypeFromPickup(GameObject pickup)
        {
            var mp = pickup.GetComponent<MaskPickup>();
            if (mp != null && mp.maskDefinition != null) return mp.maskDefinition.type;
            return null;
        }
    }
}
