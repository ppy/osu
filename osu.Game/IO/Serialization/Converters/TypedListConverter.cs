// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu.Game.IO.Serialization.Converters
{
    /// <summary>
    /// A type of <see cref="JsonConverter"/> that serializes a <see cref="List{T}"/> alongside
    /// a lookup table for the types contained. The lookup table is used in deserialization to
    /// reconstruct the objects with their original types.
    /// </summary>
    /// <typeparam name="T">The type of objects contained in the <see cref="List{T}"/> this attribute is attached to.</typeparam>
    public class TypedListConverter<T> : JsonConverter
    {
        private readonly bool requiresTypeVersion;

        /// <summary>
        /// Constructs a new <see cref="TypedListConverter{T}"/>.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public TypedListConverter()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="TypedListConverter{T}"/>.
        /// </summary>
        /// <param name="requiresTypeVersion">Whether the version of the type should be serialized.</param>
        // ReSharper disable once UnusedMember.Global (Used in Beatmap)
        public TypedListConverter(bool requiresTypeVersion)
        {
            this.requiresTypeVersion = requiresTypeVersion;
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(List<T>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var list = new List<T>();

            var obj = JObject.Load(reader);
            var lookupTable = serializer.Deserialize<List<string>>(obj["$lookup_table"].CreateReader());

            foreach (var tok in obj["$items"])
            {
                var itemReader = tok.CreateReader();

                var typeName = lookupTable[(int)tok["$type"]];
                var instance = (T)Activator.CreateInstance(Type.GetType(typeName));
                serializer.Populate(itemReader, instance);

                list.Add(instance);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (IList)value;

            var lookupTable = new List<string>();
            var objects = new List<JObject>();

            foreach (var item in list)
            {
                var type = item.GetType();
                var assemblyName = type.Assembly.GetName();

                var typeString = $"{type.FullName}, {assemblyName.Name}";
                if (requiresTypeVersion)
                    typeString += $", {assemblyName.Version}";

                int typeId = lookupTable.IndexOf(typeString);

                if (typeId == -1)
                {
                    lookupTable.Add(typeString);
                    typeId = lookupTable.Count - 1;
                }

                var itemObject = JObject.FromObject(item, serializer);
                itemObject.AddFirst(new JProperty("$type", typeId));
                objects.Add(itemObject);
            }

            writer.WriteStartObject();

            writer.WritePropertyName("$lookup_table");
            serializer.Serialize(writer, lookupTable);

            writer.WritePropertyName("$items");
            serializer.Serialize(writer, objects);

            writer.WriteEndObject();
        }
    }
}
