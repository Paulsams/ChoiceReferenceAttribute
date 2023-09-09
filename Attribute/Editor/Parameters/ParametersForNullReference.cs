using UnityEditor;
using UnityEngine;

namespace ChoiceReference.Editor.Parameters
{
    public class ParametersForNullReference : BaseParameters
    {
        public override int IndexInPopup => Data.IndexNullVariable;
        public override object ManagedReferenceValue => null;

        public override bool IsHaveFoldout => false;

        public ParametersForNullReference(SerializedProperty property, ReferenceData data)
            : base(property, data)
        {
            property.managedReferenceValue = null;
        }

        public override void DrawLabel(string label, Rect rectLabel)
        {
            EditorGUI.LabelField(rectLabel, label);
        }
    }
}