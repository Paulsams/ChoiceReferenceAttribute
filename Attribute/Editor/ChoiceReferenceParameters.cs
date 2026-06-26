using System;

namespace Paulsams.MicsUtils
{
    public readonly struct ChoiceReferenceParameters : IEquatable<ChoiceReferenceParameters>
    {
        public bool Nullable { get; }

        public ChoiceReferenceParameters(bool nullable = false)
        {
            Nullable = nullable;
        }

        public bool Equals(ChoiceReferenceParameters other) =>
            Nullable == other.Nullable;
        public override bool Equals(object obj) =>
            obj is ChoiceReferenceParameters other && Equals(other);
        public override int GetHashCode() =>
            Nullable.GetHashCode();

        public static bool operator ==(ChoiceReferenceParameters left, ChoiceReferenceParameters right) =>
            left.Equals(right);
        public static bool operator !=(ChoiceReferenceParameters left, ChoiceReferenceParameters right) =>
            !left.Equals(right);
    }
}