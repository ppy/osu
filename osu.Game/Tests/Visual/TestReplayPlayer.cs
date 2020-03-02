// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public class TestReplayPlayer : ReplayPlayer
    {
        protected override bool PauseOnFocusLost { get; }

        public new DrawableRuleset DrawableRuleset => base.DrawableRuleset;

        public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

        public TestReplayPlayer(Score score, bool allowPause = true, bool showResults = true, bool pauseOnFocusLost = false)
            : base(score, allowPause, showResults)
        {
            PauseOnFocusLost = pauseOnFocusLost;
        }
    }
}
