// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSkinEditorComponentsList : SkinnableTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Test]
        public void TestToggleEditor()
        {
            var skinComponentsContainer = new SkinComponentsContainer(new SkinComponentsContainerLookup(SkinComponentsContainerLookup.TargetArea.SongSelect));

            AddStep("show available components", () => SetContents(_ => new SkinComponentToolbox(skinComponentsContainer, null)
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Width = 0.6f,
            }));
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();
    }
}
