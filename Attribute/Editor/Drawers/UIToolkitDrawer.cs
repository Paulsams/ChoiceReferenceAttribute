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
                    containerProperties,
                    () => GetParameters(property, drawerParameters),
                    label,
                    (currentParameters) => RemoveObject(currentParameters),
                    (currentParameters) =>
                    {
                        AddObject(currentParameters);
                        UpdateCheckmark(currentParameters);
                        foldout.value = true;
                    });
                
                containerOnSameRowWithToggle.Add(popup);
                
                return foldout;
            }

            public static DropdownField CreateDropdown(VisualElement containerProperties,
                Func<PropertyParameters> getterParameters,
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
                
                PropertyParameters parameters = getterParameters();
                
                var popup = new DropdownField(parameters.Data.TypesNames.ToList(), parameters.IndexInPopup);
                popup.RegisterValueChangedCallback((_) =>
                {
                    PropertyParameters currentParameters = getterParameters();
                    if (popup.index == currentParameters.IndexInPopup)
                        return;
                    
                    valueBeforeChangeCallback?.Invoke(currentParameters);
                    ChangeManagedReferenceValue(ref currentParameters, popup.index);
                    if (popup.index != currentParameters.IndexInPopup)
                    {
                        popup.index = currentParameters.IndexInPopup;
                        return;
                    }

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