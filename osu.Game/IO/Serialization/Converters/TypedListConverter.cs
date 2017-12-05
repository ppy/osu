using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu.Game.IO.Serialization.Converters
{
    public class TypedListConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(List<T>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var list = new List<T>();

            var localSerializer = createLocalSerializer();

            var obj = JObject.Load(reader);

            var lookupTable = new List<string>();
            localSerializer.Populate(obj["LookupTable"].CreateReader(), lookupTable);

            foreach (var tok in obj["Items"])
            {
                var itemReader = tok.CreateReader();

                var typeName = lookupTable[(int)tok["Type"]];
                var instance = (T)Activator.CreateInstance(Type.GetType(typeName));
                localSerializer.Populate(itemReader, instance);

                list.Add(instance);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (List<T>)value;

            var localSerializer = createLocalSerializer();

            var lookupTable = new List<string>();
            var objects = new List<JObject>();
            foreach (var item in list)
            {
                var type = item.GetType().AssemblyQualifiedName;

                int typeId = lookupTable.IndexOf(type);
                if (typeId == -1)
                {
                    lookupTable.Add(type);
                    typeId = lookupTable.Count - 1;
                }

                var itemObject = JObject.FromObject(item, localSerializer);
                itemObject.AddFirst(new JProperty("Type", typeId));
                objects.Add(itemObject);
            }

            writer.WriteStartObject();

            writer.WritePropertyName("LookupTable");
            localSerializer.Serialize(writer, lookupTable);

            writer.WritePropertyName("Items");
            writer.WriteStartArray();
            foreach (var item in objects)
                item.WriteTo(writer);
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        private JsonSerializer createLocalSerializer()
        {
            var localSettings = JsonSerializableExtensions.CreateGlobalSettings();
            localSettings.Converters = localSettings.Converters.Where(c => !(c is TypedListConverter<T>)).ToArray();
            return JsonSerializer.Create(localSettings);
        }
    }
}
