// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerSpectatorPlayer : SpectatorPlayer
    {
        public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

        public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

        public MultiplayerSpectatorPlayer(Score score)
            : base(score)
        {
        }
    }
}
