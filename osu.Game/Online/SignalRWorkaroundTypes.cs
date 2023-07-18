// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;

namespace osu.Game.Online
{
    /// <summary>
    /// A static class providing the list of types requiring workarounds for serialisation in SignalR.
    /// </summary>
    /// <seealso cref="SignalRUnionWorkaroundResolver"/>
    /// <seealso cref="SignalRDerivedTypeWorkaroundJsonConverter"/>
    internal static class SignalRWorkaroundTypes
    {
        internal static readonly IReadOnlyList<(Type derivedType, Type baseType)> BASE_TYPE_MAPPING = new[]
        {
            (typeof(ChangeTeamRequest), typeof(MatchUserRequest)),
            (typeof(StartMatchCountdownRequest), typeof(MatchUserRequest)),
            (typeof(StopCountdownRequest), typeof(MatchUserRequest)),
            (typeof(CountdownStartedEvent), typeof(MatchServerEvent)),
            (typeof(CountdownStoppedEvent), typeof(MatchServerEvent)),
            (typeof(TeamVersusRoomState), typeof(MatchRoomState)),
            (typeof(TeamVersusUserState), typeof(MatchUserState)),
            (typeof(MatchStartCountdown), typeof(MultiplayerCountdown)),
            (typeof(ForceGameplayStartCountdown), typeof(MultiplayerCountdown)),
            (typeof(ServerShuttingDownCountdown), typeof(MultiplayerCountdown)),
        };
    }
}
