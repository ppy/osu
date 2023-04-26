// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu.Game.Online
{
    /// <summary>
    /// A type of <see cref="JsonConverter"/> that serializes a subset of types used in multiplayer/spectator communication that
    /// derive from a known base type. This is a safe alternative to using <see cref="TypeNameHandling.Auto"/> or <see cref="TypeNameHandling.All"/>,
    /// which are known to have security issues.
    /// </summary>
    public class SignalRDerivedTypeWorkaroundJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            SignalRWorkaroundTypes.BASE_TYPE_MAPPING.Any(t =>
                objectType == t.baseType ||
                objectType == t.derivedType);

        public override object? ReadJson(JsonReader reader, Type objectType, object? o, JsonSerializer jsonSerializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject obj = JObject.Load(reader);

            string type = (string)obj[@"$dtype"]!;

            var resolvedType = SignalRWorkaroundTypes.BASE_TYPE_MAPPING.Select(t => t.derivedType).Single(t => t.Name == type);

            object? instance = Activator.CreateInstance(resolvedType);

            if (instance != null)
                jsonSerializer.Populate(obj["$value"]!.CreateReader(), instance);

            return instance;
        }

        public override void WriteJson(JsonWriter writer, object? o, JsonSerializer serializer)
        {
            if (o == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName(@"$dtype");
            serializer.Serialize(writer, o.GetType().Name);

            writer.WritePropertyName(@"$value");
            writer.WriteRawValue(JsonConvert.SerializeObject(o));

            writer.WriteEndObject();
        }
    }
}
