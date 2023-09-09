using UnityEditor;
using UnityEngine;

namespace ChoiceReference.Editor.Parameters
{
    public enum DrawnChoiceReferenceType
    {
        OnGUI,
        UIToolkit,
        NotDrawn,
    };
    
    public abstract class BaseParameters
    {
        public readonly ReferenceData Data;
        public readonly SerializedProperty Property;

        public abstract int IndexInPopup { get; }
        public abstract object ManagedReferenceValue { get; }
        public abstract bool IsHaveFoldout { get; }
        public DrawnChoiceReferenceType DrawnType { get; set; }

        protected BaseParameters(SerializedProperty property, ReferenceData data)
        {
            Property = property;
            Data = data;
        }

        public abstract void DrawLabel(string label, Rect rectLabel);
    }
}