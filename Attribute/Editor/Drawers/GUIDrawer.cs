using Paulsams.MicsUtils.ChoiceReference.Editor.Parameters;
using UnityEditor;
using UnityEngine;

namespace Paulsams.MicsUtils.ChoiceReference.Editor.Drawers
{
    public static partial class ChoiceReferenceDrawer
    {
        public static class OnGUI
        {
            public static float GetPropertyHeight(SerializedProperty property, GUIContent label,
                DrawerParameters drawerParameters)
            {
                property.serializedObject.ApplyModifiedProperties();
                PropertyParameters parameters = GetParameters(property, drawerParameters);

                float height = EditorGUIUtility.singleLineHeight;
                DrawFromPropertyDrawerOrLoopFromChildren(parameters,
                    (drawer) =>
                    {
                        height += drawer.GetPropertyHeight(parameters.Property, label) +
                                  EditorGUIUtility.standardVerticalSpacing;
                    },
                    (children) =>
                    {
                        height += EditorGUI.GetPropertyHeight(children, true) +
                                  EditorGUIUtility.standardVerticalSpacing;
                    });

                return height;
            }

            public static void Draw(Rect position, SerializedProperty property, GUIContent label,
                DrawerParameters drawerParameters) =>
                DrawManagedReference(property, label, position, drawerParameters);

            private static void DrawManagedReference(SerializedProperty property, GUIContent label, Rect rect,
                DrawerParameters drawerParameters)
            {
                PropertyParameters parameters = GetParameters(property, drawerParameters);
                ObjectState state = GetOrCreateObjectState(parameters.Property);

                rect.height = EditorGUIUtility.singleLineHeight;
                Rect rectLabel = rect;
                rectLabel.width = EditorGUIUtility.labelWidth;

                if (parameters.MayExpanded)
                    property.isExpanded = EditorGUI.Foldout(rectLabel, property.isExpanded, label, true);
                else
                    EditorGUI.LabelField(rectLabel, label.text);

                int indexInPopup = DrawPopupAndGetIndex(parameters, rect);
                if (indexInPopup != parameters.IndexInPopup)
                {
                    RemoveObject(parameters);
                    ChangeManagedReferenceValue(ref parameters, state, indexInPopup);
                    AddObject(parameters);
                }

                DrawProperty(parameters, label, rect);
            }

            private static int DrawPopupAndGetIndex(PropertyParameters parameters, Rect rect)
            {
                Rect rectPopup = rect;
                float offset = EditorGUIUtility.labelWidth + 2f - EditorGUI.indentLevel * 15f;
                rectPopup.x += offset;
                rectPopup.width -= offset;

                int indexInPopup = EditorGUI.Popup(rectPopup, parameters.IndexInPopup, parameters.Data.TypesNames);
                return indexInPopup;
            }

            private static void DrawProperty(PropertyParameters parameters, GUIContent label, Rect rect)
            {
                ++EditorGUI.indentLevel;
                {
                    Rect rectField = rect;
                    rectField.y += rect.height + EditorGUIUtility.standardVerticalSpacing;

                    DrawFromPropertyDrawerOrLoopFromChildren(parameters,
                        (drawer) =>
                        {
                            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            drawer.OnGUI(rect, parameters.Property, label);
                        },
                        (children) =>
                        {
                            EditorGUI.PropertyField(rectField, children, true);
                            rectField.y += EditorGUI.GetPropertyHeight(children, true) +
                                           EditorGUIUtility.standardVerticalSpacing;
                        });
                }
                --EditorGUI.indentLevel;
            }
        }
    }
}