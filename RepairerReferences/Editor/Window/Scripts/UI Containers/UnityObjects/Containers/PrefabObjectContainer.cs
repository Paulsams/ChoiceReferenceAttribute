using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChoiceReferenceEditor.Repairer
{
    public class PrefabObjectContainer : UnityObjectBaseContainer
    {
        private ObjectField _prefabField;

        public PrefabObjectContainer(VisualElement contentContainer) : base(contentContainer.Q<VisualElement>("PrefabObjectContainer"))
        {
            _prefabField = _container.Q<ObjectField>("PrefabObject");
        }

        public void ChangeContent(PrefabObjectData prefabObject)
        {
            _prefabField.SetValueWithoutNotify(prefabObject.Prefab);
        }
    }
}
