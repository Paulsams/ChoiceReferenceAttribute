using System;
using System.Collections.Generic;
using System.Reflection;
using Paulsams.MicsUtils.ChoiceReference.Editor.Parameters;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Paulsams.MicsUtils.ChoiceReference.Editor.Drawers
{
    public static partial class ChoiceReferenceDrawer
    {
        private static readonly Dictionary<FieldInfo, ReferenceData> _dataReferences =
            new Dictionary<FieldInfo, ReferenceData>();

        private static void SetValueAndSaveProperty(SerializedProperty otherProperty, object baseNewValue)
        {
            void ApplyBySerializedObject(Object targetObject, object newValue)
            {
                SerializedObject serializedObject = new SerializedObject(targetObject);
                SerializedProperty property = serializedObject.FindProperty(otherProperty.propertyPath);
                property.managedReferenceValue = newValue;
                property.isExpanded = newValue != null;

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            ApplyBySerializedObject(otherProperty.serializedObject.targetObjects[0], baseNewValue);
            for (var i = 1; i < otherProperty.serializedObject.targetObjects.Length; i++)
            {
                var targetObject = otherProperty.serializedObject.targetObjects[i];
                var otherNewValue = baseNewValue is null
                    ? null
                    : ReflectionUtilities.CreateObjectByDefaultConstructorOrUninitializedObject(baseNewValue.GetType());
                EditorUtility.CopySerializedManagedFieldsOnly(baseNewValue, otherNewValue);
                ApplyBySerializedObject(targetObject, otherNewValue);
            }

            otherProperty.serializedObject.Update();
        }

        private static PropertyParameters GetParameters(
            SerializedProperty property, DrawerParameters drawerParameters
        ) => GetParameters(property, GetOrCreateDataReference(drawerParameters));

        private static PropertyParameters GetParameters(SerializedProperty property, ReferenceData referenceData)
        {
            object managedReferenceValue = property.GetManagedReferenceValueFromPropertyPath();
            return new PropertyParameters(property, referenceData, managedReferenceValue);
        }

        private static void ChangeManagedReferenceValue(ref PropertyParameters parameters, int indexInPopup)
        {
            var property = parameters.Property;
            var data = parameters.Data;

            if (indexInPopup == data.IndexNullVariable)
            {
                SetValueAndSaveProperty(property, null);
                parameters = new PropertyParameters(property, data, null);
            }
            else if (TryCreateManagedReferenceValueAndCopyFields(parameters, indexInPopup, out var newManagedReference))
            {
                SetValueAndSaveProperty(property, newManagedReference);
                parameters = new PropertyParameters(
                    property, parameters.Data, newManagedReference, indexInPopup
                );
            }
        }

        private static bool TryCreateManagedReferenceValueAndCopyFields(
            in PropertyParameters parameters, int indexInPopup, out object newManagedReference)
        {
            parameters.Property.serializedObject.Update();

            object oldManagedReference = parameters.ManagedReferenceValue;
            int indexChoiceType = indexInPopup;

            // I don't want to add the first element to Types as null, so I make such a crutch.
            if (parameters.Data.DrawParameters.Nullable)
                indexChoiceType -= 1;

            Type typeNewManagedReference = parameters.Data.Types[indexChoiceType];
            try
            {
                newManagedReference =
                    ReflectionUtilities.CreateObjectByDefaultConstructorOrUninitializedObject(typeNewManagedReference);

                var canChangeSerializeReference = newManagedReference as ISerializeReferenceChangeValidate;
                if (canChangeSerializeReference == null || canChangeSerializeReference.Validate(out string textError))
                {
                    if (oldManagedReference != null)
                        EditorUtility.CopySerializedManagedFieldsOnly(oldManagedReference, newManagedReference);
                }
                else
                {
                    EditorUtility.DisplayDialog($"Cannot be changed to {typeNewManagedReference}", textError, "Ok");
                    newManagedReference = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                newManagedReference = null;
                return false;
            }
        }

        private static void DrawFromPropertyDrawerOrLoopFromChildren(PropertyParameters parameters,
            Action<PropertyDrawer> actionFromDrawer,
            Action<SerializedProperty> actionFromChildren)
        {
            if (parameters.MayExpanded)
            {
                var drawerType = EditorGUIUtilityInternal.GetDrawerTypeForPropertyAndType(
                    parameters.Property,
                    parameters.Property.GetManagedReferenceValueFromPropertyPath().GetType()
                );

                if (drawerType != null)
                {
                    actionFromDrawer((PropertyDrawer)Activator.CreateInstance(drawerType));
                }
                else
                {
                    foreach (var children in parameters.Property.GetChildren())
                        actionFromChildren(children);
                }
            }
        }

        private static ReferenceData GetOrCreateDataReference(DrawerParameters drawerParameters)
        {
            if (_dataReferences.TryGetValue(drawerParameters.FieldInfo, out ReferenceData data) == false)
            {
                data = new ReferenceData(drawerParameters.FieldInfo.FieldType, drawerParameters.DrawParameters);
                _dataReferences.Add(drawerParameters.FieldInfo, data);
            }

            return data;
        }
    }
}