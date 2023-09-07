using UnityEditor;
using UnityEngine;

namespace ChoiceReferenceEditor
{
    public class ParametersForNullReference : BaseParameters
    {
        public override int IndexInPopup => Data.IndexNullVariable;
        public override object ManagedReferenceValue => null;

        public override bool IsHaveFoldout => false;

        public ParametersForNullReference(ReferenceData data) : base(data)
        {

        }

        public override void DrawLabel(string label, Rect rectLabel)
        {
            EditorGUI.LabelField(rectLabel, label);
        }
    }
}