using UnityEngine;

namespace Paulsams.MicsUtils
{
    [System.Serializable]
    public class ChoiceReferenceAttribute : PropertyAttribute
    {
        public bool Nullable { get; }

        public ChoiceReferenceAttribute(bool nullable = false)
        {
            Nullable = nullable;
        }
    }
}