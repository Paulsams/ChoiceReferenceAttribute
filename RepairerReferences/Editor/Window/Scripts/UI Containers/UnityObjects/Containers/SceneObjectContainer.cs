using Paulsams.MicsUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ChoiceReferenceEditor.Repairer
{
    public class SceneObjectContainer : UnityObjectBaseContainer
    {
        private readonly TextField _sceneNameField;
        private readonly Button _openSceneButton;
        private readonly ObjectField _objectField;

        private string _pathToCurrentCashedScene;
        private Dictionary<int, MonoBehaviour> _instanceIDToObject = new Dictionary<int, MonoBehaviour>();
        private SceneObjectData _currentSceneObject;

        public SceneObjectContainer(VisualElement contentContainer) : base(contentContainer.Q<VisualElement>("SceneObjectContainer"))
        {
            _sceneNameField = _container.Q<TextField>("SceneName");
            _openSceneButton = _container.Q<Button>("OpenScene");
            _objectField = _container.Q<ObjectField>("SceneObject");

            _openSceneButton.clicked += OnWantOpenScene;
        }

        public MonoBehaviour GetMonoObject(SceneObjectData sceneObject)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != _currentSceneObject.SceneName)
                OpenScene(sceneObject);

            return _instanceIDToObject[sceneObject.LocalIdentifierInFile];
        }

        public void ChangeContent(SceneObjectData sceneObject)
        {
            _currentSceneObject = sceneObject;
            _sceneNameField.SetValueWithoutNotify(_currentSceneObject.SceneName);

            if (_pathToCurrentCashedScene != _currentSceneObject.ScenePath)
                FindAllComponentInCurrentSceneAndUpdateSceneObjectContainer();
            else
                UpdateButtonOpenSceneAndUnityObjectField();
        }

        public override void Enable()
        {
            base.Enable();

            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
        }

        public override void Disable()
        {
            base.Disable();

            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
        }

        private void FindAllComponentInCurrentSceneAndUpdateSceneObjectContainer()
        {
            FindAllComponentInCurrentScene();
            UpdateButtonOpenSceneAndUnityObjectField();
        }

        private void FindAllComponentInCurrentScene()
        {
            _pathToCurrentCashedScene = _currentSceneObject.ScenePath;
            _instanceIDToObject = SceneUtilities.GetAllComponentsInScene<MonoBehaviour>().
                ToDictionary((value) => value.GetLocalIdentifierInFile());
        }

        private void UpdateButtonOpenSceneAndUnityObjectField()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            bool isNeedScene = activeScene.name == _currentSceneObject.SceneName;

            _openSceneButton.style.display = isNeedScene ? DisplayStyle.None : DisplayStyle.Flex;
            _objectField.SetValueWithoutNotify(isNeedScene ? _instanceIDToObject[_currentSceneObject.LocalIdentifierInFile] : null);
        }

        private void OnSceneChanged(Scene last, Scene current)
        {
            FindAllComponentInCurrentSceneAndUpdateSceneObjectContainer();
        }

        private void OnWantOpenScene()
        {
            OpenScene(_currentSceneObject);
        }

        private void OpenScene(SceneObjectData sceneObject)
        {
            EditorSceneManager.OpenScene(sceneObject.ScenePath, OpenSceneMode.Single);
        }
    }
}
