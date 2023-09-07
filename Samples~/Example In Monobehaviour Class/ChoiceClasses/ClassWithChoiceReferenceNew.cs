using UnityEngine;

public class ClassWithChoiceReferenceNew : BaseClass
{
    [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReference;
}