// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneLegacyBeatmapSkin : LegacyBeatmapSkinColourTest
    {
        [Resolved]
        private AudioManager audio { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.BeatmapSkins, BeatmapSkins);
            config.BindWith(OsuSetting.BeatmapColours, BeatmapColours);
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public override void TestBeatmapComboColours(bool userHasCustomColours, bool useBeatmapSkin)
        {
            TestBeatmap = new OsuCustomSkinWorkingBeatmap(audio, true);
            base.TestBeatmapComboColours(userHasCustomColours, useBeatmapSkin);
            AddAssert("is beatmap skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(TestBeatmapSkin.Colours));
        }

        [TestCase(true)]
        [TestCase(false)]
        public override void TestBeatmapComboColoursOverride(bool useBeatmapSkin)
        {
            TestBeatmap = new OsuCustomSkinWorkingBeatmap(audio, true);
            base.TestBeatmapComboColoursOverride(useBeatmapSkin);
            AddAssert("is user custom skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(TestSkin.Colours));
        }

        [TestCase(true)]
        [TestCase(false)]
        public override void TestBeatmapComboColoursOverrideWithDefaultColours(bool useBeatmapSkin)
        {
            TestBeatmap = new OsuCustomSkinWorkingBeatmap(audio, true);
            base.TestBeatmapComboColoursOverrideWithDefaultColours(useBeatmapSkin);
            AddAssert("is default user skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(SkinConfiguration.DefaultComboColours));
        }

        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public override void TestBeatmapNoComboColours(bool useBeatmapSkin, bool useBeatmapColour)
        {
            TestBeatmap = new OsuCustomSkinWorkingBeatmap(audio, false);
            base.TestBeatmapNoComboColours(useBeatmapSkin, useBeatmapColour);
            AddAssert("is default user skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(SkinConfiguration.DefaultComboColours));
        }

        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public override void TestBeatmapNoComboColoursSkinOverride(bool useBeatmapSkin, bool useBeatmapColour)
        {
            TestBeatmap = new OsuCustomSkinWorkingBeatmap(audio, false);
            base.TestBeatmapNoComboColoursSkinOverride(useBeatmapSkin, useBeatmapColour);
            AddAssert("is custom user skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(TestSkin.Colours));
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void TestLegacyOffsetWithBeatmapColours(bool userHasCustomColours, bool useBeatmapSkin)
        {
            TestBeatmap = new OsuCustomSkinWorkingBeatmap(audio, true, getHitCirclesWithLegacyOffsets());
            base.TestBeatmapComboColours(userHasCustomColours, useBeatmapSkin);

            assertCorrectObjectComboColours("is beatmap skin colours with legacy offsets applied",
                TestBeatmapSkin.Colours,
                (i, obj) => i + 1 + obj.LegacyBeatmapComboOffset);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestLegacyOffsetWithIgnoredBeatmapColours(bool useBeatmapSkin)
        {
            TestBeatmap = new OsuCustomSkinWorkingBeatmap(audio, true, getHitCirclesWithLegacyOffsets());
            base.TestBeatmapComboColoursOverride(useBeatmapSkin);

            assertCorrectObjectComboColours("is user skin colours without legacy offsets applied",
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

                foreach (var drawable in TestPlayer.DrawableRuleset.Playfield.AllHitObjects)
                {
                    index = nextExpectedComboIndex(index, (OsuHitObject)drawable.HitObject);

                    if (drawable.AccentColour.Value != expectedColours[index % expectedColours.Length])
                        return false;
                }

                return true;
            });
        }

        private static IEnumerable<OsuHitObject> getHitCirclesWithLegacyOffsets()
        {
            var hitObjects = new List<OsuHitObject>();

            for (int i = 0; i < 5; i++)
            {
                hitObjects.Add(new HitCircle
                {
                    StartTime = i,
                    Position = new Vector2(256, 192),
                    NewCombo = true,
                    LegacyBeatmapComboOffset = i,
                });
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
