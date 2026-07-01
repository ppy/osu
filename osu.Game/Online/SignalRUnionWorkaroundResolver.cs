// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using osu.Framework.Extensions.TypeExtensions;

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
        private static readonly ConcurrentDictionary<Type, IMessagePackFormatter> enum_formatter_map = [];

        private static IReadOnlyDictionary<Type, IMessagePackFormatter> createFormatterMap()
        {
            IEnumerable<(Type derivedType, Type baseType)> baseMap = SignalRWorkaroundTypes.BASE_TYPE_MAPPING;

            // This should not be required. The fallback should work. But something is weird with the way caching is done.
            // For future adventurers, I would not advise looking into this further. It's likely not worth the effort.
            baseMap = baseMap.Concat(baseMap.Select(t => (t.baseType, t.baseType)).Distinct());

            return new Dictionary<Type, IMessagePackFormatter>(baseMap.Select(t =>
            {
                var formatter = (IMessagePackFormatter)Activator.CreateInstance(typeof(TypeRedirectingFormatter<,>).MakeGenericType(t.derivedType, t.baseType))!;
                return new KeyValuePair<Type, IMessagePackFormatter>(t.derivedType, formatter);
            }));
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (formatter_map.TryGetValue(typeof(T), out var formatter))
                return (IMessagePackFormatter<T>)formatter;

            if (typeof(T).IsEnum)
            {
                if (enum_formatter_map.TryGetValue(typeof(T), out formatter))
                    return (IMessagePackFormatter<T>)formatter;

                return (IMessagePackFormatter<T>)(enum_formatter_map[typeof(T)] = (IMessagePackFormatter)Activator.CreateInstance(typeof(EnumFormatter<>).MakeGenericType(typeof(T)))!);
            }

            return StandardResolver.Instance.GetFormatterWithVerify<T>();
        }

        public class TypeRedirectingFormatter<TActual, TBase> : IMessagePackFormatter<TActual>
        {
            private readonly IMessagePackFormatter<TBase> baseFormatter;

            public TypeRedirectingFormatter()
            {
                baseFormatter = StandardResolver.Instance.GetFormatterWithVerify<TBase>();
            }

            public void Serialize(ref MessagePackWriter writer, TActual value, MessagePackSerializerOptions options)
                => baseFormatter.Serialize(ref writer, (TBase)(object)value!, StandardResolver.Options);

            public TActual Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
                => (TActual)(object)baseFormatter.Deserialize(ref reader, StandardResolver.Options)!;
        }

        public class EnumFormatter<T> : IMessagePackFormatter<T>
            where T : Enum
        {
            private readonly IMessagePackFormatter<T> formatter;

            public EnumFormatter()
            {
                formatter = StandardResolver.Instance.GetFormatterWithVerify<T>();
            }

            public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
            {
                EnumValueOutOfRangeException<T>.ThrowIfNotDefined(value);
                formatter.Serialize(ref writer, value, StandardResolver.Options);
            }

            public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                T result = formatter.Deserialize(ref reader, StandardResolver.Options);
                EnumValueOutOfRangeException<T>.ThrowIfNotDefined(result);
                return result;
            }
        }

        public class EnumValueOutOfRangeException<T> : Exception
            where T : Enum
        {
            public EnumValueOutOfRangeException(T value)
                : base($"Enum value '{value}' out of range for type '{typeof(T).ReadableName()}'")
            {
            }

            public static void ThrowIfNotDefined(T value)
            {
                if (Enum.IsDefined(typeof(T), value))
                    return;

                throw new EnumValueOutOfRangeException<T>(value);
            }
        }
    }
}
