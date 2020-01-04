// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public class TestPlayer : Player
    {
        protected override bool PauseOnFocusLost { get; }

        public new DrawableRuleset DrawableRuleset => base.DrawableRuleset;

        public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

        public TestPlayer(bool allowPause = true, bool showResults = true, bool pauseOnFocusLost = false)
            : base(allowPause, showResults)
        {
            PauseOnFocusLost = pauseOnFocusLost;
        }
    }
}
