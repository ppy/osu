// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning.Editor;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinEditorComponentsList : SkinnableTestScene
    {
        [Test]
        public void TestToggleEditor()
        {
            AddStep("show available components", () => SetContents(_ => new SkinComponentToolbox(300)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            }));
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();
    }
}
