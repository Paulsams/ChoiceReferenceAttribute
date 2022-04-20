using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChoiceReferenceEditor.Repairer
{
    public class SettingsChangeReferenceContainer
    {
        public event Action<bool> ChangedReference;

        private readonly VisualElement _container;

        private readonly Toggle _updateAllToggle;
        private readonly Button _updateButton;

        public SettingsChangeReferenceContainer(VisualElement contentContainer)
        {
            _container = contentContainer.Q<VisualElement>("ChangeReferenceContainer");
            _updateAllToggle = _container.Q<Toggle>("UpdateAll");
            _updateButton = _container.Q<Button>("UpdateButton");

            _updateButton.clicked += OnWantUpdate;
        }

        private void OnWantUpdate()
        {
            ChangedReference?.Invoke(_updateAllToggle.value);
        }
    }
}
