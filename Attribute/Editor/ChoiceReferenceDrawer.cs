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
                return (Value == null ? 0 : Value.GetHashCode()) + (Parent == null ? 1423542 : Parent.GetHashCode());
            }
        }
        
        private static readonly Dictionary<KeyForReference, BaseParameters> _parameters = new Dictionary<KeyForReference, BaseParameters>();
        private static readonly Dictionary<FieldInfo, ReferenceData> _dataReferences = new Dictionary<FieldInfo, ReferenceData>();
        private static readonly HashSet<object> _objects = new HashSet<object>();

        #region Remove Deleted Parameters
        private static readonly List<KeyForReference> _unusedParameters = new List<KeyForReference>();
        private static readonly FieldInfo _serializedObjectObjectPtrFieldInfo;

        private static bool _isCheckUnused;
        private static bool _didPreviousCalledUpdate;

        static ChoiceReferenceDrawer()
        {
            _serializedObjectObjectPtrFieldInfo = typeof(SerializedObject).GetField("m_NativeObjectPtr",
                BindingFlags.Instance | BindingFlags.NonPublic);

            EditorApplication.update += CoroutineForCollectUnusedParameters;
        }

        private static void CoroutineForCollectUnusedParameters()
        {
            // I didn't figure out how best to check for unused parameters ones on the next frame when everything stopped rendering.
            // Let's at least do this.
            if (_isCheckUnused)
            {
                _isCheckUnused = false;
                _didPreviousCalledUpdate = true;
                
                CollectUnusedParameters();
            } else if (_didPreviousCalledUpdate)
            {
                CollectUnusedParameters();
                _didPreviousCalledUpdate = false;
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
            foreach (var (key, parameters) in _parameters)
            {
                bool isValidSerializedObject = (IntPtr)_serializedObjectObjectPtrFieldInfo.GetValue(
                    parameters.Property.serializedObject) != IntPtr.Zero;

                if (isValidSerializedObject == false || parameters.DrawnType == DrawnChoiceReferenceType.NotDrawn)
                    _unusedParameters.Add(key);

                if (parameters.DrawnType == DrawnChoiceReferenceType.OnGUI)
                    parameters.DrawnType = DrawnChoiceReferenceType.NotDrawn;
            }
        }
        
        private static KeyForReference GetKey(BaseParameters parameters)
        {
            return new KeyForReference(parameters.ManagedReferenceValue,
                parameters.Property.GetFieldInfoFromPropertyPath().parentObject);
        }
        #endregion

        public static class OnGUI
        {
            public static float GetPropertyHeight(SerializedProperty property, GUIContent label,
                ChoiceReferenceDrawerParameters drawerParameters)
            {
                BaseParameters parameters = GetParameters(property, drawerParameters);

                float height = EditorGUIUtility.singleLineHeight;

                DrawFromPropertyDrawerOrLoopFromChildren(parameters,
                    (drawer) =>
                    {
                        height += drawer.GetPropertyHeight(parameters.Property, label) + EditorGUIUtility.standardVerticalSpacing;
                    },
                    (children) =>
                    {
                        height += EditorGUI.GetPropertyHeight(children, true) + EditorGUIUtility.standardVerticalSpacing;
                    });

                return height;
            }
            
            public static void Draw(Rect position, SerializedProperty property, GUIContent label,
                ChoiceReferenceDrawerParameters drawerParameters)
            {
                _isCheckUnused = true;
                DrawManagedReference(property, label, position, drawerParameters);
            }
            
            private static void DrawManagedReference(SerializedProperty property, GUIContent label, Rect rect,
                ChoiceReferenceDrawerParameters drawerParameters)
            {
                BaseParameters parameters = GetParameters(property, drawerParameters);
                parameters.DrawnType = DrawnChoiceReferenceType.OnGUI;

                rect.height = EditorGUIUtility.singleLineHeight;
                Rect rectLabel = rect;

                parameters.DrawLabel(label.text, rectLabel);

                int indexInPopup = DrawPopupAndGetIndex(parameters, rect);
                if (indexInPopup != parameters.IndexInPopup)
                {
                    RemoveReference(GetKey(parameters));
                    ChangeManagedReferenceValue(ref parameters, indexInPopup);
                    if (parameters is ParametersForReference)
                        AddReference(GetKey(parameters), parameters);
                }

                DrawProperty(parameters, label, rect);
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

            private static void DrawProperty(BaseParameters parameters, GUIContent label, Rect rect)
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
                            rectField.y += EditorGUI.GetPropertyHeight(children, true) + EditorGUIUtility.standardVerticalSpacing;
                        });
                }
                --EditorGUI.indentLevel;
            }
        }

        public static class UIToolkit
        {
            public static VisualElement Create(SerializedProperty property, string label,
                ChoiceReferenceDrawerParameters drawerParameters)
            {
                BaseParameters parameters = GetParameters(property, drawerParameters);
                parameters.DrawnType = DrawnChoiceReferenceType.UIToolkit;
                
                Foldout foldout = new Foldout();
                foldout.text = label;
                foldout.contentContainer.style.marginBottom = 1;
                VisualElementsUtilities.SetAlignedLabelFromFoldout(foldout, out VisualElement containerOnSameRowWithToggle,
                    out VisualElement checkmark);
                void UpdateCheckmark(BaseParameters currentParameters) =>
                    checkmark.style.visibility = currentParameters.IsHaveFoldout
                    ? Visibility.Visible
                    : Visibility.Hidden;

                UpdateCheckmark(parameters);
                
                var containerProperties = new VisualElement();
                foldout.Add(containerProperties);
                var popup = CreateDropdown(
                    containerProperties,
                    () => GetParameters(property, drawerParameters),
                    label,
                    (currentParameters) => RemoveReference(GetKey(currentParameters)),
                    (currentParameters) =>
                    {
                        AddReference(GetKey(currentParameters), currentParameters);
                        UpdateCheckmark(currentParameters);
                        foldout.value = true;
                    });
                
                containerOnSameRowWithToggle.Add(popup);
                
                foldout.RegisterCallback<DetachFromPanelEvent>(_ =>
                {
                    Debug.Log("Detach");
                    _isCheckUnused = true;
                });
                
                return foldout;
            }

            public static DropdownField CreateDropdown(VisualElement containerProperties,
                Func<BaseParameters> getterParameters,
                string label = null,
                Action<BaseParameters> valueBeforeChangeCallback = null,
                Action<BaseParameters> valueAfterChangeCallback = null)
            {
                void DrawChildren(BaseParameters currentParameters)
                {
                    DrawFromPropertyDrawerOrLoopFromChildren(currentParameters,
                        (drawer) =>
                        {
                            var container = drawer.CreatePropertyGUI(currentParameters.Property);
                            if (container == null)
                            {
                                var guiContent = new GUIContent(label == null ? currentParameters.Property.displayName : label);
                                container = new IMGUIContainer(() => drawer.OnGUI(containerProperties.contentRect,
                                    currentParameters.Property, guiContent));
                                container.style.height = drawer.GetPropertyHeight(currentParameters.Property, guiContent);
                            }

                            containerProperties.Add(container);
                        },
                        (children) =>
                        {
                            PropertyField field = new PropertyField(children);
                            field.BindProperty(children);
                            containerProperties.Add(field);
                        });
                }
                
                BaseParameters parameters = getterParameters();
                
                var popup = new DropdownField(parameters.Data.TypesNames.ToList(), parameters.IndexInPopup);
                popup.RegisterValueChangedCallback((_) =>
                {
                    BaseParameters currentParameters = getterParameters();
                    if (popup.index != currentParameters.IndexInPopup)
                    {
                        valueBeforeChangeCallback?.Invoke(currentParameters);
                        ChangeManagedReferenceValue(ref currentParameters, popup.index);
                        currentParameters.DrawnType = DrawnChoiceReferenceType.UIToolkit;
                        containerProperties.Clear();
                        DrawChildren(currentParameters);
                        valueAfterChangeCallback?.Invoke(currentParameters);
                    }
                });
                popup.style.flexGrow = 1;
                DrawChildren(parameters);
                
                return popup;
            }
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

        private static BaseParameters GetParameters(SerializedProperty property,
            ChoiceReferenceDrawerParameters drawerParameters) =>
            GetParameters(property, GetOrCreateDataReference(drawerParameters));

        private static BaseParameters GetParameters(SerializedProperty property, ReferenceData referenceData)
        {
            object managedReferenceValue = property.GetManagedReferenceValueFromPropertyPath();
            var parent = property.GetFieldInfoFromPropertyPath().parentObject;
            var key = new KeyForReference(managedReferenceValue, parent);

            BaseParameters parameters;
            if (managedReferenceValue == null)
            {
                parameters = new ParametersForNullReference(property, referenceData);
            }
            else if (_parameters.TryGetValue(key, out parameters) == false)
            {
                if (_objects.Contains(managedReferenceValue))
                {
                    property.managedReferenceValue = null;
                    parameters = new ParametersForNullReference(property, referenceData);
                }
                else
                {
                    parameters = new ParametersForReference(property, referenceData, managedReferenceValue);
                    AddReference(key, parameters);
                }
            }

            return parameters;
        }

        private static void ChangeManagedReferenceValue(ref BaseParameters parameters, int indexInPopup)
        {
            SerializedProperty property = parameters.Property;
            
            if (indexInPopup == parameters.Data.IndexNullVariable)
            {
                parameters = new ParametersForNullReference(property, parameters.Data);
            }
            else
            {
                bool changeReference = true;
                var newManagedReference = CreateManagedReferenceValueAndCopyFields(parameters, indexInPopup, ref changeReference);

                if (changeReference)
                {
                    if (parameters is ParametersForReference objectParameters)
                    {
                        objectParameters.SetNewManagedReferenceValue(newManagedReference, indexInPopup);
                    }
                    else
                    {
                        objectParameters = new ParametersForReference(property, parameters.Data, newManagedReference, indexInPopup);
                    }

                    parameters = objectParameters;
                }
            }

            property.serializedObject.ApplyModifiedProperties();
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

        private static void DrawFromPropertyDrawerOrLoopFromChildren(BaseParameters parameters,
            Action<PropertyDrawer> actionFromDrawer,
            Action<SerializedProperty> actionFromChildren)
        {
            if (parameters.IsHaveFoldout)
            {
                var drawerType = EditorGUIUtilityWithReflection.GetDrawerTypeForType(
                    parameters.Property.GetManagedReferenceValueFromPropertyPath().GetType());
                if (drawerType != null)
                {
                    // TODO: Perhaps it is worth set fieldInfo and preferredLabel?
                    actionFromDrawer((PropertyDrawer) Activator.CreateInstance(drawerType));
                }
                else
                {
                    foreach (var children in parameters.Property.GetChildren())
                        actionFromChildren(children);
                }
            }
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
