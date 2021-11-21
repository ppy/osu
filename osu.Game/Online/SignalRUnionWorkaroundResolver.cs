// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace osu.Game.Online
{
    /// <summary>
    /// Handles SignalR being unable to comprehend [Union] types correctly by redirecting to a known base (union) type.
    /// See https://github.com/dotnet/aspnetcore/issues/7298.
    /// </summary>
    public class SignalRUnionWorkaroundResolver : IFormatterResolver
    {
        public static readonly MessagePackSerializerOptions OPTIONS =
            MessagePackSerializerOptions.Standard.WithResolver(new SignalRUnionWorkaroundResolver());

        private static readonly IReadOnlyDictionary<Type, IMessagePackFormatter> formatter_map = createFormatterMap();

        private static IReadOnlyDictionary<Type, IMessagePackFormatter> createFormatterMap()
        {
            IEnumerable<(Type derivedType, Type baseType)> baseMap = SignalRWorkaroundTypes.BASE_TYPE_MAPPING;

            // This should not be required. The fallback should work. But something is weird with the way caching is done.
            // For future adventurers, I would not advise looking into this further. It's likely not worth the effort.
            baseMap = baseMap.Concat(baseMap.Select(t => (t.baseType, t.baseType)));

            return new Dictionary<Type, IMessagePackFormatter>(baseMap.Select(t =>
            {
                var formatter = (IMessagePackFormatter)Activator.CreateInstance(typeof(TypeRedirectingFormatter<,>).MakeGenericType(t.derivedType, t.baseType));
                return new KeyValuePair<Type, IMessagePackFormatter>(t.derivedType, formatter);
            }));
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (formatter_map.TryGetValue(typeof(T), out var formatter))
                return (IMessagePackFormatter<T>)formatter;

            return StandardResolver.Instance.GetFormatter<T>();
        }

        public class TypeRedirectingFormatter<TActual, TBase> : IMessagePackFormatter<TActual>
        {
            private readonly IMessagePackFormatter<TBase> baseFormatter;

            public TypeRedirectingFormatter()
            {
                baseFormatter = StandardResolver.Instance.GetFormatter<TBase>();
            }

            public void Serialize(ref MessagePackWriter writer, TActual value, MessagePackSerializerOptions options) =>
                baseFormatter.Serialize(ref writer, (TBase)(object)value, StandardResolver.Options);

            public TActual Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
                (TActual)(object)baseFormatter.Deserialize(ref reader, StandardResolver.Options);
        }
    }
}
