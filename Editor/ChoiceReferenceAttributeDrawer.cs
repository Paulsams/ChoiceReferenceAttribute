using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.ObjectModel;
using Paulsams.MicsUtil;

[CustomPropertyDrawer(typeof(ChoiceReferenceAttribute))]
public class ChoiceReferenceAttributeDrawer : PropertyDrawer
{
    private class Parameters
    {
        public readonly SerializedProperty Property;
        public readonly DataReference Data;

        public bool Foldout;
        public int IndexChoicedType;
        public object ManagedReferenceValue;

        public Parameters(SerializedProperty property, DataReference data)
        {
            Property = property;
            Data = data;

            ManagedReferenceValue = Property.GetManagedReferenceValueFromPropertyPath();

            if (ManagedReferenceValue != null)
                IndexChoicedType = Array.IndexOf(Data.TypesNames, ManagedReferenceValue.GetType().Name);
            else
                IndexChoicedType = Data.IndexNullVariable;
        }
    }

    private class DataReference
    {
        private const string _nullableNameInPopup = "None";

        //An array is declared here, because EditorGUI.Popup accepts only an array.
        public readonly string[] TypesNames;
        public readonly ReadOnlyCollection<Type> Types;
        public readonly ReadOnlyCollection<string> IgnoreNameProperties;

        public readonly ChoiceReferenceAttribute Attribute;
        public readonly int IndexNullVariable;

        public DataReference(Type fieldType, ChoiceReferenceAttribute choiceReferenceAttribute)
        {
            IgnoreNameProperties = choiceReferenceAttribute.IgnoreNameProperties;

            Type typeProperty = ReflectionUtilities.GetArrayOrListElementTypeOrThisType(fieldType);
            Types = ReflectionUtilities.GetFinalAssignableTypesFromAllTypes(typeProperty);
            List<string> typesNames = Types.Select(type => type.Name).ToList();

            if (choiceReferenceAttribute.Nullable)
                typesNames.Insert(0, _nullableNameInPopup);

            TypesNames = typesNames.ToArray();

            Attribute = choiceReferenceAttribute;
            IndexNullVariable = choiceReferenceAttribute.Nullable ? 0 : -1;
        }
    }

    private readonly static Dictionary<string, Parameters> _parameters = new Dictionary<string, Parameters>();
    private readonly static Dictionary<FieldInfo, DataReference> _dataReferences = new Dictionary<FieldInfo, DataReference>();

    #region Collect Unused Parameters
    private readonly static List<string> _unusedParameters = new List<string>();
    private readonly static FieldInfo _serializedOjbectObjectPtrFieldInfo;
    private readonly static PropertyInfo _isValidPropertyInfo;

    private static bool _isCalledOnGui;

    static ChoiceReferenceAttributeDrawer()
    {
        _serializedOjbectObjectPtrFieldInfo = typeof(SerializedObject).GetField("m_NativeObjectPtr", BindingFlags.Instance | BindingFlags.NonPublic);
        _isValidPropertyInfo = typeof(SerializedProperty).GetProperty("isValid", BindingFlags.Instance | BindingFlags.NonPublic);

        EditorApplication.update += CoroutineForCollectUnusedParameters;
    }

