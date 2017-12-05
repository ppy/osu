// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK;

namespace osu.Game.IO.Serialization.Converters
{
    public class Vector2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Vector2);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return new Vector2((float)obj["X"], (float)obj["Y"]);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vector = (Vector2)value;

            writer.WriteStartObject();

            writer.WritePropertyName("X");
            writer.WriteValue(vector.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(vector.Y);

            writer.WriteEndObject();
        }
    }
}
