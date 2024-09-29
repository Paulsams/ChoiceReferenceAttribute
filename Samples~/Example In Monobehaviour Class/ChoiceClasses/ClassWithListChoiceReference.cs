using System.Collections.Generic;
using UnityEngine;

namespace Paulsams.MicsUtils.ChoiceReference.Example
{
    public class ClassWithListChoiceReference : BaseClass
    {
        [SerializeReference, ChoiceReference] private List<BaseClass> _listChoiceReferences;
    }
}