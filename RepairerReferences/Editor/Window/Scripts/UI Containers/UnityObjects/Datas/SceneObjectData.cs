using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoiceReferenceEditor.Repairer
{
    public class SceneObjectData : BaseUnityObjectData
    {
        public readonly int LocalIdentifierInFile;
        public readonly string SceneName;
        public readonly string ScenePath;

        public override string LocalAssetPath => ScenePath;

        public SceneObjectData(int localIdentifierInFile, Scene scene)
        {
            LocalIdentifierInFile = localIdentifierInFile;
            SceneName = scene.name;
            ScenePath = scene.path;
        }

        public override UnityObjectBaseContainer ChangeContent(DataObjectContainer dataObjectContainer)
        {
            dataObjectContainer.SceneObjectContainer.ChangeContent(this);
            return dataObjectContainer.SceneObjectContainer;
        }
    }
}
