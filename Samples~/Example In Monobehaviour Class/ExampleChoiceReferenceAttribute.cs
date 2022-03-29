using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paulsams.MicsUtil;

public class Example : MonoBehaviour
{
    [Header("Single and Lists")]
    [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReference;

    [SerializeReference, ChoiceReference] private List<BaseClass> _listChoiceReferences;

    [Header("Nullable")]
    [SerializeReference, ChoiceReference(true)] private BaseClass _singleChoiceReferenceNullable;
    [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReferenceNotNullable;

    [Header("Ignore Datas")]
    [SerializeReference, ChoiceReference("_data")] private BaseClass _singleChoiceReferenceWithIgnoreData;
    [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReferenceNotIgnoreData;
}