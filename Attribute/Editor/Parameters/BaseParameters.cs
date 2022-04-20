using UnityEditor;
using UnityEngine;

namespace ChoiceReferenceEditor
{
    public abstract class BaseParameters
    {
        public readonly ReferenceData Data;
        public SerializedProperty Property;

        public abstract int IndexInPopup { get; }
        public abstract object ManagedReferenceValue { get; }
        public abstract bool Foldout { get; }
        public bool IsDrawed { get; set; }

        public BaseParameters(ReferenceData data)
        {
            Data = data;
        }

        public abstract void DrawLabel(string label, Rect rectLabel);
    }
}