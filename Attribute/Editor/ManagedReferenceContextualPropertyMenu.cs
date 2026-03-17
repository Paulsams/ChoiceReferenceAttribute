using System;
using UnityEditor;
using UnityEngine;

namespace Paulsams.MicsUtils.ChoiceReference.Editor
{
    public static class ManagedReferenceContextualPropertyMenu
    {
        private const char _splitSymbol = '\n';
        
        private class PasteData
        {
            public readonly SerializedProperty Property;
            public readonly Type ObjectType;
            public readonly int IndexSpace;
            public readonly string CopyBuffer;

            public PasteData(SerializedProperty property, Type objectType, string copyBuffer, int indexSpace)
            {
                Property = property;
                ObjectType = objectType;
                CopyBuffer = copyBuffer;
                IndexSpace = indexSpace;
            }

            public void Deconstruct(out SerializedProperty property, out Type objectType,
                out string copyBuffer, out int indexSpace)
            {
                property = Property;
                objectType = ObjectType;
                copyBuffer = CopyBuffer;
                indexSpace = IndexSpace;
            }
        }

        private static readonly GUIContent _pasteContent = new GUIContent("Paste");
        private static readonly GUIContent _newInstanceContent = new GUIContent("New Instance");
        private static readonly GUIContent _resetAndNewInstanceContent = new GUIContent("Reset and New Instance");

        [InitializeOnLoadMethod]
        private static void Initialize() => EditorApplication.contextualPropertyMenu += OnContextualPropertyMenu;

        private static void OnContextualPropertyMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return;
            property = property.Copy();

            menu.AddItem(new GUIContent("Copy"), false, Copy, property);

            bool MayPaste()
            {
                try
                {
                    var copyBuffer = EditorGUIUtility.systemCopyBuffer;
                    var indexSpace = copyBuffer.IndexOf(_splitSymbol);
                    if (indexSpace <= 0)
                        return false;
                    var type = Type.GetType(copyBuffer.Substring(0, indexSpace));
                    if (type is null || property.GetManagedReferenceFieldType().IsAssignableFrom(type) == false)
                        return false;
                    menu.AddItem(_pasteContent, false, Paste, new PasteData(property, type, copyBuffer, indexSpace));
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    return false;
                }
                return true;
            }

            if (MayPaste() == false)
            {
                menu.AddDisabledItem(_pasteContent);
            }

            menu.AddSeparator("");

            if (property.managedReferenceValue != null)
            {
                menu.AddItem(_newInstanceContent, false, NewInstance, property);
                menu.AddItem(_resetAndNewInstanceContent, false, ResetAndNewInstance, property);
            }
            else
            {
                menu.AddDisabledItem(_newInstanceContent);
                menu.AddDisabledItem(_resetAndNewInstanceContent);
            }
        }

        private static void Copy(object customData)
        {
            var property = (SerializedProperty)customData;
            var value = property.managedReferenceValue;
            EditorGUIUtility.systemCopyBuffer = value == null
                ? ""
                : value.GetType().AssemblyQualifiedName + _splitSymbol + JsonUtility.ToJson(property.managedReferenceValue);
        }

        private static void Paste(object customData)
        {
            var (property, objectType, copyBuffer, indexSpace) = (PasteData)customData;

            property.managedReferenceValue = JsonUtility.FromJson(copyBuffer.Substring(indexSpace + 1), objectType);
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void NewInstance(object customData)
        {
            var property = (SerializedProperty)customData;

            var oldValue = property.managedReferenceValue;
            var newValue = oldValue is null
                ? null
                : ReflectionUtilities.CreateObjectByDefaultConstructorOrUninitializedObject(oldValue.GetType());
            EditorUtility.CopySerializedManagedFieldsOnly(oldValue, newValue);
            property.managedReferenceValue = newValue;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void ResetAndNewInstance(object customData)
        {
            var property = (SerializedProperty)customData;
            property.managedReferenceValue = ReflectionUtilities.CreateObjectByDefaultConstructorOrUninitializedObject(
                property.managedReferenceValue.GetType());
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}