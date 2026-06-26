using UnityEditor;

namespace Paulsams.MicsUtils.ChoiceReference.Editor.Parameters
{
    public struct PropertyParameters
    {
        public readonly ReferenceData Data;
        public readonly SerializedProperty Property;
        public readonly int IndexInPopup;
        public readonly object ManagedReferenceValue;

        public bool MayExpanded { get; }

        public PropertyParameters(SerializedProperty property, ReferenceData data)
            : this(property, data, property.GetManagedReferenceValueFromPropertyPath()) { }

        public PropertyParameters(SerializedProperty property, ReferenceData data, object managedReferenceValue)
            : this(property, data, managedReferenceValue, GetIndexInPopupFromValue(data, managedReferenceValue)) { }

        public PropertyParameters(
            SerializedProperty property, ReferenceData data,
            object newManagedReferenceValue, int indexInPopup)
        {
            Property = property;
            Data = data;
            ManagedReferenceValue = newManagedReferenceValue;
            IndexInPopup = indexInPopup;
            MayExpanded = IndexInPopup != Data.IndexNullVariable;
        }

        public static int GetIndexInPopupFromValue(ReferenceData data, object managedReferenceValue) =>
            managedReferenceValue == null ||
            data.TypeToIndexInPopup.TryGetValue(managedReferenceValue.GetType(), out var index) == false
                ? data.IndexNullVariable
                : index + (data.DrawParameters.Nullable ? 1 : 0);
    }
}