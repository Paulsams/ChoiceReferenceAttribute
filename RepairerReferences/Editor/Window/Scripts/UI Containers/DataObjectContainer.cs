using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChoiceReferenceEditor.Repairer
{
    public class DataObjectContainer
    {
        private readonly VisualElement _dataObjectContainer;
        private readonly VisualElement _miscInfoContainer;

        private readonly LongField _referenceIdField;
        private readonly TextField _serializedDataField;

        public readonly PrefabObjectContainer PrefabObjectContainer;
        public readonly SceneObjectContainer SceneObjectContainer;

        private readonly ChangerIndexContainer _changerIndex;

        private UnityObjectBaseContainer _currentContainerForObject;

        private IReadonlyCollectionWithEvent<MissingTypeData> _missingTypeDatas;

        public MissingTypeData CurrentMissingType => _missingTypeDatas[_changerIndex.CurrentIndex];

        public DataObjectContainer(VisualElement mainContentContainer)
        {
            _dataObjectContainer = mainContentContainer.Q<VisualElement>("DataObjectContainer").Q<VisualElement>("ContentContainer").contentContainer;
            _miscInfoContainer = _dataObjectContainer.Q<VisualElement>("MiscInfoContainer");

            _referenceIdField = _miscInfoContainer.Q<LongField>("ReferenceId");
            _serializedDataField = _miscInfoContainer.Q<TextField>("SerializedData");

            _changerIndex = new ChangerIndexContainer(mainContentContainer, "");
            _changerIndex.ChangedIndex += OnChangedIndex;

            SceneObjectContainer = new SceneObjectContainer(_dataObjectContainer);
            PrefabObjectContainer = new PrefabObjectContainer(_dataObjectContainer);
        }

        private void OnChangedIndex()
        {
            UpdateContent();
        }

        public void ChangeCollectionDatas(IReadonlyCollectionWithEvent<MissingTypeData> missingTypeDatas)
        {
            _missingTypeDatas = missingTypeDatas;
            _changerIndex.ChangeCollection(_missingTypeDatas);

            UpdateContent();
        }

        public void UpdateContent()
        {
            _currentContainerForObject?.Disable();
            _currentContainerForObject = null;

            if (_missingTypeDatas == null || _missingTypeDatas.Count == 0)
                return;

            MissingTypeData missingType = CurrentMissingType;

            _referenceIdField.SetValueWithoutNotify(missingType.Data.referenceId);
            _serializedDataField.SetValueWithoutNotify(missingType.Data.serializedData);

            _currentContainerForObject = missingType.UnityObject.ChangeContent(this);
            _currentContainerForObject.Enable();
        }
    }
}
