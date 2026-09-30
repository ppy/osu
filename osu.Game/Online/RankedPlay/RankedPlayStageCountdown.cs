// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;

namespace osu.Game.Online.RankedPlay
{
    [MessagePackObject]
    public class RankedPlayStageCountdown : MultiplayerCountdown
    {
        [Key(2)]
        public RankedPlayStage Stage { get; set; }
    }
}
