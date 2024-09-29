using UnityEngine;

namespace Paulsams.MicsUtils.ChoiceReference.Example
{
    public class ClassWithChoiceReferenceNew : BaseClass
    {
        [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReference;
    }
}