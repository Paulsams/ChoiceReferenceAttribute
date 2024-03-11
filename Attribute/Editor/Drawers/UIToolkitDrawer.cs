using System;
using System.Linq;
using ChoiceReference.Editor.Parameters;
using Paulsams.MicsUtils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChoiceReference.Editor.Drawers
{
    public static partial class ChoiceReferenceDrawer
    {
        public static class UIToolkit
        {
            public static VisualElement Create(SerializedProperty property, string label,
                DrawerParameters drawerParameters)
            {
                PropertyParameters parameters = GetParameters(property, drawerParameters);
                
                Foldout foldout = new Foldout();
                foldout.text = label;
                foldout.contentContainer.style.marginBottom = 1;
                VisualElementsUtilities.SetAlignedLabelFromFoldout(foldout, out var containerOnSameRowWithToggle,
                    out VisualElement checkmark);
                void UpdateCheckmark(PropertyParameters currentParameters) =>
                    checkmark.style.visibility = currentParameters.IsExpanded
                    ? Visibility.Visible
                    : Visibility.Hidden;

                UpdateCheckmark(parameters);
                
                var containerProperties = new VisualElement();
                foldout.Add(containerProperties);
                var popup = CreateDropdown(
                    property,
                    containerProperties,
                    () => drawerParameters,
                    label,
                    (_) => { },
                    (currentParameters) =>
                    {
                        UpdateCheckmark(currentParameters);
                        foldout.value = true;
                    });
                
                containerOnSameRowWithToggle.Add(popup);
                
                return foldout;
            }

            public static DropdownField CreateDropdown(
                SerializedProperty property,
                VisualElement containerProperties,
                Func<DrawerParameters> getterDrawerParameters,
                string label = null,
                Action<PropertyParameters> valueBeforeChangeCallback = null,
                Action<PropertyParameters> valueAfterChangeCallback = null)
            {
                void DrawChildren(PropertyParameters currentParameters)
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
                
                PropertyParameters parameters = GetParameters(property, getterDrawerParameters());
                ObjectState state = GetOrCreateObjectState(parameters.Property);
                
                var popup = new DropdownField(parameters.Data.TypesNames.ToList(), parameters.IndexInPopup);
                popup.RegisterValueChangedCallback((_) =>
                {
                    PropertyParameters currentParameters = GetParameters(property, getterDrawerParameters());
                    if (popup.index == currentParameters.IndexInPopup)
                        return;
                    
                    RemoveObject(currentParameters);
                    valueBeforeChangeCallback?.Invoke(currentParameters);
                    ChangeManagedReferenceValue(ref currentParameters, state, popup.index);
                    if (popup.index != currentParameters.IndexInPopup)
                    {
                        AddObject(currentParameters);
                        popup.index = currentParameters.IndexInPopup;
                        return;
                    }

                    AddObject(currentParameters);
                    containerProperties.Clear();
                    DrawChildren(currentParameters);
                    valueAfterChangeCallback?.Invoke(currentParameters);
                });
                popup.style.flexGrow = 1;
                DrawChildren(parameters);
                
                return popup;
            }
        }
    }
}