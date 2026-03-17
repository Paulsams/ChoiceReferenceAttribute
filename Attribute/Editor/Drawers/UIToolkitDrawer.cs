using System;
using System.Linq;
using Paulsams.MicsUtils.ChoiceReference.Editor.Parameters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Paulsams.MicsUtils.ChoiceReference.Editor.Drawers
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
                foldout.BindProperty(property);
                VisualElementsUtilities.SetAlignedLabelFromFoldout(foldout, out var containerOnSameRowWithToggle,
                    out VisualElement checkmark);

                void UpdateCheckmark(PropertyParameters currentParameters) =>
                    checkmark.style.visibility = currentParameters.MayExpanded
                        ? Visibility.Visible
                        : Visibility.Hidden;

                UpdateCheckmark(parameters);

                var popup = CreateDropdown(
                    property,
                    foldout.contentContainer,
                    () => drawerParameters,
                    label,
                    (_) => { },
                    UpdateCheckmark);

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
                                var guiContent =
                                    new GUIContent(label == null ? currentParameters.Property.displayName : label);
                                container = new IMGUIContainer(() => drawer.OnGUI(containerProperties.contentRect,
                                    currentParameters.Property, guiContent));
                                container.style.height =
                                    drawer.GetPropertyHeight(currentParameters.Property, guiContent);
                            }

                            containerProperties.Add(container);
                        },
                        (children) =>
                        {
                            containerProperties.Add(new PropertyField(children)
                            {
                                bindingPath = children.name
                            });
                        });
                }

                PropertyParameters parameters = GetParameters(property, getterDrawerParameters());

                var popup = new DropdownField(parameters.Data.TypesNames.ToList(), parameters.IndexInPopup);
                popup.RegisterValueChangedCallback((_) =>
                {
                    PropertyParameters currentParameters = GetParameters(property, getterDrawerParameters());
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