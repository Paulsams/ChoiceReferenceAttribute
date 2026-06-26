using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;

namespace Paulsams.MicsUtils.ChoiceReference.Editor.Parameters
{
    public class ReferenceData
    {
        private const string _nullableNameInPopup = "None";

        // An array is declared here, because EditorGUI.Popup accepts only an array
        public readonly string[] TypesNames;
        public readonly ReadOnlyCollection<Type> Types;
        public readonly ReadOnlyDictionary<Type, int> TypeToIndexInPopup;

        public readonly ChoiceReferenceParameters DrawParameters;
        public readonly int IndexNullVariable;

        public ReferenceData(Type typeProperty, ChoiceReferenceParameters drawParameters,
            Func<IEnumerable<Type>, IEnumerable<(Type, string)>> filter = null)
        {
            var derivedTypes = TypeCache.GetTypesDerivedFrom(typeProperty)
                .Append(typeProperty)
                .Where(type => type.IsAbstract == false);
            IEnumerable<(Type type, string name)> typesWithNames;
            if (filter != null)
                typesWithNames = filter(derivedTypes);
            else
                typesWithNames = derivedTypes.Select(type => (type, type.Name));

            var materializedTypesWithNames = typesWithNames.ToList();
            var types = new Type[materializedTypesWithNames.Count];
            var typeToIndexInPopup = new Dictionary<Type, int>(materializedTypesWithNames.Count);
            var offsetIndexForNames = drawParameters.Nullable ? 1 : 0;
            var typesNames = new string[materializedTypesWithNames.Count + offsetIndexForNames];
            if (offsetIndexForNames != 0)
                typesNames[0] = _nullableNameInPopup;
            for (var i = 0; i < materializedTypesWithNames.Count; i++)
            {
                var (type, name) = materializedTypesWithNames[i];
                types[i] = type;
                typeToIndexInPopup.Add(type, i);
                typesNames[i + offsetIndexForNames] = name;
            }
            Types = Array.AsReadOnly(types);
            TypeToIndexInPopup = new ReadOnlyDictionary<Type, int>(typeToIndexInPopup);
            TypesNames = typesNames;

            DrawParameters = drawParameters;
            IndexNullVariable = drawParameters.Nullable ? 0 : -1;
        }
    }
}