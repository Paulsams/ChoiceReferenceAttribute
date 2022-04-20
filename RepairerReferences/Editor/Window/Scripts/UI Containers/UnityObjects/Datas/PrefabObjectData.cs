using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoiceReferenceEditor.Repairer
{
    public class PrefabObjectData : BaseUnityObjectData
    {
        public readonly GameObject Prefab;
        public readonly string PathToPrefab;

        public override string LocalAssetPath => PathToPrefab;

        public PrefabObjectData(GameObject componentInPrefab, string pathToPrefab)
        {
            Prefab = componentInPrefab;
            PathToPrefab = pathToPrefab;
        }

        public override UnityObjectBaseContainer ChangeContent(DataObjectContainer dataObjectContainer)
        {
            dataObjectContainer.PrefabObjectContainer.ChangeContent(this);
            return dataObjectContainer.PrefabObjectContainer;
        }
    }
}
