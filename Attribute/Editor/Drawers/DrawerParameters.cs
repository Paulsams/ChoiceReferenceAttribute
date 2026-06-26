using System;
using UnityEditor;

namespace Paulsams.MicsUtils.ChoiceReference.Editor.Drawers
{
    public struct DrawerParameters
    {
        public readonly Type FieldType;
        public readonly ChoiceReferenceParameters Parameters;

        public DrawerParameters(Type fieldType, ChoiceReferenceParameters parameters)
        {
            FieldType = ReflectionUtilities.GetArrayOrListElementTypeOrThisType(fieldType);
            Parameters = parameters;
        }

        public DrawerParameters(SerializedProperty property, ChoiceReferenceParameters parameters)
            : this(property.GetManagedReferenceFieldType(), parameters)
        {
        }
    }
}