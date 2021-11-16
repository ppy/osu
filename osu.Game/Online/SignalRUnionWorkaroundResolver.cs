// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;

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

        public static readonly IReadOnlyList<Type> BASE_TYPES = new[]
        {
            typeof(MatchServerEvent),
            typeof(MatchUserRequest),
            typeof(MatchRoomState),
            typeof(MatchUserState),
        };

        public static readonly IReadOnlyList<Type> DERIVED_TYPES = new[]
        {
            typeof(ChangeTeamRequest),
            typeof(TeamVersusRoomState),
            typeof(TeamVersusUserState),
        };

        private static readonly IReadOnlyDictionary<Type, IMessagePackFormatter> formatter_map = new Dictionary<Type, IMessagePackFormatter>
        {
            { typeof(TeamVersusUserState), new TypeRedirectingFormatter<TeamVersusUserState, MatchUserState>() },
            { typeof(TeamVersusRoomState), new TypeRedirectingFormatter<TeamVersusRoomState, MatchRoomState>() },
            { typeof(ChangeTeamRequest), new TypeRedirectingFormatter<ChangeTeamRequest, MatchUserRequest>() },

            // These should not be required. The fallback should work. But something is weird with the way caching is done.
            // For future adventurers, I would not advise looking into this further. It's likely not worth the effort.
            { typeof(MatchUserState), new TypeRedirectingFormatter<MatchUserState, MatchUserState>() },
            { typeof(MatchRoomState), new TypeRedirectingFormatter<MatchRoomState, MatchRoomState>() },
            { typeof(MatchUserRequest), new TypeRedirectingFormatter<MatchUserRequest, MatchUserRequest>() },
            { typeof(MatchServerEvent), new TypeRedirectingFormatter<MatchServerEvent, MatchServerEvent>() },
        };

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
