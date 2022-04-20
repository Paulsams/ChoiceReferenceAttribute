using UnityEngine;

public class ClassWithChoiceReference : BaseClass
{
    [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReference;
}