using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace ChoiceReference.Editor
{
    [CustomPropertyDrawer(typeof(ChoiceReferenceAttribute))]
    public class ChoiceReferenceAttributeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return ChoiceReferenceDrawer.CreateVisualElement(property, property.displayName,
                new ChoiceReferenceDrawerParameters(fieldInfo, attribute as ChoiceReferenceAttribute));
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ChoiceReferenceDrawer.GetPropertyHeight(property, label,
                new ChoiceReferenceDrawerParameters(fieldInfo, attribute as ChoiceReferenceAttribute));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ChoiceReferenceDrawer.DrawOnGUI(position, property, label,
                new ChoiceReferenceDrawerParameters(fieldInfo, attribute as ChoiceReferenceAttribute));
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property) => true;
    }
}