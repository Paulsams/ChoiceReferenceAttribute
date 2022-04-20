using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChoiceReferenceEditor.Repairer
{
    public class MainContentContainer
    {
        private class FieldForTwoValues
        {
            public readonly TextField Old;
            public readonly TextField New;

            private readonly Button _copyButton;

            public FieldForTwoValues(VisualElement container)
            {
                VisualElement values = container.Q<VisualElement>("Values");

                Old = values.Q<TextField>("Old");
                New = values.Q<TextField>("New");

                _copyButton = values.Q<Button>("Copy");
                _copyButton.clicked += OnCopyOldToNew;
            }

            private void OnCopyOldToNew()
            {
                New.SetValueWithoutNotify(Old.value);
            }
        }

        public delegate void ChangeSingleReferenceHandler(Type type, MissingTypeData missingTypeData);
        public delegate void ChangeContainerReferenceHandler(Type type, ContainerMissingTypes containerMissingTypes);

        public event ChangeSingleReferenceHandler ChangedSingleReference;
        public event ChangeContainerReferenceHandler ChangedContainerReferences;

        private readonly ChangerIndexContainer _changerIndex;

        private readonly VisualElement _container;
        private readonly VisualElement _typeInfoContainer;

        private readonly FieldForTwoValues _classNameField;
        private readonly FieldForTwoValues _assemblyNameField;
        private readonly FieldForTwoValues _namespaceNameField;

        public readonly DataObjectContainer DataObjectContainer;
        public readonly SettingsChangeReferenceContainer SettingsChangeReferenceContainer;

        private IReadonlyCollectionWithEvent<ContainerMissingTypes> _missingTypeContainers;

        public int CurrentIndex => _changerIndex.CurrentIndex;

        private ContainerMissingTypes CurrentContainerMissingTypes => _missingTypeContainers[_changerIndex.CurrentIndex];

        public MainContentContainer(VisualElement editorWindow)
        {
            _container = editorWindow.Q<VisualElement>("MainContentContainer");
            _container.style.display = DisplayStyle.None;

            _typeInfoContainer = _container.Q<VisualElement>("TypeInfoContainer");

            _classNameField = new FieldForTwoValues(_typeInfoContainer.Q<VisualElement>("ClassNameContainer"));
            _assemblyNameField = new FieldForTwoValues(_typeInfoContainer.Q<VisualElement>("AssemblyNameContainer"));
            _namespaceNameField = new FieldForTwoValues(_typeInfoContainer.Q<VisualElement>("NamespaceNameContainer"));

            DataObjectContainer = new DataObjectContainer(_container);

            SettingsChangeReferenceContainer = new SettingsChangeReferenceContainer(_typeInfoContainer);
            SettingsChangeReferenceContainer.ChangedReference += OnChangedReference;

            _changerIndex = new ChangerIndexContainer(editorWindow, "Missings not have");
            _changerIndex.ChangedIndex += OnChangedIndex;
        }

        public void Init(IReadonlyCollectionWithEvent<ContainerMissingTypes> missingTypes)
        {
            _container.style.display = DisplayStyle.Flex;

            _missingTypeContainers = missingTypes;
            _changerIndex.ChangeCollection(_missingTypeContainers);

            UpdateContainerFromCurrentIndex();
        }

        private void UpdateContainerFromCurrentIndex()
        {
            if (_missingTypeContainers.Count == 0)
            {
                _container.style.display = DisplayStyle.None;
                DataObjectContainer.ChangeCollectionDatas(null);
                return;
            }

            ResetContent();
            ChangeCollectionDatas();
        }

        private void ChangeCollectionDatas()
        {
            DataObjectContainer.ChangeCollectionDatas(CurrentContainerMissingTypes.ManagedReferencesMissingTypeDatas);
        }

        private void ResetContent()
        {
            ContainerMissingTypes containerMissingTypes = CurrentContainerMissingTypes;
            TypeData typeData = containerMissingTypes.TypeData;

            _classNameField.Old.SetValueWithoutNotify(typeData.ClassName);
            _assemblyNameField.Old.SetValueWithoutNotify(typeData.AssemblyName);
            _namespaceNameField.Old.SetValueWithoutNotify(typeData.NamespaceName);

            _classNameField.New.SetValueWithoutNotify("");
            _assemblyNameField.New.SetValueWithoutNotify("");
            _namespaceNameField.New.SetValueWithoutNotify("");
        }

        private void OnChangedIndex()
        {
            //_container.style.display = _missingTypeContainers.Count != 0 ? DisplayStyle.Flex : DisplayStyle.None;

            UpdateContainerFromCurrentIndex();
        }

        private void OnChangedReference(bool isManyUpdateToggle)
        {
            string typeName = $"{_namespaceNameField.New.value}.{_classNameField.New.value}";
            string assemblyName = _assemblyNameField.New.value;

            Type type = Type.GetType(typeName + ", " + assemblyName, throwOnError: false, false);

            if (type == null)
            {
                EditorUtility.DisplayDialog("Type equal null", "System.Type could not be created based on the data you specified.", "OK");
                return;
            }

            if (isManyUpdateToggle)
                ChangedContainerReferences?.Invoke(type, CurrentContainerMissingTypes);
            else
                ChangedSingleReference?.Invoke(type, DataObjectContainer.CurrentMissingType);
        }
    }
}
