using Paulsams.MicsUtils.ChoiceReference.Editor.Drawers;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Paulsams.MicsUtils.ChoiceReference.Editor
{
    [CustomPropertyDrawer(typeof(ChoiceReferenceAttribute))]
    public class ChoiceReferenceAttributeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            ChoiceReferenceDrawer.UIToolkit.Create(property, property.displayName,
                new DrawerParameters(fieldInfo, attribute as ChoiceReferenceAttribute));

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            ChoiceReferenceDrawer.OnGUI.GetPropertyHeight(property, label,
                new DrawerParameters(fieldInfo, attribute as ChoiceReferenceAttribute));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
            ChoiceReferenceDrawer.OnGUI.Draw(position, property, label,
                new DrawerParameters(fieldInfo, attribute as ChoiceReferenceAttribute));

        public override bool CanCacheInspectorGUI(SerializedProperty property) => true;
    }
}