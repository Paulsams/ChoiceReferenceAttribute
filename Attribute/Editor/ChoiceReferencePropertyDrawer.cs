using Paulsams.MicsUtils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ChoiceReferenceEditor
{
    public static class ChoiceReferencePropertyDrawer
    {
        private readonly static Dictionary<object, BaseParameters> _parameters = new Dictionary<object, BaseParameters>();
        private readonly static Dictionary<FieldInfo, ReferenceData> _dataReferences = new Dictionary<FieldInfo, ReferenceData>();

        #region Remove Deleted Parameters
        private readonly static List<object> _unusedParameters = new List<object>();
        private readonly static FieldInfo _serializedOjbectObjectPtrFieldInfo;

        private static bool _isCalledOnGui;

        static ChoiceReferencePropertyDrawer()
        {
            _serializedOjbectObjectPtrFieldInfo = typeof(SerializedObject).GetField("m_NativeObjectPtr", BindingFlags.Instance | BindingFlags.NonPublic);

            EditorApplication.update += CoroutineForCollectUnusedParameters;
        }

        private static void CoroutineForCollectUnusedParameters()
        {
            if (_isCalledOnGui)
            {
                CollectUnusedParameters();
                _isCalledOnGui = false;
            }
        }

        private static void CollectUnusedParameters()
        {
            CheckUnusedParameters();

            for (int i = 0; i < _unusedParameters.Count; ++i)
                _parameters.Remove(_unusedParameters[i]);

            _unusedParameters.Clear();
        }

        private static void CheckUnusedParameters()
        {
            foreach (var parameterPair in _parameters)
            {
                bool isValidSerializedObject = (IntPtr)_serializedOjbectObjectPtrFieldInfo.GetValue(parameterPair.Value.Property.serializedObject) != IntPtr.Zero;

                if (isValidSerializedObject && parameterPair.Value.IsDrawed == false)
                    _unusedParameters.Add(parameterPair.Key);

                parameterPair.Value.IsDrawed = false;
            }
        }
        #endregion

        public static void DrawManagedReference(SerializedProperty property, string label, Rect rect)
        {
            _isCalledOnGui = true;

            BaseParameters parameters = GetParameters(property);

            rect.height = EditorGUIUtility.singleLineHeight;
            Rect rectLabel = rect;

            parameters.DrawLabel(label, rectLabel);

            int indexInPopup = DrawPopupAndGetIndex(parameters, rect);
            if (indexInPopup != parameters.IndexInPopup)
                ChangeManagedReferenceValue(ref parameters, indexInPopup);

            DrawProperty(parameters, rect);

            parameters.IsDrawed = true;
        }

        public static BaseParameters GetParameters(SerializedProperty property)
        {
            object managedReferenceValue = property.GetManagedReferenceValueFromPropertyPath();

            BaseParameters parameters;
            if (managedReferenceValue == null)
            {
                parameters = new ParametersForNullReference(GetOrCreateDataReference(property.GetFieldInfoFromPropertyPath().field));
            }
            else if (_parameters.TryGetValue(managedReferenceValue, out parameters) == false)
            {
                parameters = new ParametersForReference(GetOrCreateDataReference(property.GetFieldInfoFromPropertyPath().field), managedReferenceValue);
                _parameters.Add(managedReferenceValue, parameters);
            }

            parameters.Property = property;

            return parameters;
        }

        public static void LoopFromChildrens(BaseParameters parameters, Action<SerializedProperty> action)
        {
            if (parameters.Foldout)
            {
                foreach (var children in parameters.Property.GetChildrens())
                {
                    action(children);
                }
            }
        }

        private static void ChangeManagedReferenceValue(ref BaseParameters parameters, int indexInPopup)
        {
            SerializedProperty property = parameters.Property;
            object newManagedReference = null;

            if (indexInPopup == parameters.Data.IndexNullVariable)
            {
                _parameters.Remove(parameters.ManagedReferenceValue);

                parameters = new ParametersForNullReference(parameters.Data);
                parameters.Property = property;
            }
            else
            {
                bool changeReference = true;
                newManagedReference = CreateManagedReferenceValueAndCopyFields(parameters, indexInPopup, ref changeReference);

                if (changeReference)
                {
                    if (parameters is ParametersForReference objectParameters)
                    {
                        _parameters.Remove(parameters.ManagedReferenceValue);
                        objectParameters.SetNewManagedReferenceValue(newManagedReference, indexInPopup);
                    }
                    else
                    {
                        objectParameters = new ParametersForReference(parameters.Data, newManagedReference, indexInPopup);
                    }

                    _parameters.Add(newManagedReference, objectParameters);
                }
            }

            property.managedReferenceValue = newManagedReference;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static int DrawPopupAndGetIndex(BaseParameters parameters, Rect rect)
        {
            Rect rectPopup = rect;
            float offset = EditorGUIUtility.labelWidth + 2f - EditorGUI.indentLevel * 15f;
            rectPopup.x += offset;
            rectPopup.width -= offset;

            int indexInPopup = EditorGUI.Popup(rectPopup, parameters.IndexInPopup, parameters.Data.TypesNames);
            return indexInPopup;
        }

        private static void DrawProperty(BaseParameters parameters, Rect rect)
        {
            ++EditorGUI.indentLevel;
            {
                Rect rectField = rect;
                rectField.y += rect.height + EditorGUIUtility.standardVerticalSpacing;

                LoopFromChildrens(parameters, (children) =>
                {
                    EditorGUI.PropertyField(rectField, children, true);
                    rectField.y += EditorGUI.GetPropertyHeight(children, true) + EditorGUIUtility.standardVerticalSpacing;
                });
            }
            --EditorGUI.indentLevel;
        }

        private static object CreateManagedReferenceValueAndCopyFields(BaseParameters parameters, int indexInPopup, ref bool changeReference)
        {
            object newManagedReference;
            parameters.Property.serializedObject.Update();

            object oldManagedReference = parameters.ManagedReferenceValue;
            int indexChoiceType = indexInPopup;

            //I don't want to add the first element to Types as null, so I make such a crutch.
            //if (parameters.Data.Attribute.Nullable)
            //    indexChoiceType -= 1;

            Type typeNewManagedReference = parameters.Data.Types[indexChoiceType];
            newManagedReference = Activator.CreateInstance(typeNewManagedReference);

            var canChangeSerializeReference = newManagedReference as ISerializeReferenceChangeValidate;
            if (canChangeSerializeReference == null || canChangeSerializeReference.Validate(out string textError))
            {
                ReflectionUtilities.CopyFieldsFromSourceToDestination(oldManagedReference, newManagedReference);
            }
            else
            {
                EditorUtility.DisplayDialog($"Cannot be changed to {typeNewManagedReference}", textError, "Ok");
                changeReference = false;
                return null;
            }

            return newManagedReference;
        }

        private static ReferenceData GetOrCreateDataReference(FieldInfo fieldInfo)
        {
            if (_dataReferences.TryGetValue(fieldInfo, out ReferenceData data) == false)
            {
                data = new ReferenceData(fieldInfo.FieldType);
                _dataReferences.Add(fieldInfo, data);
            }
            return data;
        }
    }
}