using System;
using UnityEditor;
using UnityEngine;

namespace ChoiceReferenceEditor
{
    public class ParametersForReference : BaseParameters
    {
        public override int IndexInPopup => _indexInPopup;

        public override object ManagedReferenceValue => _managedReferenceValue;

        public override bool Foldout => _foldout;

        public bool _foldout = true;
        public object _managedReferenceValue;

        public int _indexInPopup;

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
            _foldout = EditorGUI.Foldout(rectLabel, Foldout, label);
        }
    }
}