using UnityEngine;

namespace Paulsams.MicsUtils.ChoiceReference.Example
{
    public class ClassWithChoceReference : BaseClass
    {
        [SerializeReference, ChoiceReference] private BaseClass _singleChoiceReference;
    }
}