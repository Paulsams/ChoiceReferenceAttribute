using UnityEngine;

public class SimpleClass : BaseClass
{
    [SerializeField] private string _string;

    [Header("Test Header")]
    [SerializeField] private Vector3 _vector;
}