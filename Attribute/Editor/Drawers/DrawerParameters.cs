﻿using System.Reflection;
using UnityEditor;

namespace Paulsams.MicsUtils.ChoiceReference.Editor.Drawers
{
    public struct DrawerParameters
    {
        public readonly FieldInfo FieldInfo;
        public readonly IChoiceReferenceParameters DrawParameters;

        public DrawerParameters(FieldInfo fieldInfo, IChoiceReferenceParameters drawParameters)
        {
            FieldInfo = fieldInfo;
            DrawParameters = drawParameters;
        }

        public DrawerParameters(SerializedProperty property, IChoiceReferenceParameters drawParameters)
        {
            FieldInfo = property.GetFieldInfoFromPropertyPath().field;
            DrawParameters = drawParameters;
        }
    }
}