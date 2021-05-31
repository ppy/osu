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
