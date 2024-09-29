using UnityEngine;

namespace Paulsams.MicsUtils.ChoiceReference.Example
{
    [System.Serializable]
    public struct NewData
    {
        [SerializeField] private float _speed;
        [SerializeField] private Vector2 _direction;
    }

    public class ClassWithNewData : BaseClass
    {
        [SerializeField, Ignore] private NewData _data;
    }
}