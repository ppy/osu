// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
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
            TestBeatmap = new CatchCustomSkinWorkingBeatmap(audio, true);
            base.TestBeatmapComboColours(userHasCustomColours, useBeatmapSkin);
            AddAssert("is beatmap skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(TestBeatmapSkin.Colours));
        }

        [TestCase(true)]
        [TestCase(false)]
        public override void TestBeatmapComboColoursOverride(bool useBeatmapSkin)
        {
            TestBeatmap = new CatchCustomSkinWorkingBeatmap(audio, true);
            base.TestBeatmapComboColoursOverride(useBeatmapSkin);
            AddAssert("is user custom skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(TestSkin.Colours));
        }

        [TestCase(true)]
        [TestCase(false)]
        public override void TestBeatmapComboColoursOverrideWithDefaultColours(bool useBeatmapSkin)
        {
            TestBeatmap = new CatchCustomSkinWorkingBeatmap(audio, true);
            base.TestBeatmapComboColoursOverrideWithDefaultColours(useBeatmapSkin);
            AddAssert("is default user skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(SkinConfiguration.DefaultComboColours));
        }

        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public override void TestBeatmapNoComboColours(bool useBeatmapSkin, bool useBeatmapColour)
        {
            TestBeatmap = new CatchCustomSkinWorkingBeatmap(audio, false);
            base.TestBeatmapNoComboColours(useBeatmapSkin, useBeatmapColour);
            AddAssert("is default user skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(SkinConfiguration.DefaultComboColours));
        }

        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public override void TestBeatmapNoComboColoursSkinOverride(bool useBeatmapSkin, bool useBeatmapColour)
        {
            TestBeatmap = new CatchCustomSkinWorkingBeatmap(audio, false);
            base.TestBeatmapNoComboColoursSkinOverride(useBeatmapSkin, useBeatmapColour);
            AddAssert("is custom user skin colours", () => TestPlayer.UsableComboColours.SequenceEqual(TestSkin.Colours));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBeatmapHyperDashColours(bool useBeatmapSkin)
        {
            TestBeatmap = new CatchCustomSkinWorkingBeatmap(audio, true);
            ConfigureTest(useBeatmapSkin, true, true);
            AddAssert("is custom hyper dash colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashColour == TestBeatmapSkin.HYPER_DASH_COLOUR);
            AddAssert("is custom hyper dash after image colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashAfterImageColour == TestBeatmapSkin.HYPER_DASH_AFTER_IMAGE_COLOUR);
            AddAssert("is custom hyper dash fruit colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashFruitColour == TestBeatmapSkin.HYPER_DASH_FRUIT_COLOUR);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBeatmapHyperDashColoursOverride(bool useBeatmapSkin)
        {
            TestBeatmap = new CatchCustomSkinWorkingBeatmap(audio, true);
            ConfigureTest(useBeatmapSkin, false, true);
            AddAssert("is custom hyper dash colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashColour == TestSkin.HYPER_DASH_COLOUR);
            AddAssert("is custom hyper dash after image colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashAfterImageColour == TestSkin.HYPER_DASH_AFTER_IMAGE_COLOUR);
            AddAssert("is custom hyper dash fruit colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashFruitColour == TestSkin.HYPER_DASH_FRUIT_COLOUR);
        }

        protected override ExposedPlayer CreateTestPlayer(bool userHasCustomColours) => new CatchExposedPlayer(userHasCustomColours);

        private class CatchExposedPlayer : ExposedPlayer
        {
            public CatchExposedPlayer(bool userHasCustomColours)
                : base(userHasCustomColours)
            {
            }

            public Color4 UsableHyperDashColour =>
                GameplayClockContainer.ChildrenOfType<BeatmapSkinProvidingContainer>()
                                      .First()
                                      .GetConfig<SkinCustomColourLookup, Color4>(new SkinCustomColourLookup(CatchSkinColour.HyperDash))?
                                      .Value ?? Color4.Red;

            public Color4 UsableHyperDashAfterImageColour =>
                GameplayClockContainer.ChildrenOfType<BeatmapSkinProvidingContainer>()
                                      .First()
                                      .GetConfig<SkinCustomColourLookup, Color4>(new SkinCustomColourLookup(CatchSkinColour.HyperDashAfterImage))?
                                      .Value ?? Color4.Red;

            public Color4 UsableHyperDashFruitColour =>
                GameplayClockContainer.ChildrenOfType<BeatmapSkinProvidingContainer>()
                                      .First()
                                      .GetConfig<SkinCustomColourLookup, Color4>(new SkinCustomColourLookup(CatchSkinColour.HyperDashFruit))?
                                      .Value ?? Color4.Red;
        }

        private class CatchCustomSkinWorkingBeatmap : CustomSkinWorkingBeatmap
        {
            public CatchCustomSkinWorkingBeatmap(AudioManager audio, bool hasColours)
                : base(createBeatmap(), audio, hasColours)
            {
            }

            private static IBeatmap createBeatmap() =>
                new Beatmap
                {
                    BeatmapInfo =
                    {
                        BeatmapSet = new BeatmapSetInfo(),
                        Ruleset = new CatchRuleset().RulesetInfo
                    },
                    HitObjects = { new Fruit { StartTime = 1816, X = 56, NewCombo = true } }
                };
        }
    }
}
