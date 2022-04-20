using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChoiceReferenceEditor.Repairer
{
    public class ChangerIndexContainer
    {
        public delegate void ChangedIndexHandler();

        public event ChangedIndexHandler ChangedIndex;

        private readonly VisualElement _container;
        private readonly Button _lastIndexButton;
        private readonly Label _currentIndexField;
        private readonly Button _nextIndexButton;

        private IReadonlyCollectionWithEvent _collection;
        private int _currentIndex;

        public int CurrentIndex => _currentIndex;

        private readonly string _textFromCollectionEmpty;

        public ChangerIndexContainer(VisualElement parent, string textFromCollectionEmpty)
        {
            _container = parent.Q<VisualElement>("ChangerContainer");

            _lastIndexButton = _container.Q<Button>("Last");
            _currentIndexField = _container.Q<Label>("CurrentIndex");
            _nextIndexButton = _container.Q<Button>("Next");

            _lastIndexButton.clicked += () => OnChangeIndex(-1);
            _nextIndexButton.clicked += () => OnChangeIndex(1);

            _textFromCollectionEmpty = textFromCollectionEmpty;
            ChangeContentFromCollectionEmpty();
        }

        public void ChangeCollection(IReadonlyCollectionWithEvent collection)
        {
            if (_collection != null)
            {
                _collection.Removed -= OnRemovedIndex;
            }

            _currentIndex = 0;
            _collection = collection;
            
            if (_collection != null)
            {
                _collection.Removed += OnRemovedIndex;
            }

            Update();
        }

        private void Update()
        {
            if (_collection != null && _collection.Count != 0)
            {
                UpdateTextAndButtons();
            }
            else
            {
                ChangeContentFromCollectionEmpty();
            }
        }

        private void ChangeContentFromCollectionEmpty()
        {
            _currentIndex = 0;
            _currentIndexField.text = _textFromCollectionEmpty;
            _lastIndexButton.style.visibility = Visibility.Hidden;
            _nextIndexButton.style.visibility = Visibility.Hidden;
        }

        private void OnRemovedIndex(int index)
        {
            if (_currentIndex == _collection.Count && _collection.Count != 0)
                _currentIndex -= 1;

            ChangedIndex?.Invoke();
            Update();
        }

        private void OnChangeIndex(int direction)
        {
            _currentIndex += direction;
            ChangedIndex?.Invoke();
            UpdateTextAndButtons();
        }

        private void UpdateTextAndButtons()
        {
            _currentIndexField.text = $"{_currentIndex + 1}/{_collection.Count}";

            bool isFirstIndex = _currentIndex == 0;
            bool isLastIndex = _currentIndex == _collection.Count - 1;

            _lastIndexButton.style.visibility = isFirstIndex ? Visibility.Hidden : Visibility.Visible;
            _nextIndexButton.style.visibility = isLastIndex ? Visibility.Hidden : Visibility.Visible;

            _lastIndexButton.SetEnabled(!isFirstIndex);
            _nextIndexButton.SetEnabled(!isLastIndex);
        }
    }
}
