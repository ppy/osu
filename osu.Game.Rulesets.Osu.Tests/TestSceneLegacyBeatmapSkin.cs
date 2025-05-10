// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneLegacyBeatmapSkin : LegacyBeatmapSkinColourTest
    {
        [Resolved]
        private AudioManager audio { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.BeatmapSkins, BeatmapSkins);
            config.BindWith(OsuSetting.BeatmapColours, BeatmapColours);

            config.SetValue(OsuSetting.ComboColourNormalisationAmount, 0f);
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void TestBeatmapComboColours(bool userHasCustomColours, bool useBeatmapSkin)
        {
            PrepareBeatmap(() => new OsuCustomSkinWorkingBeatmap(audio, true));
            ConfigureTest(useBeatmapSkin, true, userHasCustomColours);
            AddAssert("is beatmap skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(TestBeatmapSkin.Colours));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBeatmapComboColoursOverride(bool useBeatmapSkin)
        {
            PrepareBeatmap(() => new OsuCustomSkinWorkingBeatmap(audio, true));
            ConfigureTest(useBeatmapSkin, false, true);
            AddAssert("is user custom skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(TestSkin.Colours));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBeatmapComboColoursOverrideWithDefaultColours(bool useBeatmapSkin)
        {
            PrepareBeatmap(() => new OsuCustomSkinWorkingBeatmap(audio, true));
            ConfigureTest(useBeatmapSkin, false, false);
            AddAssert("is default user skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(SkinConfiguration.DefaultComboColours));
        }

        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public void TestBeatmapNoComboColours(bool useBeatmapSkin, bool useBeatmapColour)
        {
            PrepareBeatmap(() => new OsuCustomSkinWorkingBeatmap(audio, false));
            ConfigureTest(useBeatmapSkin, useBeatmapColour, false);
            AddAssert("is default user skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(SkinConfiguration.DefaultComboColours));
        }

        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public void TestBeatmapNoComboColoursSkinOverride(bool useBeatmapSkin, bool useBeatmapColour)
        {
            PrepareBeatmap(() => new OsuCustomSkinWorkingBeatmap(audio, false));
            ConfigureTest(useBeatmapSkin, useBeatmapColour, true);
            AddAssert("is custom user skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(TestSkin.Colours));
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void TestComboOffsetWithBeatmapColours(bool userHasCustomColours, bool useBeatmapSkin)
        {
            PrepareBeatmap(() => new OsuCustomSkinWorkingBeatmap(audio, true, getHitCirclesWithLegacyOffsets()));
            ConfigureTest(useBeatmapSkin, true, userHasCustomColours);
            assertCorrectObjectComboColours("is beatmap skin colours with combo offsets applied",
                TestBeatmapSkin.Colours,
                (i, obj) => i + 1 + obj.ComboOffset);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestComboOffsetWithIgnoredBeatmapColours(bool useBeatmapSkin)
        {
            PrepareBeatmap(() => new OsuCustomSkinWorkingBeatmap(audio, true, getHitCirclesWithLegacyOffsets()));
            ConfigureTest(useBeatmapSkin, false, true);
            assertCorrectObjectComboColours("is user skin colours without combo offsets applied",
                TestSkin.Colours,
                (i, _) => i + 1);
        }

        private void assertCorrectObjectComboColours(string description, Color4[] expectedColours, Func<int, OsuHitObject, int> nextExpectedComboIndex)
        {
            AddUntilStep("wait for objects to become alive", () =>
                TestPlayer.DrawableRuleset.Playfield.AllHitObjects.Count() == TestPlayer.DrawableRuleset.Objects.Count());

            AddAssert(description, () =>
            {
                int index = 0;

                return TestPlayer.DrawableRuleset.Playfield.AllHitObjects.All(d =>
                {
                    index = nextExpectedComboIndex(index, (OsuHitObject)d.HitObject);
                    return checkComboColour(d, expectedColours[index % expectedColours.Length]);
                });
            });

            static bool checkComboColour(DrawableHitObject drawableHitObject, Color4 expectedColour)
            {
                return drawableHitObject.AccentColour.Value == expectedColour &&
                       drawableHitObject.NestedHitObjects.All(n => checkComboColour(n, expectedColour));
            }
        }

        private static IEnumerable<OsuHitObject> getHitCirclesWithLegacyOffsets()
        {
            var hitObjects = new List<OsuHitObject>();

            for (int i = 0; i < 10; i++)
            {
                var hitObject = i % 2 == 0
                    ? (OsuHitObject)new HitCircle()
                    : new Slider
                    {
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(new Vector2(0, 0)),
                            new PathControlPoint(new Vector2(100, 0)),
                        })
                    };

                hitObject.StartTime = i;
                hitObject.Position = new Vector2(256, 192);
                hitObject.NewCombo = true;
                hitObject.ComboOffset = i;

                hitObjects.Add(hitObject);
            }

            return hitObjects;
        }

        private class OsuCustomSkinWorkingBeatmap : CustomSkinWorkingBeatmap
        {
            public OsuCustomSkinWorkingBeatmap(AudioManager audio, bool hasColours, IEnumerable<OsuHitObject> hitObjects = null)
                : base(createBeatmap(hitObjects), audio, hasColours)
            {
            }

            private static IBeatmap createBeatmap(IEnumerable<OsuHitObject> hitObjects)
            {
                var beatmap = new Beatmap
                {
                    BeatmapInfo =
                    {
                        BeatmapSet = new BeatmapSetInfo(),
                        Ruleset = new OsuRuleset().RulesetInfo,
                    },
                };

                beatmap.HitObjects.AddRange(hitObjects ?? new[]
                {
                    new HitCircle { Position = new Vector2(256, 192) }
                });

                return beatmap;
            }
        }
    }
}
