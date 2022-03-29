# ChoiceReferenceAttribute
Данный пакет позволяет вам использовать атрибут [ChoiceReference] для создания popup взаимодействия для выбора подкласса, от которого наследуется базовый класс, указанный вами в качестве типа поля. Списки и массивы также поддерживаются, как и тот факт, что этот атрибут будет снова использоваться в подклассах.

## Добавление в проект:
Чтобы добавить данный пакет в проект, нужно выполнить следующие шаги:
1) Откройте PackageManager;
2) Выберите "Add package from get URL";
3) Вставьте ссылки на пакеты, которые являются зависимостями данного пакета:
    + `https://github.com/Paulsams/MiscUtilities.git`
3) Вставьте ссылку на данный пакет: `https://github.com/Paulsams/ChoiceReferenceAttribute.git`

## Возможности:
1) Работает для листов/массивов и для любой вложенности данного атрибута.
ВАЖНО: чтобы для листов/массивов работало, то нужно обязательно базовому классу дописать [System.Serializable]. Не спрашивайте меня почему - такова жизнь:

![image](https://github.com/Paulsams/ChoiceReferenceAttribute/blob/master/Documentation~/Single%20and%20Lists.gif)

2) Можно в параметрах атрибута указать флаг nullable, который позволяет из выбора убрать задание объекту состояние "null". Если он будет "false" (а это есть значение по умолчанию), то Вам всё равно надо будет в первый раз выбрать тип, а иначе была бы рекурсия, если в объекте будет поле с этим же атрибутом.

![image](https://github.com/Paulsams/ChoiceReferenceAttribute/blob/master/Documentation~/Nullable.gif)

3) Также можно в параметрах указать массив (params) игнорируемых полей и это полезно для тех случаев, когда вам нужно внутри объекта разделить данные по разным классам и чтобы не приходилось их каждый раз расскрывать через foldout, то можете заюзать данное игнорирование.

![image](https://github.com/Paulsams/ChoiceReferenceAttribute/blob/master/Documentation~/IgnoreNames.gif)

4) ICanChangeSerializeReference - интерфейс, который вы можете реализовать у наследника для того, чтобы сделать какую-то свою проверку, что валидна ли в данный момент смена типа.

## Конструкторы:
	ChoiceReferenceAttribute(bool nullable, params string[] ignoreNameProperties)
	ChoiceReferenceAttribute(params string[] ignoreNameProperties) - где nullable = false

## Примеры:
Чтобы скачать примеры к данному пакету:
1) Выберите данный пакет в PackageManager;
2) Раскройте справа вкладку "Samples";
3) И нажмите кнопку "Import" на интересующем вас примере.