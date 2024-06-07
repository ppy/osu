// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Buffers;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using MessagePack.Formatters;
using osu.Game.Configuration;

namespace osu.Game.Online.API
{
    public class ModSettingsDictionaryFormatter : IMessagePackFormatter<Dictionary<string, object>>
    {
        public void Serialize(ref MessagePackWriter writer, Dictionary<string, object> value, MessagePackSerializerOptions options)
        {
            var primitiveFormatter = PrimitiveObjectFormatter.Instance;

            writer.WriteArrayHeader(value.Count);

            foreach (var kvp in value)
            {
                var stringBytes = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(kvp.Key));
                writer.WriteString(in stringBytes);

                primitiveFormatter.Serialize(ref writer, kvp.Value.GetUnderlyingSettingValue(), options);
            }
        }

        public Dictionary<string, object> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var output = new Dictionary<string, object>();

            int itemCount = reader.ReadArrayHeader();

            for (int i = 0; i < itemCount; i++)
            {
                output[reader.ReadString()!] =
                    PrimitiveObjectFormatter.Instance.Deserialize(ref reader, options)!;
            }

            return output;
        }
    }
}
