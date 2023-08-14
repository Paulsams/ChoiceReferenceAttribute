using Paulsams.MicsUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChoiceReferenceEditor
{
    public class ReferenceData
    {
        private const string _nullableNameInPopup = "None";

        //An array is declared here, because EditorGUI.Popup accepts only an array.
        public readonly string[] TypesNames;
        public readonly ReadOnlyCollection<Type> Types;

        public readonly ChoiceReferenceAttribute Attribute;
        public readonly int IndexNullVariable;

        public ReferenceData(Type fieldType, ChoiceReferenceAttribute choiceReferenceAttribute) : this(fieldType)
        {
            List<string> typesNames = Types.Select(type => type.Name).ToList();
            if (choiceReferenceAttribute.Nullable)
                typesNames.Insert(0, _nullableNameInPopup);

            TypesNames = typesNames.ToArray();
            Attribute = choiceReferenceAttribute;
            IndexNullVariable = choiceReferenceAttribute.Nullable ? 0 : -1;
        }

        public ReferenceData(Type fieldType)
        {
            Type typeProperty = ReflectionUtilities.GetArrayOrListElementTypeOrThisType(fieldType);
            Types = ReflectionUtilities.GetFinalAssignableTypesFromAllTypes(typeProperty);
            List<string> typesNames = Types.Select(type => type.Name).ToList();

            TypesNames = typesNames.ToArray();
            IndexNullVariable = -1;
        }
    }
}
