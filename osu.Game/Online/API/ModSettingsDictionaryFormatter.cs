// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MessagePack;
using MessagePack.Formatters;
using osu.Framework.Bindables;

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

                switch (kvp.Value)
                {
                    case Bindable<double> d:
                        primitiveFormatter.Serialize(ref writer, d.Value, options);
                        break;

                    case Bindable<int> i:
                        primitiveFormatter.Serialize(ref writer, i.Value, options);
                        break;

                    case Bindable<float> f:
                        primitiveFormatter.Serialize(ref writer, f.Value, options);
                        break;

                    case Bindable<bool> b:
                        primitiveFormatter.Serialize(ref writer, b.Value, options);
                        break;

                    case IBindable u:
                        // A mod with unknown (e.g. enum) generic type.
                        var valueMethod = u.GetType().GetProperty(nameof(IBindable<int>.Value));
                        Debug.Assert(valueMethod != null);
                        primitiveFormatter.Serialize(ref writer, valueMethod.GetValue(u), options);
                        break;

                    default:
                        // fall back for non-bindable cases.
                        primitiveFormatter.Serialize(ref writer, kvp.Value, options);
                        break;
                }
            }
        }

        public Dictionary<string, object> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var output = new Dictionary<string, object>();

            int itemCount = reader.ReadArrayHeader();

            for (int i = 0; i < itemCount; i++)
            {
                output[reader.ReadString()] =
                    PrimitiveObjectFormatter.Instance.Deserialize(ref reader, options);
            }

            return output;
        }
    }
}
