using UnityEngine;

public class ClassWithChoceReference : BaseClass
{
    [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReference;
}