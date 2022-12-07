// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneModSettingsArea : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Test]
        public void TestModToggleArea()
        {
            ModSettingsArea modSettingsArea = null;

            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = modSettingsArea = new ModSettingsArea()
            });
            AddStep("set DT", () => modSettingsArea.SelectedMods.Value = new[] { new OsuModDoubleTime() });
            AddStep("set DA", () => modSettingsArea.SelectedMods.Value = new Mod[] { new OsuModDifficultyAdjust() });
            AddStep("set FL+WU+DA+AD", () => modSettingsArea.SelectedMods.Value = new Mod[] { new OsuModFlashlight(), new ModWindUp(), new OsuModDifficultyAdjust(), new OsuModApproachDifferent() });
            AddStep("set empty", () => modSettingsArea.SelectedMods.Value = Array.Empty<Mod>());
        }
    }
}
