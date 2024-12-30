// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editing
{
    [HeadlessTest]
    public partial class TestSceneColoursSection : OsuManualInputManagerTestScene
    {
        [Test]
        public void TestNoBeatmapSkinColours()
        {
            LegacyBeatmapSkin skin = null!;
            ColoursSection coloursSection = null!;

            AddStep("create beatmap skin", () => skin = new LegacyBeatmapSkin(new BeatmapInfo(), null));
            AddStep("create colours section", () => Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies =
                [
                    (typeof(EditorBeatmap), new EditorBeatmap(new Beatmap
                    {
                        BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo }
                    }, skin)),
                    (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Aquamarine))
                ],
                Child = coloursSection = new ColoursSection
                {
                    RelativeSizeAxes = Axes.X,
                }
            });
            AddAssert("beatmap skin has no colours", () => skin.Configuration.CustomComboColours, () => Is.Empty);
            AddAssert("section displays default combo colours",
                () => coloursSection.ChildrenOfType<FormColourPalette>().Single().Colours,
                () => Is.EquivalentTo(new Colour4[]
                {
                    SkinConfiguration.DefaultComboColours[1],
                    SkinConfiguration.DefaultComboColours[2],
                    SkinConfiguration.DefaultComboColours[3],
                    SkinConfiguration.DefaultComboColours[0],
                }));

            AddStep("add a colour", () => coloursSection.ChildrenOfType<FormColourPalette>().Single().Colours.Add(Colour4.Aqua));
            AddAssert("beatmap skin has colours",
                () => skin.Configuration.CustomComboColours,
                () => Is.EquivalentTo(new[]
                {
                    SkinConfiguration.DefaultComboColours[1],
                    SkinConfiguration.DefaultComboColours[2],
                    SkinConfiguration.DefaultComboColours[3],
                    Color4.Aqua,
                    SkinConfiguration.DefaultComboColours[0],
                }));
        }

        [Test]
        public void TestExistingColours()
        {
            LegacyBeatmapSkin skin = null!;
            ColoursSection coloursSection = null!;

            AddStep("create beatmap skin", () =>
            {
                skin = new LegacyBeatmapSkin(new BeatmapInfo(), null);
                skin.Configuration.CustomComboColours = new List<Color4>
                {
                    Color4.Azure,
                    Color4.Beige,
                    Color4.Chartreuse
                };
            });
            AddStep("create colours section", () => Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies =
                [
                    (typeof(EditorBeatmap), new EditorBeatmap(new Beatmap
                    {
                        BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo }
                    }, skin)),
                    (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Aquamarine))
                ],
                Child = coloursSection = new ColoursSection
                {
                    RelativeSizeAxes = Axes.X,
                }
            });
            AddAssert("section displays combo colours",
                () => coloursSection.ChildrenOfType<FormColourPalette>().Single().Colours,
                () => Is.EquivalentTo(new[]
                {
                    Colour4.Beige,
                    Colour4.Chartreuse,
                    Colour4.Azure,
                }));

            AddStep("add a colour", () => coloursSection.ChildrenOfType<FormColourPalette>().Single().Colours.Add(Colour4.Aqua));
            AddAssert("beatmap skin has colours",
                () => skin.Configuration.CustomComboColours,
                () => Is.EquivalentTo(new[]
                {
                    Color4.Azure,
                    Color4.Beige,
                    Color4.Aqua,
                    Color4.Chartreuse
                }));
        }
    }
}
