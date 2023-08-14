using UnityEngine;
using UnityEditor;

namespace ChoiceReferenceEditor
{
    [CustomPropertyDrawer(typeof(ChoiceReferenceAttribute))]
    public class ChoiceReferenceAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            BaseParameters parameters = ChoiceReferencePropertyDrawer.GetParameters(property);

            float height = EditorGUIUtility.singleLineHeight;

            ChoiceReferencePropertyDrawer.LoopFromChildrens(parameters, (children) =>
            {
                height += EditorGUI.GetPropertyHeight(children, true) + EditorGUIUtility.standardVerticalSpacing;
            });

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ChoiceReferencePropertyDrawer.DrawManagedReference(property, label.text, position);
        }
    }
}