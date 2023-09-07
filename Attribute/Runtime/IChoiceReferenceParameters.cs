using UnityEngine;

public interface IChoiceReferenceParameters
{
    bool Nullable { get; }
}

public class ChoiceReferenceParameters : IChoiceReferenceParameters {
    public bool Nullable { get; }

    public ChoiceReferenceParameters(bool nullable = false)
    {
        Nullable = nullable;
    }
}

