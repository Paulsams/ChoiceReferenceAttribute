using Paulsams.MicsUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChoiceReference.Editor.Parameters
{
    public class ReferenceData
    {
        private const string _nullableNameInPopup = "None";

        // An array is declared here, because EditorGUI.Popup accepts only an array
        public readonly string[] TypesNames;
        public readonly ReadOnlyCollection<Type> Types;
        public readonly ReadOnlyDictionary<Type, int> TypeToIndexInPopup;
        
        public readonly IChoiceReferenceParameters DrawParameters;
        public readonly int IndexNullVariable;

        public ReferenceData(Type fieldType, IChoiceReferenceParameters drawParameters)
        {
            Type typeProperty = ReflectionUtilities.GetArrayOrListElementTypeOrThisType(fieldType);
            Types = ReflectionUtilities.GetFinalAssignableTypesFromAllTypes(typeProperty);
            TypeToIndexInPopup = new ReadOnlyDictionary<Type, int>(
                Types
                .Select((type, i) => (type, i))
                .ToDictionary(tuple => tuple.type, tuple => tuple.i)
            );
            List<string> typesNames = Types.Select(type => type.Name).ToList();

            if (drawParameters.Nullable)
                typesNames.Insert(0, _nullableNameInPopup);

            TypesNames = typesNames.ToArray();

            DrawParameters = drawParameters;
            IndexNullVariable = drawParameters.Nullable ? 0 : -1;
        }
    }
}
