using System;
using UnityEngine;

namespace Utility_Scripts
{
    public static class JsonArrayUtility
    {
        public static T[] FromJson<T>(string jsonKey)
        {
            SerializableArray<T> serializableArray = JsonUtility.FromJson<SerializableArray<T>>(jsonKey);
            return serializableArray.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            SerializableArray<T> serializableArray = new SerializableArray<T>
            {
                Items = array
            };

            return JsonUtility.ToJson(serializableArray);
        }

        [Serializable]
        private class SerializableArray<T>
        {
            public T[] Items;
        }
    }
}
