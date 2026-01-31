using UnityEngine;

namespace GGJ.Masks
{
    [RequireComponent(typeof(Collider))]
    public class MaskPickup : MonoBehaviour
    {
        [Tooltip("Mask Definition assigned to this pickup")]
        public MaskDefinition maskDefinition;

        [Tooltip("If true, the mask will be automatically equipped by the player on pickup")]
        public bool autoEquip = true;

        [Tooltip("Destroy the pickup GameObject after it's picked up")]
        public bool destroyOnPickup = true;

        void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (maskDefinition == null) return;

            var controller = other.GetComponentInParent<PlayerMaskController>();
            if (controller == null) return;

            var result = controller.EquipMaskWithResult(maskDefinition);
            PickupBy(controller);
        }

        /// <summary>
        /// Called to pick this mask up by a controller. External code can call this too.
        /// </summary>
        public void PickupBy(PlayerMaskController controller)
        {
            if (controller == null || maskDefinition == null) return;

            if (autoEquip)
                controller.EquipMask(maskDefinition);

            if (destroyOnPickup)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
