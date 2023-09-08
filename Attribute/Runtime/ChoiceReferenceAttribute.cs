using UnityEngine;


[System.Serializable]
public class ChoiceReferenceAttribute : PropertyAttribute, IChoiceReferenceParameters
{
    public bool Nullable { get; }

    public ChoiceReferenceAttribute(bool nullable = false)
    {
        Nullable = nullable;
    }
}