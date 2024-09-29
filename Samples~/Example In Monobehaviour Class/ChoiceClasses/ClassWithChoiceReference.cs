using UnityEngine;

namespace Paulsams.MicsUtils.ChoiceReference.Example
{
    public class ClassWithChoiceReference : BaseClass
    {
        [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReference;
    }
}