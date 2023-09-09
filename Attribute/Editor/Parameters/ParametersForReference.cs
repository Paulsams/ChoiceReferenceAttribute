using System;
using UnityEditor;
using UnityEngine;

namespace ChoiceReference.Editor.Parameters
{
    public class ParametersForReference : BaseParameters
    {
        public override int IndexInPopup => _indexInPopup;

        public override object ManagedReferenceValue => _managedReferenceValue;

        public override bool IsHaveFoldout => _foldout;

        private bool _foldout = true;
        private object _managedReferenceValue;
        private int _indexInPopup;

        public ParametersForReference(SerializedProperty property, ReferenceData data, object managedReferenceValue)
            : this(property, data, managedReferenceValue,
                Array.IndexOf(data.TypesNames, managedReferenceValue.GetType().Name)) { }

        public ParametersForReference(SerializedProperty property, ReferenceData data,
            object managedReferenceValue, int indexChooseType)
            : base(property, data)
        {
            SetNewManagedReferenceValue(managedReferenceValue, indexChooseType);
        }

        public void SetNewManagedReferenceValue(object managedReferenceValue, int indexInPopup)
        {
            if (managedReferenceValue == null)
                throw new ArgumentException();

            Property.managedReferenceValue = managedReferenceValue;
            _managedReferenceValue = managedReferenceValue;
            _indexInPopup = indexInPopup;

            _foldout = true;
        }

        public override void DrawLabel(string label, Rect rectLabel)
        {
            _foldout = EditorGUI.Foldout(rectLabel, IsHaveFoldout, label);
        }
    }
}