    private static void CoroutineForCollectUnusedParameters()
    {
        if (_isCalledOnGui || _parameters.Count != 0)
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

            if (isValidSerializedObject == false)
            {
                _unusedParameters.Add(parameterPair.Key);
            }
            else
            {
                bool isValidProperty = (bool)_isValidPropertyInfo.GetValue(parameterPair.Value.Property);

                if (isValidProperty == false)
                    _unusedParameters.Add(parameterPair.Key);
            }
        }
    }
    #endregion

    private string GetKeyForParameter(SerializedProperty property) => property.serializedObject.targetObject.GetInstanceID().ToString() + property.propertyPath;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        Parameters parameters = GetParameters(property);

        float height = EditorGUIUtility.singleLineHeight;

        LoopOverProperty(parameters, property, () =>
        {
            height += EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.standardVerticalSpacing;
        });

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        _isCalledOnGui = true;

        //if (property.hasMultipleDifferentValues == false)
            DrawManagedReference(property, label.text, position);
    }

    private void DrawManagedReference(SerializedProperty property, string label, Rect rect)
    {
        Parameters parameters = GetParameters(property);

        rect.height = EditorGUIUtility.singleLineHeight;
        Rect rectLabel = rect;

        DrawLabel(parameters, label, rectLabel);

        int indexInPopup = DrawPopupAndGetIndex(parameters, rect);
        if (indexInPopup != parameters.IndexChoicedType)
        {
            parameters.IndexChoicedType = indexInPopup;
            ChangeManagedReferenceValue(parameters);
        }

        DrawProperty(parameters, property, rect);
    }

    private Parameters GetParameters(SerializedProperty property)
    {
        var key = GetKeyForParameter(property);
        if (_parameters.TryGetValue(key, out Parameters parameters) == false)
        {
            parameters = new Parameters(property, GetOrCreateDataReference());
            _parameters.Add(key, parameters);
        }

        return parameters;
    }

    private void DrawLabel(Parameters parameters, string label, Rect rectLabel)
    {
        if (parameters.ManagedReferenceValue != null)
            parameters.Foldout = EditorGUI.Foldout(rectLabel, parameters.Foldout, label);
        else
            EditorGUI.LabelField(rectLabel, label);
    }

    private void ChangeManagedReferenceValue(Parameters parameters)
    {
        SerializedProperty property = parameters.Property;
        bool changeReference = true;
        object newManagedReference = null;

        if (parameters.IndexChoicedType != parameters.Data.IndexNullVariable)
        {
            newManagedReference = CreateManagedReferenceValueAndCopyFields(parameters, ref changeReference);
        }

        if (changeReference)
        {
            parameters.ManagedReferenceValue = newManagedReference;
            parameters.Foldout = true;
            property.managedReferenceValue = newManagedReference;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private int DrawPopupAndGetIndex(Parameters parameters, Rect rect)
    {
        Rect rectPopup = rect;
        float offset = EditorGUIUtility.labelWidth + 2f - EditorGUI.indentLevel * 15f;
        rectPopup.x += offset;
        rectPopup.width -= offset;

        int indexInPopup = EditorGUI.Popup(rectPopup, parameters.IndexChoicedType, parameters.Data.TypesNames);
        return indexInPopup;
    }

    private void DrawProperty(Parameters parameters, SerializedProperty iterator, Rect rect)
    {
        ++EditorGUI.indentLevel;
        {
            Rect rectField = rect;
            rectField.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            LoopOverProperty(parameters, iterator, () =>
            {
                EditorGUI.PropertyField(rectField, iterator, true);
                rectField.y += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
            });
        }
        --EditorGUI.indentLevel;
    }

    private object CreateManagedReferenceValueAndCopyFields(Parameters parameters, ref bool changeReference)
    {
        object newManagedReference;
        parameters.Property.serializedObject.Update();

        object oldManagedReference = parameters.ManagedReferenceValue;
        int indexChoiceType = parameters.IndexChoicedType;

        //I don't want to add the first element to Types as null, so I make such a crutch.
        if (parameters.Data.Attribute.Nullable)
            indexChoiceType -= 1;

        newManagedReference = Activator.CreateInstance(parameters.Data.Types[indexChoiceType]);

        var canChangeSerializeReference = newManagedReference as ISerializeReferenceChangeValidate;
        if (canChangeSerializeReference == null || canChangeSerializeReference.Validate(out string textError))
        {
            ReflectionUtilities.CopyFieldsFromSourceToDestination(oldManagedReference, newManagedReference);
        }
        else
        {
            EditorUtility.DisplayDialog($"Cannot be changed to {newManagedReference.GetType()}", textError, "Ok");
            changeReference = false;
        }

        return newManagedReference;
    }

    private void LoopOverProperty(Parameters parameters, SerializedProperty iterator, Action action)
    {
        int CalculateCountDigits(SerializedProperty property) => property.propertyPath.Count((character) => character == '.');

        if (parameters.Foldout && iterator.hasVisibleChildren && iterator.Next(true))
        {
            int countDigits = iterator.propertyPath.Count((character) => character == '.');
            do
            {
                if (parameters.Data.IgnoreNameProperties.Contains(iterator.name))
                {
                    LoopOverProperty(parameters, iterator, action);
                    if (CalculateCountDigits(iterator) != countDigits)
                        break;
                }

                action();
            }
            while (iterator.NextVisible(false) && CalculateCountDigits(iterator) == countDigits);
        }
    }

    private DataReference GetOrCreateDataReference()
    {
        if (_dataReferences.TryGetValue(fieldInfo, out DataReference data) == false)
        {
            data = new DataReference(fieldInfo.FieldType, attribute as ChoiceReferenceAttribute);
            _dataReferences.Add(fieldInfo, data);
        }
        return data;
    }
}