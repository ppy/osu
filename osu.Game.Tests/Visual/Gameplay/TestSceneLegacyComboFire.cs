// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneLegacyComboFire : TestScene
    {
        [Cached]
        private readonly ScoreProcessor scoreProcessor = new ScoreProcessor(new OsuRuleset());

        public TestSceneLegacyComboFire()
        {
            AddSliderStep("combo", 0, 500, 250, combo => scoreProcessor.Combo.Value = combo);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(new LegacyComboFire());
        }
    }
}
