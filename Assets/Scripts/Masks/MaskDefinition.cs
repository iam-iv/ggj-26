using UnityEngine;

namespace GGJ.Masks
{
    [CreateAssetMenu(fileName = "MaskDefinition", menuName = "Masks/Mask Definition")]
    public class MaskDefinition : ScriptableObject
    {
        [Tooltip("Logical mask type used by code")]
        public MaskType type = MaskType.None;

        [Tooltip("Duration in seconds. If <= 0 the mask will be treated as infinite until manually unequipped.")]
        public float duration = 10f;
    }
}
