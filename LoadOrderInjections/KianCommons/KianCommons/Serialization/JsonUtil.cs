using KianCommons.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine;
using ColossalFramework;

namespace KianCommons.Serialization {
    internal static class JsonUtil {
        internal static JsonSerializerSettings Settings => new JsonSerializerSettings {
            Formatting = Formatting.Indented,
            Converters = new JsonConverter[] {
                new VersionConverter(),
                new StringEnumConverter(),
                new KeyValuePairConverter(),
                new Vector3Converter(),
                new PrefabConverter<PropInfo>(),
                new PrefabConverter<TreeInfo>(),
            },
        };

        internal class Vector3Converter : JsonConverter {
            public override bool CanConvert(Type objectType) {
                return
                    objectType == typeof(Vector3) ||
                    objectType == typeof(Vector3Serializable);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
                string data = reader.Value as string;
                if (data.IsNullOrWhiteSpace())
                    return null;
                data = data.Remove("x=", "y=", "z=");
                var datas = data.Split(" ");
                float x = float.Parse(datas[0]);
                float y = float.Parse(datas[1]);
                float z = float.Parse(datas[2]);
                Vector3 ret = new Vector3(x, y, z);

                if (objectType == typeof(Vector3))
                    return ret;
                if (objectType == typeof(Vector3Serializable))
                    return (Vector3Serializable)ret;

                return ret;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                if (value is Vector3 vector3)
                    writer.WriteValue($"x={vector3.x} y={vector3.y} z={vector3.z}");
                if (value is Vector3Serializable vector3Serializable)
                    writer.WriteValue($"x={vector3Serializable.x} y={vector3Serializable.y} z={vector3Serializable.z}");
            }
        }

        internal class PrefabConverter<T> : JsonConverter
            where T : PrefabInfo {
            public override bool CanConvert(Type objectType) =>
                objectType == typeof(T);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
                string name = reader.Value as string;
                if (name.IsNullOrWhiteSpace())
                    return null;
                return PrefabCollection<T>.FindLoaded(name);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                string name = (value as T)?.name;
                writer.WriteValue(name);
            }
        }

    }
}
