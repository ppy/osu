// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        private class OsuCustomSkinWorkingBeatmap : CustomSkinWorkingBeatmap
        {
            public OsuCustomSkinWorkingBeatmap(AudioManager audio, bool hasColours)
                : base(createBeatmap(), audio, hasColours)
            {
            }

            private static IBeatmap createBeatmap() =>
                new Beatmap
                {
                    BeatmapInfo =
                    {
                        BeatmapSet = new BeatmapSetInfo(),
                        Ruleset = new OsuRuleset().RulesetInfo,
                    },
                    HitObjects = { new HitCircle { Position = new Vector2(256, 192) } }
                };
        }
    }
}
