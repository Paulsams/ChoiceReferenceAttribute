using System.Collections.Generic;
using UnityEngine;

namespace Paulsams.MicsUtils.ChoiceReference.Example
{
    public class ExampleChoiceReferenceAttribute : MonoBehaviour
    {
        [Header("Single and Lists")]
        [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReference;
        [SerializeReference, ChoiceReference] private List<BaseClass> _listChoiceReferences;

        [Header("Nullable")]
        [SerializeReference, ChoiceReference(true)] private BaseClass _singleChoiceReferenceNullable;
        [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReferenceNotNullable;
    }
}