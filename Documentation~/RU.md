# ChoiceReferenceAttribute
Данный пакет позволяет вам использовать атрибут [ChoiceReference] для создания popup взаимодействия для выбора подкласса, от которого наследуется базовый класс, указанный вами в качестве типа поля. Списки и массивы также поддерживаются, как и тот факт, что этот атрибут будет снова использоваться в подклассах.

## Добавление в проект
Чтобы добавить данный пакет в проект, нужно выполнить следующие шаги:
1) Откройте PackageManager;
2) Выберите "Add package from get URL";
3) Вставьте ссылки на пакеты, которые являются зависимостями данного пакета:
    + `https://github.com/Paulsams/MiscUtilities.git`
3) Вставьте ссылку на данный пакет: `https://github.com/Paulsams/ChoiceReferenceAttribute.git`

## Связанные пакеты
1) RepairerSerializeReferences (пока что только с 2021.2) - окно редактора, которое позволяет починить ссылки, которые полетели в связи со сменой названия класса, пространстрва имён или сборки: https://github.com/Paulsams/RepairerSerializeReferences

## Зависимости
- Использует:
    + MicsUtilities: https://github.com/Paulsams/MiscUtilities.git

## Возможности
1) Работает для листов/массивов и для любой вложенности данного атрибута.

ВАЖНО: чтобы атрибут работал с листами или массивами, то нужно обязательно базовому классу дописать `[System.Serializable]`. Не спрашивайте меня почему - такова жизнь:

```cs
[SerializeReference, ChoiceReference] private BaseClass _singleChoiceReference;
[SerializeReference, ChoiceReference] private List<BaseClass> _listChoiceReferences;
```

![image](https://github.com/Paulsams/ChoiceReferenceAttribute/blob/master/Documentation~/Single%20and%20Lists.gif)

2) Можно в параметрах атрибута указать флаг nullable, который позволяет из выбора убрать задание объекту состояние "null". Если он будет "false" (а это есть значение по умолчанию), то Вам всё равно надо будет в первый раз выбрать тип, а иначе была бы рекурсия, если в объекте будет поле с этим же атрибутом.

```cs
[SerializeReference, ChoiceReference(true)] private BaseClass _singleChoiceReferenceNullable;
[SerializeReference, ChoiceReference] private BaseClass _singleChoiceReferenceNotNullable;
```

![image](https://github.com/Paulsams/ChoiceReferenceAttribute/blob/master/Documentation~/Nullable.gif)

3) ISerializeReferenceChangeValidate - интерфейс, который вы можете реализовать у наследника для того, чтобы сделать какую-то свою проверку, что валидна ли в данный момент смена типа.

## Конструкторы
```cs
ChoiceReferenceAttribute(bool nullable)
```

## Примеры
Чтобы скачать примеры к данному пакету:
1) Выберите данный пакет в PackageManager;
2) Раскройте справа вкладку "Samples";
3) И нажмите кнопку "Import" на интересующем вас примере.
