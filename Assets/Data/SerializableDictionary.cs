using System;
using System.Collections.Generic;
using UnityEngine;

namespace WyzalUtilities.Data
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public List<TKey> SerializedKeys = new List<TKey>();
        public List<TValue> SerializedValues = new List<TValue>();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            SynchroniseToSerializedData();
        }

#if UNITY_EDITOR
        public void EditorOnly_Add(TKey inKey, TValue inValue)
        {
            SerializedKeys.Add(inKey);
            SerializedValues.Add(inValue);
        }
#endif

        public void SynchroniseToSerializedData()
        {
            this.Clear();

            // if we have valid data then build the dictionary
            if ((SerializedKeys != null) && (SerializedValues != null))
            {
                var correctCount = Mathf.Min(SerializedKeys.Count, SerializedValues.Count);
                for (var i = 0; i < correctCount; ++i)
                {
                    this[SerializedKeys[i]] = SerializedValues[i];
                }
            }
            else
            {
                SerializedKeys = new();
                SerializedValues = new();
            }

            // if the lists are out of sync then rebuild
            if (SerializedKeys.Count == SerializedValues.Count) return;
            
            SerializedKeys = new(Keys);
            SerializedValues = new(Values);
        }
    }
}