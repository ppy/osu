// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Testing;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneLegacyComboFire : TestScene
    {
        private readonly ScoreProcessor scoreProcessor;

        public TestSceneLegacyComboFire()
        {
            scoreProcessor = new ScoreProcessor(new OsuRuleset());

            AddSliderStep("combo", 0, 500, 250, combo => scoreProcessor.Combo.Value = combo);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var skin = new LegacySkin(new SkinInfo(), null!);
            skin.Configuration.ConfigDictionary["ComboFire"] = "1";

            Add(new SkinProvidingContainer(skin)
            {
                Child = new LegacyComboFire(scoreProcessor),
            });
        }
    }
}
