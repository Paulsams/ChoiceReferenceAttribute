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

        public ParametersForReference(ReferenceData data, object managedReferenceValue) : base(data)
        {
            int indexInPopup = Array.IndexOf(Data.TypesNames, managedReferenceValue.GetType().Name);
            SetNewManagedReferenceValue(managedReferenceValue, indexInPopup);
        }

        public ParametersForReference(ReferenceData data, object managedReferenceValue, int indexChoicedType) : base(data)
        {
            SetNewManagedReferenceValue(managedReferenceValue, indexChoicedType);
        }

        public void SetNewManagedReferenceValue(object managedReferenceValue, int indexInPopup)
        {
            if (managedReferenceValue == null)
                throw new ArgumentException();

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