// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [Description("Player instantiated with an autoplay mod.")]
    public class TestCaseAutoplay : TestCasePlayer
    {
        protected override Player CreatePlayer(Ruleset ruleset)
        {
            Beatmap.Value.Mods.Value = Beatmap.Value.Mods.Value.Concat(new[] { ruleset.GetAutoplayMod() });
            return new ScoreAccessiblePlayer
            {
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false,
            };
        }

        protected override void AddCheckSteps(Func<Player> player)
        {
            base.AddCheckSteps(player);
            AddUntilStep(() => ((ScoreAccessiblePlayer)player()).ScoreProcessor.TotalScore > 0, "score above zero");
            AddUntilStep(() => ((ScoreAccessiblePlayer)player()).HUDOverlay.KeyCounter.Children.Any(kc => kc.CountPresses > 0), "key counter counted keys");
        }

        private class ScoreAccessiblePlayer : Player
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
            public new HUDOverlay HUDOverlay => base.HUDOverlay;
        }
    }
}
