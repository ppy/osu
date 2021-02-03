// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using osu.Framework.Bindables;

namespace osu.Game.Online.API
{
    public class ModSettingsDictionaryFormatter : IMessagePackFormatter<Dictionary<string, object>>
    {
        public int Serialize(ref byte[] bytes, int offset, Dictionary<string, object> value, IFormatterResolver formatterResolver)
        {
            int startOffset = offset;

            var primitiveFormatter = PrimitiveObjectFormatter.Instance;

            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Count);

            foreach (var kvp in value)
            {
                offset += MessagePackBinary.WriteString(ref bytes, offset, kvp.Key);

                switch (kvp.Value)
                {
                    case Bindable<double> d:
                        offset += primitiveFormatter.Serialize(ref bytes, offset, d.Value, formatterResolver);
                        break;

                    case Bindable<float> f:
                        offset += primitiveFormatter.Serialize(ref bytes, offset, f.Value, formatterResolver);
                        break;

                    case Bindable<bool> b:
                        offset += primitiveFormatter.Serialize(ref bytes, offset, b.Value, formatterResolver);
                        break;

                    default:
                        throw new ArgumentException("A setting was of a type not supported by the messagepack serialiser", nameof(bytes));
                }
            }

            return offset - startOffset;
        }

        public Dictionary<string, object> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            int startOffset = offset;

            var output = new Dictionary<string, object>();

            int itemCount = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            for (int i = 0; i < itemCount; i++)
            {
                var key = MessagePackBinary.ReadString(bytes, offset, out readSize);
                offset += readSize;

                output[key] = PrimitiveObjectFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out readSize);
                offset += readSize;
            }

            readSize = offset - startOffset;
            return output;
        }
    }
}
