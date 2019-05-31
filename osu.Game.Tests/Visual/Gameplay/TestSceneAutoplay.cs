﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    [Description("Player instantiated with an autoplay mod.")]
    public class TestSceneAutoplay : AllPlayersTestScene
    {
        protected override Player CreatePlayer(Ruleset ruleset)
        {
            Mods.Value = Mods.Value.Concat(new[] { ruleset.GetAutoplayMod() }).ToArray();
            return new ScoreAccessiblePlayer();
        }

        protected override void AddCheckSteps()
        {
            AddUntilStep("score above zero", () => ((ScoreAccessiblePlayer)Player).ScoreProcessor.TotalScore.Value > 0);
            AddUntilStep("key counter counted keys", () => ((ScoreAccessiblePlayer)Player).HUDOverlay.KeyCounter.Children.Any(kc => kc.CountPresses > 0));
        }

        private class ScoreAccessiblePlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
            public new HUDOverlay HUDOverlay => base.HUDOverlay;

            public ScoreAccessiblePlayer()
                : base(false, false)
            {
            }
        }
    }
}
