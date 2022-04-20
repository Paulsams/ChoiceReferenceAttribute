using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace ChoiceReferenceEditor.Repairer
{
    public struct MissingTypeData
    {
        public readonly ManagedReferenceMissingType Data;
        public readonly BaseUnityObjectData UnityObject;

        public MissingTypeData(ManagedReferenceMissingType missingType, BaseUnityObjectData unityObject)
        {
            Data = missingType;
            UnityObject = unityObject;
        }
    }

    public struct TypeData
    {
        public readonly string AssemblyName;
        public readonly string NamespaceName;
        public readonly string ClassName;

        public TypeData(ManagedReferenceMissingType missingType)
        {
            AssemblyName = missingType.assemblyName;
            NamespaceName = missingType.namespaceName;
            ClassName = missingType.className;
        }
    }

    public interface IReadonlyCollectionWithEvent
    {
        public event Action<int> Removed;
        public int Count { get; }
    }

    public interface IReadonlyCollectionWithEvent<T> : IReadonlyCollectionWithEvent
    {
        public ReadOnlyCollection<T> Collection { get; }
        public T this[int index] { get; }
    }

    public class ListWithEvent<T> : IEnumerable<T>, IReadonlyCollectionWithEvent<T>
    {
        public event Action<int> Removed;

        private readonly List<T> _collection;

        public ReadOnlyCollection<T> Collection { get; }

        public int Count => _collection.Count;

        public T this[int index] => _collection[index];

        public ListWithEvent(List<T> list)
        {
            _collection = list;
        }

        public ListWithEvent() : this(new List<T>())
        {
            Collection = _collection.AsReadOnly();
        }

        public void Add(T value)
        {
            _collection.Add(value);
        }

        public void Remove(T value) => RemoveAt(_collection.IndexOf(value));

        public void RemoveAt(int index)
        {
            _collection.RemoveAt(index);
            Removed?.Invoke(index);
        }

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
    }

    public class ContainerMissingTypes
    {
        public readonly TypeData TypeData;

        private readonly ListWithEvent<MissingTypeData> _managedReferencesMissingTypeDatas = new ListWithEvent<MissingTypeData>();

        public IReadonlyCollectionWithEvent<MissingTypeData> ManagedReferencesMissingTypeDatas => _managedReferencesMissingTypeDatas;

        public ContainerMissingTypes(TypeData typeData)
        {
            TypeData = typeData;
        }

        public void Add(MissingTypeData missingTypeData) => _managedReferencesMissingTypeDatas.Add(missingTypeData);

        public void Remove(MissingTypeData missingTypeData) => _managedReferencesMissingTypeDatas.Remove(missingTypeData);

        public void RemoveAt(int index) => _managedReferencesMissingTypeDatas.RemoveAt(index);
    }
}