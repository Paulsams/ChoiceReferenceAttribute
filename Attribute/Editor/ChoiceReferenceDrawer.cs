using System;
using System.Collections.Generic;
using System.Reflection;
using ChoiceReference.Editor.Parameters;
using Paulsams.MicsUtils;
using UnityEditor;
using UnityEngine;

namespace ChoiceReference.Editor.Drawers
{
    public static partial class ChoiceReferenceDrawer
    {
        private class ObjectState
        {
            public SerializedObject SerializedObject;

            // After thinking a lot, I couldn’t figure out how to do it better
            public readonly List<WeakReference<object>> WeakReferencesOnObjects = new List<WeakReference<object>>();
            public readonly HashSet<string> PropertiesNotMetFirstTime = new HashSet<string>();
        }

        // TODO: need to add support for many SerializedObject
        private static readonly ObjectState _objectState = new ObjectState();

        private static readonly Dictionary<FieldInfo, ReferenceData> _dataReferences =
            new Dictionary<FieldInfo, ReferenceData>();

        #region Removing unused parameters

        private static readonly FieldInfo _serializedObjectObjectPtrFieldInfo;

        static ChoiceReferenceDrawer()
        {
            _serializedObjectObjectPtrFieldInfo = typeof(SerializedObject).GetField("m_NativeObjectPtr",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Selection.selectionChanged += CollectUnusedParameters;
            EditorApplication.update += CoroutineForCollectUnusedParameters;
        }

        private static void CoroutineForCollectUnusedParameters() => CollectUnusedParameters();

        private static void CollectUnusedParameters()
        {
            if (_objectState.SerializedObject == null)
                return;

            bool isNotValidSerializedObject = (IntPtr)_serializedObjectObjectPtrFieldInfo
                .GetValue(_objectState.SerializedObject) == IntPtr.Zero;

            if (isNotValidSerializedObject)
            {
                _objectState.SerializedObject = null;
                _objectState.WeakReferencesOnObjects.Clear();
                _objectState.PropertiesNotMetFirstTime.Clear();
                return;
            }

            _objectState.PropertiesNotMetFirstTime.RemoveWhere(propertyPath =>
                _objectState.SerializedObject.FindProperty(propertyPath) == null);

            _objectState.WeakReferencesOnObjects.RemoveAll(obj => obj.TryGetTarget(out _) == false);
        }

        #endregion

        private static void AddObject(in PropertyParameters parameters)
        {
            if (parameters.ManagedReferenceValue != null)
                _objectState.WeakReferencesOnObjects
                    .Add(new WeakReference<object>(parameters.ManagedReferenceValue));
        }

        private static void RemoveObject(in PropertyParameters parameters)
        {
            if (parameters.ManagedReferenceValue != null)
                _objectState.WeakReferencesOnObjects
                    .RemoveAt(GetIndexObject(parameters.ManagedReferenceValue));
        }

        private static void SetValueAndSaveProperty(SerializedProperty otherProperty, object newValue)
        {
            foreach (var targetObject in _objectState.SerializedObject.targetObjects)
            {
                SerializedObject serializedObject = new SerializedObject(targetObject);
                SerializedProperty property = serializedObject.FindProperty(otherProperty.propertyPath);
                property.managedReferenceValue = newValue;
                property.isExpanded = newValue != null;

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            _objectState.SerializedObject.Update();
        }

        private static int GetIndexObject(object currentObject) =>
            currentObject == null
                ? -1
                : _objectState.WeakReferencesOnObjects
                    .FindIndex(reference =>
                        reference.TryGetTarget(out object obj) &&
                        object.ReferenceEquals(obj, currentObject));

        private static PropertyParameters GetParameters(
            SerializedProperty property, DrawerParameters drawerParameters
        ) => GetParameters(property, GetOrCreateDataReference(drawerParameters));

        private static PropertyParameters GetParameters(SerializedProperty property, ReferenceData referenceData)
        {
            object managedReferenceValue = property.GetManagedReferenceValueFromPropertyPath();
            var fieldInfo = property.GetFieldInfoFromPropertyPath();
            var fieldType = fieldInfo.field.FieldType;

            var objectType = fieldInfo.serializedPropertyFieldType == SerializedPropertyFieldType.ArrayElement
                ? fieldType.IsArray
                    ? fieldType.GetElementType()
                    : fieldType.GetGenericArguments()[0]
                : fieldType;
            
            if (_objectState.SerializedObject == null)
                _objectState.SerializedObject = property.serializedObject;

            bool isNotAssignable = objectType.IsAssignableFrom(managedReferenceValue?.GetType()) == false;
            bool isSuchPropertyNotAlreadyExisted = _objectState.PropertiesNotMetFirstTime.Add(property.propertyPath);

            if (managedReferenceValue != null && (isNotAssignable || isSuchPropertyNotAlreadyExisted))
            {
                if (GetIndexObject(managedReferenceValue) != -1)
                {
                    managedReferenceValue = null;
                    SetValueAndSaveProperty(property, null);
                }
                else
                {
                    AddObject(new PropertyParameters(property, referenceData, managedReferenceValue));
                }
            }

            return new PropertyParameters(property, referenceData, managedReferenceValue);
        }

        private static void ChangeManagedReferenceValue(ref PropertyParameters parameters, int indexInPopup)
        {
            SerializedProperty property = parameters.Property;

            if (indexInPopup == parameters.Data.IndexNullVariable)
            {
                SetValueAndSaveProperty(property, null);
                parameters = new PropertyParameters(property, parameters.Data, null);
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
            PropertyParameters parameters, int indexInPopup, out object newManagedReference)
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
                newManagedReference = Activator.CreateInstance(typeNewManagedReference);

                var canChangeSerializeReference = newManagedReference as ISerializeReferenceChangeValidate;
                if (canChangeSerializeReference == null || canChangeSerializeReference.Validate(out string textError))
                {
                    ReflectionUtilities.CopyFieldsFromSourceToDestination(oldManagedReference, newManagedReference);
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
            if (parameters.IsExpanded)
            {
                var drawerType = EditorGUIUtilityWithReflection.GetDrawerTypeForType(
                    parameters.Property.GetManagedReferenceValueFromPropertyPath().GetType());

                if (drawerType != null)
                    actionFromDrawer((PropertyDrawer)Activator.CreateInstance(drawerType));
                else
                    foreach (var children in parameters.Property.GetChildren())
                        actionFromChildren(children);
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