using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[System.Serializable]
public class ChoiceReferenceAttribute : PropertyAttribute
{
    public readonly bool Nullable;

    public ChoiceReferenceAttribute(bool nullable = false)
    {
        Nullable = nullable;
    }
}