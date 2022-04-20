using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Data
{
    [SerializeField] private float _speed;
    [SerializeField] private Vector2 _direction;
}

public class ClassWithData : BaseClass
{
    [SerializeField, Ignore] private Data _data;
}