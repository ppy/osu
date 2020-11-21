// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osuTK;

namespace osu.Game.IO.Serialization.Converters
{
    /// <summary>
    /// A type of <see cref="JsonConverter"/> that serializes only the X and Y coordinates of a <see cref="Vector2"/>.
    /// </summary>
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return new Vector2((float)obj["x"], (float)obj["y"]);
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("x");
            writer.WriteValue(value.X);
            writer.WritePropertyName("y");
            writer.WriteValue(value.Y);

            writer.WriteEndObject();
        }
    }
}
