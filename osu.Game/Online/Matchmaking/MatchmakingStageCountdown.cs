// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;

namespace osu.Game.Online.Matchmaking
{
    [MessagePackObject]
    public class MatchmakingStageCountdown : MultiplayerCountdown
    {
        [Key(2)]
        public MatchmakingStage Stage { get; set; }
    }
}
