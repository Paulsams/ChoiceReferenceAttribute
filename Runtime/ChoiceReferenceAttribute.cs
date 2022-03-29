using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[System.Serializable]
public class ChoiceReferenceAttribute : PropertyAttribute
{
    public readonly bool Nullable;
    public readonly ReadOnlyCollection<string> IgnoreNameProperties;

    public ChoiceReferenceAttribute(bool nullable = false, params string[] ignoreNameProperties)
    {
        Nullable = nullable;
        IgnoreNameProperties = System.Array.AsReadOnly(ignoreNameProperties);
    }
}