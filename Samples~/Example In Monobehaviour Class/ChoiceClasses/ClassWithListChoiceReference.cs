using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassWithListChoiceReference : BaseClass
{
    [SerializeReference, ChoiceReference] private List<BaseClass> _listChoiceReferences;
}
