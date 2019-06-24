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
    public class Vector2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Vector2);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return new Vector2((float)obj["x"], (float)obj["y"]);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vector = (Vector2)value;

            writer.WriteStartObject();

            writer.WritePropertyName("x");
            writer.WriteValue(vector.X);
            writer.WritePropertyName("y");
            writer.WriteValue(vector.Y);

            writer.WriteEndObject();
        }
    }
}
