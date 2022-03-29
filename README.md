# ChoiceReferenceAttribute
This package allows you to use the [ChoiceReference] attribute to make a popup interaction to select a subclass from which the base class that you specified as the field type inherits. Lists and arrays are also supported, as is the fact that this attribute will be used again in subclasses.

[Документация на русском](https://github.com/Paulsams/ChoiceReferenceAttribute/blob/master/Documentation~/RU.md)

## Add to project:

To add this package to the project, follow these steps:
1) Open PackageManager;
2) Select "Add package from get URL";
3) Insert links to packages that are dependencies of this package:
    + `https://github.com/Paulsams/MiscUtilities.git`
4) Insert this link `https://github.com/Paulsams/ChoiceReferenceAttribute`

## Opportunities:
1) Works for lists/array and for any nesting of this attribute.
IMPORTANT: in order for the lists/arrays to work, it is necessary to add [System.Serializable] to the base class. Don't ask me why - that's life:

```cs
[SerializeReference, ChoiceReference] private BaseClass _singleChoiceReference;
[SerializeReference, ChoiceReference] private List<BaseClass> _listChoiceReferences;
```

![image](https://github.com/Paulsams/ChoiceReferenceAttribute/blob/master/Documentation~/Single%20and%20Lists.gif)

2) You can specify the nullable flag in the parameters attribute, which allows you to remove the "null" state from the selection for the object. If it is "false" (and this is the default value), then you will still need to select the type for the first time, otherwise there would be recursion if there is a field with the same attribute in the object.

```cs
[SerializeReference, ChoiceReference(true)] private BaseClass _singleChoiceReferenceNullable;
[SerializeReference, ChoiceReference] private BaseClass _singleChoiceReferenceNotNullable;
```

![image](https://github.com/Paulsams/ChoiceReferenceAttribute/blob/master/Documentation~/Nullable.gif)

3) You can also specify an array (params) of ignored fields in the parameters, and this is useful for those cases when you need to divide data into different classes inside an object and so that you don't have to open them every time through foldout, then you can use this ignoring.

```cs
[SerializeReference, ChoiceReference("_data")] private BaseClass _singleChoiceReferenceWithIgnoreData;
[SerializeReference, ChoiceReference] private BaseClass _singleChoiceReferenceNotIgnoreData;
```

![image](https://github.com/Paulsams/ChoiceReferenceAttribute/blob/master/Documentation~/IgnoreNames.gif)

## Samples:
To download the examples for this package:
1) Select this package in PackageManager;
2) Expand the "Samples" tab on the right;
3) And click the "Import" button on the example you are interested in.
