using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;
using Paulsams.MicsUtils;
using UnityEditor.UIElements;
using UnityEngine.SceneManagement;

namespace ChoiceReferenceEditor.Repairer
{
    public class RepairerSerializeReference : EditorWindow
    {
        private static readonly Vector2 _minSizeWindow = new Vector2(450, 450);

        [SerializeField] private VisualTreeAsset _treeAsset;

        [SerializeField] private ListWithEvent<ContainerMissingTypes> _missingTypes;

        private MainContentContainer _mainContentContainer;

        [MenuItem("Tools/RepairerSerializeReference")]
        public static void ShowWindow()
        {
            RepairerSerializeReference editorWindow = GetWindow<RepairerSerializeReference>();
            editorWindow.titleContent = new GUIContent("RepairerSerializeReference");
            editorWindow.minSize = _minSizeWindow;
        }

        private void Init()
        {
            Dictionary<TypeData, ContainerMissingTypes> missingTypes = new Dictionary<TypeData, ContainerMissingTypes>();

            void AddMissingType(ManagedReferenceMissingType missingType, BaseUnityObjectData unityObject)
            {
                TypeData typeData = new TypeData(missingType);
                MissingTypeData missingTypeData = new MissingTypeData(missingType, unityObject);
                if (missingTypes.TryGetValue(typeData, out ContainerMissingTypes containerMissingTypes) == false)
                {
                    containerMissingTypes = new ContainerMissingTypes(typeData);
                    missingTypes.Add(typeData, containerMissingTypes);
                }

                containerMissingTypes.Add(missingTypeData);
            }

            Scene previewScene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();

            foreach (var pathToPrefab in AssetDatabaseUtilities.GetPathToAllPrefabsAssets())
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathToPrefab);
                PrefabUtility.LoadPrefabContentsIntoPreviewScene(pathToPrefab, previewScene);
                var copyPrefab = previewScene.GetRootGameObjects()[0];

                var componentsPrefab = prefab.GetComponentsInChildren<MonoBehaviour>();
                var componentsCopyPrefab = copyPrefab.GetComponentsInChildren<MonoBehaviour>();

                for (int i = 0; i < componentsCopyPrefab.Length; ++i)
                {
                    var monoBehaviour = componentsCopyPrefab[i];
                    if (SerializationUtility.HasManagedReferencesWithMissingTypes(monoBehaviour))
                    {
                        foreach (var missingType in SerializationUtility.GetManagedReferencesWithMissingTypes(monoBehaviour))
                        {
                            PrefabObjectData prefabObject = new PrefabObjectData(componentsPrefab[i].gameObject, pathToPrefab);

                            AddMissingType(missingType, prefabObject);
                        }
                    }
                }

                DestroyImmediate(copyPrefab);
            }

            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(previewScene);

            try//if (missingTypes.Count == 0)
            {
                foreach (var scene in AssetDatabaseUtilities.GetPathsToAllScenesInProject())
                {
                    var gameObjects = scene.GetRootGameObjects();

                    for (int i = 0; i < gameObjects.Length; ++i)
                    {
                        foreach (var monoBehaviour in gameObjects[i].GetComponentsInChildren<MonoBehaviour>())
                        {
                            if (SerializationUtility.HasManagedReferencesWithMissingTypes(monoBehaviour))
                            {
                                foreach (var missingType in SerializationUtility.GetManagedReferencesWithMissingTypes(monoBehaviour))
                                {
                                    SceneObjectData sceneObject = new SceneObjectData(monoBehaviour.GetLocalIdentifierInFile(), scene);
                                    AddMissingType(missingType, sceneObject);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                
            }

            _missingTypes = new ListWithEvent<ContainerMissingTypes>(missingTypes.Select((typeAndContainer) => typeAndContainer.Value).ToList());
        }

        private void UpdateAll()
        {
            Dispose();
            Init();
            InitContainers();
        }

        private void InitContainers()
        {
            _mainContentContainer.Init(_missingTypes);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            TemplateContainer editorWindow = _treeAsset.Instantiate();
            editorWindow.style.flexGrow = 1f;
            root.Add(editorWindow);

            Toolbar mainToolbar = editorWindow.Q<Toolbar>("MainToolbar");
            ToolbarButton updateButton = mainToolbar.Q<ToolbarButton>("Update");
            updateButton.clicked += UpdateAll;

            _mainContentContainer = new MainContentContainer(editorWindow);
            _mainContentContainer.ChangedContainerReferences += OnChangedContainerReferences;
            _mainContentContainer.ChangedSingleReference += OnChangedSingleReference;

            //InitContainers();
        }

        private void Dispose()
        {
            _missingTypes = null;
        }

        private void OnChangedSingleReference(Type type, MissingTypeData missingTypeData)
        {
            ManagedReferenceMissingType missingType = missingTypeData.Data;

            var repairer = new RepairerFile(type, missingTypeData.UnityObject.LocalAssetPath);
            repairer.Repair(() =>
            {
                return repairer.CheckNeedLineAndReplasedIt(missingType);
            });

            _missingTypes[_mainContentContainer.CurrentIndex].Remove(missingTypeData);
            if (_missingTypes[_mainContentContainer.CurrentIndex].ManagedReferencesMissingTypeDatas.Collection.Count == 0)
                _missingTypes.RemoveAt(_mainContentContainer.CurrentIndex);
        }

        private void OnChangedContainerReferences(Type type, ContainerMissingTypes containerMissingTypes)
        {
            Dictionary<string, List<ManagedReferenceMissingType>> fileDatas = new Dictionary<string, List<ManagedReferenceMissingType>>();

            foreach (var missingType in containerMissingTypes.ManagedReferencesMissingTypeDatas.Collection)
            {
                var localAssetPath = missingType.UnityObject.LocalAssetPath;
                if (fileDatas.TryGetValue(localAssetPath, out List<ManagedReferenceMissingType> missingTypes) == false)
                {
                    missingTypes = new List<ManagedReferenceMissingType>();
                    fileDatas.Add(localAssetPath, missingTypes);
                }

                missingTypes.Add(missingType.Data);
            }

            foreach (var fileData in fileDatas)
            {
                string localAssetPath = fileData.Key;
                List<ManagedReferenceMissingType> missingTypes = fileData.Value;

                var repairer = new RepairerFile(type, localAssetPath);
                repairer.Repair(() =>
                {
                    for (int i = missingTypes.Count - 1; i >= 0; --i)
                    {
                        if (repairer.CheckNeedLineAndReplasedIt(missingTypes[i]))
                        {
                            missingTypes.RemoveAt(i);
                            break;
                        }
                    }

                    return missingTypes.Count == 0;
                });
            }

            _missingTypes.RemoveAt(_mainContentContainer.CurrentIndex);
        }

        private void OnDisable()
        {
            Dispose();
        }
    }
}