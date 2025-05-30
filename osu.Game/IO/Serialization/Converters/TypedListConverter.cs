// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu.Game.IO.Serialization.Converters
{
    /// <summary>
    /// A type of <see cref="JsonConverter"/> that serializes an <see cref="IReadOnlyList{T}"/> alongside
    /// a lookup table for the types contained. The lookup table is used in deserialization to
    /// reconstruct the objects with their original types.
    /// </summary>
    public class TypedListConverter : JsonConverter
    {
        private readonly bool requiresTypeVersion;

        /// <summary>
        /// Constructs a new <see cref="TypedListConverter"/>.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public TypedListConverter()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="TypedListConverter"/>.
        /// </summary>
        /// <param name="requiresTypeVersion">Whether the version of the type should be serialized.</param>
        // ReSharper disable once UnusedMember.Global (Used in Beatmap)
        public TypedListConverter(bool requiresTypeVersion)
        {
            this.requiresTypeVersion = requiresTypeVersion;
        }

        public override bool CanConvert(Type objectType) => tryGetListItemType(objectType, out _);

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (!tryGetListItemType(objectType, out var itemType))
                throw new JsonException($"May not use {nameof(TypedListConverter)} on a type that does not implement {typeof(IReadOnlyList<>).Name}");

            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;

            var obj = JObject.Load(reader);

            if (obj["$lookup_table"] == null)
                return list;

            var lookupTable = serializer.Deserialize<List<string>>(obj["$lookup_table"]!.CreateReader());
            if (lookupTable == null)
                return list;

            if (obj["$items"] == null)
                return list;

            foreach (var tok in obj["$items"]!)
            {
                var itemReader = tok.CreateReader();

                int? typeIndex = tok["$type"]?.Value<int>();

                if (typeIndex == null)
                    throw new JsonException("Expected $type token.");

                if (typeIndex < 0 || typeIndex >= lookupTable.Count)
                    throw new JsonException($"$type index {typeIndex} is out of range.");

                // Prevent instantiation of types that do not inherit the type targetted by this converter
                Type type = Type.GetType(lookupTable[typeIndex.Value])!;
                if (!type.IsAssignableTo(itemType))
                    continue;

                object instance = Activator.CreateInstance(type)!;
                serializer.Populate(itemReader, instance);

                list.Add(instance);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var list = (IEnumerable)value;

            var lookupTable = new List<string>();
            var objects = new List<JObject>();

            foreach (object item in list)
            {
                var type = item.GetType();
                var assemblyName = type.Assembly.GetName();

                string typeString = $"{type.FullName}, {assemblyName.Name}";
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

        private static bool tryGetListItemType(Type objectType, [MaybeNullWhen(false)] out Type itemType)
        {
            if (objectType.IsInterface && isListType(objectType))
            {
                itemType = objectType.GenericTypeArguments[0];
                return true;
            }

            var listType = objectType.GetInterfaces().FirstOrDefault(isListType);

            itemType = listType?.GenericTypeArguments.FirstOrDefault();
            return itemType != null;

            bool isListType(Type type)
            {
                if (!type.IsGenericType)
                    return false;

                var genericTypeDefinition = type.GetGenericTypeDefinition();
                return genericTypeDefinition == typeof(IReadOnlyList<>) || genericTypeDefinition == typeof(IList<>);
            }
        }
    }
}
