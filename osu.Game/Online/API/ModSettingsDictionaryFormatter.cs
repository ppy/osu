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

            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Count);

            foreach (var kvp in value)
            {
                offset += MessagePackBinary.WriteString(ref bytes, offset, kvp.Key);

                switch (kvp.Value)
                {
                    case Bindable<double> d:
                        offset += MessagePackBinary.WriteDouble(ref bytes, offset, d.Value);
                        break;

                    case Bindable<float> f:
                        offset += MessagePackBinary.WriteSingle(ref bytes, offset, f.Value);
                        break;

                    case Bindable<bool> b:
                        offset += MessagePackBinary.WriteBoolean(ref bytes, offset, b.Value);
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

                switch (MessagePackBinary.GetMessagePackType(bytes, offset))
                {
                    case MessagePackType.Float:
                    {
                        // could be either float or double...
                        // see https://github.com/msgpack/msgpack/blob/master/spec.md#serialization-type-to-format-conversion
                        switch (MessagePackCode.ToFormatName(bytes[offset]))
                        {
                            case "float 32":
                                output[key] = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                                offset += readSize;
                                break;

                            case "float 64":
                                output[key] = MessagePackBinary.ReadDouble(bytes, offset, out readSize);
                                offset += readSize;
                                break;

                            default:
                                throw new ArgumentException("A setting was of a type not supported by the messagepack deserialiser", nameof(bytes));
                        }

                        break;
                    }

                    case MessagePackType.Boolean:
                        output[key] = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        offset += readSize;
                        break;

                    default:
                        throw new ArgumentException("A setting was of a type not supported by the messagepack deserialiser", nameof(bytes));
                }
            }

            readSize = offset - startOffset;
            return output;
        }
    }
}
