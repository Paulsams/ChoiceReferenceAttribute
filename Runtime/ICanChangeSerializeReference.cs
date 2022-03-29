public interface ICanChangeSerializeReference
{
    bool CanChangeSerializeReference();
    string TextError { get; }
}