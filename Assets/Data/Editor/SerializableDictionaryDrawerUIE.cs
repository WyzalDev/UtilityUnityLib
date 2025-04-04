using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace WyzalUtilities.Data
{
    public class SerializableDictionaryConverter<TKey, TValue> : UxmlAttributeConverter<SerializableDictionary<TKey, TValue>>
{
    static string ValueToString(object inValue) => System.Convert.ToString(inValue, CultureInfo.InvariantCulture);

    public override string ToString(SerializableDictionary<TKey, TValue> inSource)
    {
        //var dataBuilder = new StringBuilder();
        //
        // foreach(var keyValuePair in inSource)
        // {
        //     dataBuilder.Append($"{ValueToString(keyValuePair.Key)}|{ValueToString(keyValuePair.Value)},");
        // }

        return JsonUtility.ToJson(inSource); //dataBuilder.ToString();
    }

    public override SerializableDictionary<TKey, TValue> FromString(string inSource)
    {
        // var outputDictionary = new SerializableDictionary<TKey, TValue>();
        //
        // var keyValuePairs = inSource.Split(',');
        // foreach(var keyValuePair in keyValuePairs)
        // {
        //     var fields = keyValuePair.Split("|");
        //     var key = (TKey)System.Convert.ChangeType(fields[0], typeof(TKey));
        //     var value = (TValue)System.Convert.ChangeType(fields[1], typeof(TValue));
        //
        //     outputDictionary.EditorOnly_Add(key, value);
        // }
        var outputDictionary = JsonUtility.FromJson<SerializableDictionary<TKey, TValue>>(inSource);
        outputDictionary.SynchroniseToSerializedData();
        return outputDictionary;
    }
}

[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SerializableDictionaryDrawerUIE : PropertyDrawer
{
    private SerializedProperty _linkedProperty;
    private SerializedProperty _linkedKeys;
    private SerializedProperty _linkedValues;

    public override VisualElement CreatePropertyGUI(SerializedProperty inProperty)
    {
        _linkedProperty = inProperty;
        _linkedKeys = inProperty.FindPropertyRelative("SerializedKeys");
        _linkedValues = inProperty.FindPropertyRelative("SerializedValues");

        var containerUI = new Foldout()
        {
            text = inProperty.displayName,
            viewDataKey = $"{inProperty.serializedObject.targetObject.GetInstanceID()}.{inProperty.name}"
        };

        var contentsUI = new ListView()
        {
            showAddRemoveFooter = true,
            showBorder = true,
            showAlternatingRowBackgrounds = AlternatingRowBackground.All,
            showFoldoutHeader = false,
            showBoundCollectionSize = false,
            reorderable = false,
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            headerTitle = inProperty.displayName,
            bindingPath = _linkedKeys.propertyPath,
            bindItem = BindListItem,
            overridingAddButtonBehavior = OnAddButton,
            onRemove = OnRemove
        };

        containerUI.Add(contentsUI);

        var removeDuplicatesButton = new Button() { text = "Remove Duplicates" };
        removeDuplicatesButton.clicked += OnRemoveDuplicates;

        containerUI.Add(removeDuplicatesButton);

        return containerUI;
    }

    bool AreDuplicatesOfKeyPresent(SerializedProperty inKeyProperty, int inKeyIndex)
    {
        for (var i = 0; i < _linkedKeys.arraySize; i++)
        {
            // skip if this is our key
            if (i == inKeyIndex)
                continue;

            var otherKey = _linkedKeys.GetArrayElementAtIndex(i);

            if (otherKey.boxedValue.Equals(inKeyProperty.boxedValue))
                return true;
        }

        return false;
    }

    void BindListItem(VisualElement inItemUI, int inItemIndex)
    {
        inItemUI.Clear();
        inItemUI.Unbind();

        var keyProperty = _linkedKeys.GetArrayElementAtIndex(inItemIndex);
        var valueProperty = _linkedValues.GetArrayElementAtIndex(inItemIndex);

        var keyUI = new PropertyField(keyProperty) { label = "Key" };
        var valueUI = new PropertyField(valueProperty) { label = "Value" };
        
        inItemUI.Add(keyUI);
        inItemUI.Add(valueUI);

        var warningUI = new Label("<b>Error: Duplicate Key Detected</b>");
        inItemUI.Add(warningUI);

        warningUI.visible = AreDuplicatesOfKeyPresent(keyProperty, inItemIndex);

        inItemUI.TrackPropertyValue(keyProperty, (SerializedProperty inKeyProp) =>
        {
            warningUI.visible = AreDuplicatesOfKeyPresent(keyProperty, inItemIndex);
        });

        inItemUI.Bind(_linkedProperty.serializedObject);
    }
    
    void OnAddButton(BaseListView inListView, Button inButton)
    {
        _linkedKeys.InsertArrayElementAtIndex(_linkedKeys.arraySize);
        _linkedValues.InsertArrayElementAtIndex(_linkedValues.arraySize);
        _linkedProperty.serializedObject.ApplyModifiedProperties();
    }

    void OnRemoveDuplicates()
    {
        List<int> indicesToRemove = new();

        // search for any duplicates
        for (var i = 0; i < _linkedKeys.arraySize; i++)
        {
            var firstKey = _linkedKeys.GetArrayElementAtIndex(i);
            
            for (var y = i + 1; y < _linkedKeys.arraySize; y++) 
            { 
                var otherKey = _linkedKeys.GetArrayElementAtIndex(y);

                if (firstKey.boxedValue.Equals(otherKey.boxedValue) && 
                    !indicesToRemove.Contains(y))
                {
                    indicesToRemove.Add(y);
                }
            }
        }

        // Remove the duplicates
        for (var i = indicesToRemove.Count - 1; i >= 0; i--)
        { 
            var indexToRemove = indicesToRemove[i];
            _linkedKeys.DeleteArrayElementAtIndex(indexToRemove);
            _linkedValues.DeleteArrayElementAtIndex(indexToRemove);
        }
        _linkedProperty.serializedObject.ApplyModifiedProperties();
    }

    void OnRemove(BaseListView inListView)
    {
        if ((_linkedKeys.arraySize <= 0) ||
            (inListView.selectedIndex < 0) ||
            (inListView.selectedIndex >= _linkedKeys.arraySize)) return;
        
        var indexToRemove = inListView.selectedIndex;

        _linkedKeys.DeleteArrayElementAtIndex(indexToRemove);
        _linkedValues.DeleteArrayElementAtIndex(indexToRemove);
        _linkedProperty.serializedObject.ApplyModifiedProperties();
    }
}
}