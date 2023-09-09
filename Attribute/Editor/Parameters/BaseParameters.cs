using UnityEditor;
using UnityEngine;

namespace ChoiceReference.Editor.Parameters
{
    public abstract class BaseParameters
    {
        public readonly ReferenceData Data;
        public SerializedProperty Property;

        public abstract int IndexInPopup { get; }
        public abstract object ManagedReferenceValue { get; }
        public abstract bool IsHaveFoldout { get; }
        public bool IsDrawn { get; set; }

        protected BaseParameters(SerializedProperty property, ReferenceData data)
        {
            Data = data;
        }

        public abstract void DrawLabel(string label, Rect rectLabel);
    }
}