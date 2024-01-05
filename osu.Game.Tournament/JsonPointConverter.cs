// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using Newtonsoft.Json;

namespace osu.Game.Tournament
{
    /// <summary>
    /// We made a change from using SixLabors.ImageSharp.Point to System.Drawing.Point at some stage.
    /// This handles converting to a standardised format on json serialize/deserialize operations.
    /// </summary>
    internal class JsonPointConverter : JsonConverter<Point>
    {
        public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer)
        {
            // use the format of LaborSharp's Point since it is nicer.
            serializer.Serialize(writer, new { value.X, value.Y });
        }

        public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                // if there's no object present then this is using string representation (System.Drawing.Point serializes to "x,y")
                string? str = (string?)reader.Value;

                Debug.Assert(str != null);

                // Null check suppression is required due to .NET standard expecting a non-null context.
                // Seems to work fine at a runtime level (and the parameter is nullable in .NET 6+).
                return new PointConverter().ConvertFromString(null!, CultureInfo.InvariantCulture, str) as Point? ?? new Point();
            }

            var point = new Point();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject) break;

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string? name = reader.Value?.ToString();
                    int? val = reader.ReadAsInt32();

                    if (name == null)
                        continue;

                    if (val == null)
                        continue;

                    switch (name)
                    {
                        case "X":
                            point.X = val.Value;
                            break;

                        case "Y":
                            point.Y = val.Value;
                            break;
                    }
                }
            }

            return point;
        }
    }
}
