using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChoiceReference.Editor.Parameters;
using Paulsams.MicsUtils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChoiceReference.Editor
{
    public struct ChoiceReferenceDrawerParameters
    {
        public readonly FieldInfo FieldInfo;
        public readonly IChoiceReferenceParameters DrawParameters;

        public ChoiceReferenceDrawerParameters(FieldInfo fieldInfo, IChoiceReferenceParameters drawParameters)
        {
            FieldInfo = fieldInfo;
            DrawParameters = drawParameters;
        }
        
        public ChoiceReferenceDrawerParameters(SerializedProperty property, IChoiceReferenceParameters drawParameters)
        {
            FieldInfo = property.GetFieldInfoFromPropertyPath().field;
            DrawParameters = drawParameters;
        }
    }
    
    public static class ChoiceReferenceDrawer
    {
        private struct KeyForReference
        {
            public readonly object Value;
            public readonly object Parent;

            public KeyForReference(object value, object parent)
            {
                Value = value;
                Parent = parent;
            }

            public override bool Equals(object obj)
            {
                return obj is KeyForReference other &&
                       Value == other.Value && Parent == other.Parent;
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode() + (Parent == null ? 1423542 : Parent.GetHashCode());
            }
        }
        
        private static readonly Dictionary<KeyForReference, BaseParameters> _parameters = new Dictionary<KeyForReference, BaseParameters>();
        private static readonly Dictionary<FieldInfo, ReferenceData> _dataReferences = new Dictionary<FieldInfo, ReferenceData>();
        private static readonly HashSet<object> _objects = new HashSet<object>();

        #region Remove Deleted Parameters
        private static readonly List<KeyForReference> _unusedParameters = new List<KeyForReference>();
        private static readonly FieldInfo _serializedObjectObjectPtrFieldInfo;

        private static bool _isCalledOnGui;

        static ChoiceReferenceDrawer()
        {
            _serializedObjectObjectPtrFieldInfo = typeof(SerializedObject).GetField("m_NativeObjectPtr",
                BindingFlags.Instance | BindingFlags.NonPublic);

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
                RemoveReference(_unusedParameters[i]);

            _unusedParameters.Clear();
        }

        private static void CheckUnusedParameters()
        {
            foreach (var parameterPair in _parameters)
            {
                bool isValidSerializedObject = (IntPtr)_serializedObjectObjectPtrFieldInfo.GetValue(
                    parameterPair.Value.Property.serializedObject) != IntPtr.Zero;

                if (isValidSerializedObject && parameterPair.Value.IsDrawn == false)
                    _unusedParameters.Add(parameterPair.Key);

                parameterPair.Value.IsDrawn = false;
            }
        }
        
        private static KeyForReference GetKey(BaseParameters parameters)
        {
            return new KeyForReference(parameters.ManagedReferenceValue,
                parameters.Property.GetFieldInfoFromPropertyPath().parentObject);
        }
        #endregion

        public static float GetPropertyHeight(SerializedProperty property, ChoiceReferenceDrawerParameters drawerParameters)
        {
            BaseParameters parameters = GetParameters(property, drawerParameters);

            float height = EditorGUIUtility.singleLineHeight;

            LoopFromChildren(parameters, (children) =>
            {
                height += EditorGUI.GetPropertyHeight(children, true) + EditorGUIUtility.standardVerticalSpacing;
            });

            return height;
        }

        public static void DrawOnGUI(Rect position, SerializedProperty property, GUIContent label,
            ChoiceReferenceDrawerParameters drawerParameters)
        {
            _isCalledOnGui = true;
            DrawManagedReference(property, label.text, position, drawerParameters);
        }
        
        public static VisualElement CreateVisualElement(SerializedProperty property, string label,
            ChoiceReferenceDrawerParameters drawerParameters)
        {
            BaseParameters parameters = GetParameters(property, drawerParameters);
            
            Foldout foldout = new Foldout();
            foldout.text = label;
            Toggle toggle = foldout.Q<Toggle>();
            VisualElement checkmark = toggle.Q<VisualElement>("unity-checkmark");
            void UpdateCheckmark(BaseParameters currentParameters) =>
                checkmark.style.visibility = currentParameters.IsHaveFoldout
                ? Visibility.Visible
                : Visibility.Hidden;
            
            toggle.style.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            toggle[0].style.flexGrow = toggle[0].style.flexGrow = 0f;
            // Taken from BaseField<>.m_LabelWidthRatio
            toggle[0].style.width = new Length(45f, LengthUnit.Percent);
            foldout.schedule.Execute((_) =>
            {
                // Don't ask why. It almost works and fine.
                // TODO: But it's worth understanding better.
                toggle[0].style.marginRight = -16f + -7.5f * ((int)foldout.worldBound.x / 15);
            });
            UpdateCheckmark(parameters);
            
            var containerProperties = new VisualElement();
            foldout.Add(containerProperties);
            
            void DrawChildren(BaseParameters currentParameters)
            {
                LoopFromChildren(currentParameters, (children) =>
                {
                    PropertyField field = new PropertyField(children);
                    field.BindProperty(children);
                    containerProperties.Add(field);
                });
            }

            var popup = new DropdownField(parameters.Data.TypesNames.ToList(), parameters.IndexInPopup);
            popup.RegisterValueChangedCallback((_) =>
            {
                BaseParameters currentParameters = GetParameters(property, drawerParameters);
                if (popup.index != currentParameters.IndexInPopup)
                {
                    containerProperties.Clear();
                    ChangeManagedReferenceValue(ref currentParameters, popup.index);
                    DrawChildren(currentParameters);
                    UpdateCheckmark(currentParameters);
                    foldout.value = true;
                }
            });
            popup.style.flexGrow = 1;
            toggle.Add(popup);

            DrawChildren(parameters);

            foldout.schedule.Execute((_) =>
            {
                BaseParameters currentParameters = GetParameters(property, drawerParameters);
                currentParameters.IsDrawn = true;
                _isCalledOnGui = true;
            }).Every(50);
            
            return foldout;
        }

        private static void AddReference(KeyForReference key, BaseParameters parameters)
        {
            _parameters.Add(key, parameters);
            _objects.Add(parameters.ManagedReferenceValue);
        }

        private static void RemoveReference(KeyForReference key)
        {
            if (_parameters.TryGetValue(key, out var parameters))
            {
                _parameters.Remove(key);
                _objects.Remove(parameters.ManagedReferenceValue);
            }
        }

        private static void DrawManagedReference(SerializedProperty property, string label, Rect rect,
            ChoiceReferenceDrawerParameters drawerParameters)
        {
            BaseParameters parameters = GetParameters(property, drawerParameters);

            rect.height = EditorGUIUtility.singleLineHeight;
            Rect rectLabel = rect;

            parameters.DrawLabel(label, rectLabel);

            int indexInPopup = DrawPopupAndGetIndex(parameters, rect);
            if (indexInPopup != parameters.IndexInPopup)
                ChangeManagedReferenceValue(ref parameters, indexInPopup);

            DrawProperty(parameters, rect);

            parameters.IsDrawn = true;
        }

        private static BaseParameters GetParameters(SerializedProperty property,
            ChoiceReferenceDrawerParameters drawerParameters)
        {
            object managedReferenceValue = property.GetManagedReferenceValueFromPropertyPath();
            var parent = property.GetFieldInfoFromPropertyPath().parentObject;
            var key = new KeyForReference(managedReferenceValue, parent);

            BaseParameters parameters;
            if (managedReferenceValue == null)
            {
                parameters = new ParametersForNullReference(GetOrCreateDataReference(drawerParameters));
            }
            else if (_parameters.TryGetValue(key, out parameters) == false)
            {
                if (_objects.Contains(managedReferenceValue))
                {
                    property.managedReferenceValue = null;
                    parameters = new ParametersForNullReference(GetOrCreateDataReference(drawerParameters));
                }
                else
                {
                    parameters = new ParametersForReference(GetOrCreateDataReference(drawerParameters), managedReferenceValue);
                    AddReference(key, parameters);
                }
            }
            
            parameters.Property = property;

            return parameters;
        }

        private static void ChangeManagedReferenceValue(ref BaseParameters parameters, int indexInPopup)
        {
            SerializedProperty property = parameters.Property;
            object newManagedReference = null;

            if (indexInPopup == parameters.Data.IndexNullVariable)
            {
                RemoveReference(GetKey(parameters));

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
                        RemoveReference(GetKey(parameters));
                        objectParameters.SetNewManagedReferenceValue(newManagedReference, indexInPopup);
                    }
                    else
                    {
                        objectParameters = new ParametersForReference(parameters.Data, newManagedReference, indexInPopup);
                        objectParameters.Property = property;
                    }

                    parameters = objectParameters;
                    
                    property.managedReferenceValue = newManagedReference;
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();

                    AddReference(GetKey(objectParameters), objectParameters);
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

                LoopFromChildren(parameters, (children) =>
                {
                    EditorGUI.PropertyField(rectField, children, true);
                    rectField.y += EditorGUI.GetPropertyHeight(children, true) + EditorGUIUtility.standardVerticalSpacing;
                });
            }
            --EditorGUI.indentLevel;
        }

        private static object CreateManagedReferenceValueAndCopyFields(BaseParameters parameters, int indexInPopup, ref bool changeReference)
        {
            parameters.Property.serializedObject.Update();

            object oldManagedReference = parameters.ManagedReferenceValue;
            int indexChoiceType = indexInPopup;

            //I don't want to add the first element to Types as null, so I make such a crutch.
            if (parameters.Data.DrawParameters.Nullable)
                indexChoiceType -= 1;

            Type typeNewManagedReference = parameters.Data.Types[indexChoiceType];
            object newManagedReference = Activator.CreateInstance(typeNewManagedReference);

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

        private static void LoopFromChildren(BaseParameters parameters, Action<SerializedProperty> action)
        {
            if (parameters.IsHaveFoldout)
                foreach (var children in parameters.Property.GetChildrens())
                    action(children);
        }

        private static ReferenceData GetOrCreateDataReference(ChoiceReferenceDrawerParameters drawerParameters)
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
