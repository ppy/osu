// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Matchmaking.Events;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Users;

namespace osu.Game.Online
{
    /// <summary>
    /// A static class providing the list of types requiring workarounds for serialisation in SignalR.
    /// </summary>
    /// <seealso cref="SignalRUnionWorkaroundResolver"/>
    internal static class SignalRWorkaroundTypes
    {
        internal static readonly IReadOnlyList<(Type derivedType, Type baseType)> BASE_TYPE_MAPPING = new[]
        {
            // multiplayer
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

            // metadata
            (typeof(UserActivity.ChoosingBeatmap), typeof(UserActivity)),
            (typeof(UserActivity.InSoloGame), typeof(UserActivity)),
            (typeof(UserActivity.WatchingReplay), typeof(UserActivity)),
            (typeof(UserActivity.SpectatingUser), typeof(UserActivity)),
            (typeof(UserActivity.SearchingForLobby), typeof(UserActivity)),
            (typeof(UserActivity.InLobby), typeof(UserActivity)),
            (typeof(UserActivity.InMultiplayerGame), typeof(UserActivity)),
            (typeof(UserActivity.SpectatingMultiplayerGame), typeof(UserActivity)),
            (typeof(UserActivity.InPlaylistGame), typeof(UserActivity)),
            (typeof(UserActivity.EditingBeatmap), typeof(UserActivity)),
            (typeof(UserActivity.ModdingBeatmap), typeof(UserActivity)),
            (typeof(UserActivity.TestingBeatmap), typeof(UserActivity)),
            (typeof(UserActivity.InDailyChallengeLobby), typeof(UserActivity)),
            (typeof(UserActivity.PlayingDailyChallenge), typeof(UserActivity)),

            // matchmaking
            (typeof(MatchmakingQueueStatus.Searching), typeof(MatchmakingQueueStatus)),
            (typeof(MatchmakingQueueStatus.MatchFound), typeof(MatchmakingQueueStatus)),
            (typeof(MatchmakingQueueStatus.JoiningMatch), typeof(MatchmakingQueueStatus)),
            (typeof(MatchmakingRoomState), typeof(MatchRoomState)),
            (typeof(MatchmakingStageCountdown), typeof(MultiplayerCountdown)),
            (typeof(MatchmakingAvatarActionRequest), typeof(MatchUserRequest)),
            (typeof(MatchmakingAvatarActionEvent), typeof(MatchServerEvent)),
        };
    }
}
