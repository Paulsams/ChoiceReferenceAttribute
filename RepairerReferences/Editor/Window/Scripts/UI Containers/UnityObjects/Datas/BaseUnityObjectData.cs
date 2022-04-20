using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChoiceReferenceEditor.Repairer
{
    public abstract class BaseUnityObjectData
    {
        public abstract string LocalAssetPath { get; }

        public abstract UnityObjectBaseContainer ChangeContent(DataObjectContainer dataObjectContainer);
    }
}
