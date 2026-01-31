using System;
using UnityEngine;

namespace GGJ.Masks
{
    public class PlayerMaskController : MonoBehaviour
    {
        [Header("Optional: start with a mask")]
        [SerializeField]
        MaskDefinition startingMask;

        MaskDefinition currentMaskDefinition;
        float remainingTime;

        public MaskDefinition CurrentMaskDefinition => currentMaskDefinition;
        public MaskType CurrentMaskType => currentMaskDefinition != null ? currentMaskDefinition.type : MaskType.None;
        public float RemainingTime => float.IsInfinity(remainingTime) ? -1f : remainingTime;
        public bool IsMaskActive => currentMaskDefinition != null;

        public event Action<MaskDefinition> OnMaskEquipped;
        public event Action<MaskDefinition> OnMaskUnequipped;
        public event Action<MaskDefinition> OnMaskExpired;

        public enum MaskEquipResult
        {
            NoOp,
            EquippedNew,
            ReplacedExisting,
            RefreshedTimer,
            Unequipped
        }

        void Start()
        {
            if (startingMask != null)
                EquipMask(startingMask);
        }

        void Update()
        {
            if (currentMaskDefinition == null) return;

            if (float.IsInfinity(remainingTime)) return; // infinite duration

            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                var expired = currentMaskDefinition;
                UnequipMask();
                OnMaskExpired?.Invoke(expired);
            }
        }

        public void EquipMask(MaskDefinition mask)
        {
            if (mask == null)
            {
                UnequipMask();
                return;
            }

            if (currentMaskDefinition == mask)
            {
                ResetTimer();
                return;
            }

            if (currentMaskDefinition != null)
            {
                UnequipMask();
            }

            currentMaskDefinition = mask;
            remainingTime = mask.duration > 0f ? mask.duration : float.PositiveInfinity;
            OnMaskEquipped?.Invoke(mask);
        }

        /// <summary>
        /// Equip mask and return a result describing what happened.
        /// Useful for callers that need to know whether the mask was replaced, refreshed or newly equipped.
        /// </summary>
        public MaskEquipResult EquipMaskWithResult(MaskDefinition mask)
        {
            var previous = currentMaskDefinition;

            if (mask == null)
            {
                if (previous == null)
                    return MaskEquipResult.NoOp;

                UnequipMask();
                return MaskEquipResult.Unequipped;
            }

            if (previous == mask)
            {
                ResetTimer();
                return MaskEquipResult.RefreshedTimer;
            }

            bool hadPrevious = previous != null;
            EquipMask(mask);
            return hadPrevious ? MaskEquipResult.ReplacedExisting : MaskEquipResult.EquippedNew;
        }

        /// <summary>
        /// Returns true if the player currently has the exact mask instance equipped.
        /// </summary>
        public bool HasMask(MaskDefinition mask)
        {
            return currentMaskDefinition == mask && currentMaskDefinition != null;
        }

        /// <summary>
        /// Returns true if the player currently has a mask of the given type.
        /// </summary>
        public bool HasMaskType(MaskType type)
        {
            return currentMaskDefinition != null && currentMaskDefinition.type == type;
        }

        public void UnequipMask()
        {
            if (currentMaskDefinition == null) return;
            var old = currentMaskDefinition;
            currentMaskDefinition = null;
            remainingTime = 0f;
            OnMaskUnequipped?.Invoke(old);
        }

        public void ResetTimer()
        {
            if (currentMaskDefinition == null) return;
            remainingTime = currentMaskDefinition.duration > 0f ? currentMaskDefinition.duration : float.PositiveInfinity;
        }

        public void ForceExpire()
        {
            if (currentMaskDefinition == null) return;
            var expired = currentMaskDefinition;
            UnequipMask();
            OnMaskExpired?.Invoke(expired);
        }
    }
}
