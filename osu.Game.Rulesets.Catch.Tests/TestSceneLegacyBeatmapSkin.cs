// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;
using osuTK;
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
            AddAssert("is custom hyper dash colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashColour == CatchTestBeatmapSkin.HYPER_DASH_COLOUR);
            AddAssert("is custom hyper dash after image colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashAfterImageColour == CatchTestBeatmapSkin.HYPER_DASH_AFTER_IMAGE_COLOUR);
            AddAssert("is custom hyper dash fruit colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashFruitColour == CatchTestBeatmapSkin.HYPER_DASH_FRUIT_COLOUR);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBeatmapHyperDashColoursOverride(bool useBeatmapSkin)
        {
            TestBeatmap = new CatchCustomSkinWorkingBeatmap(audio, true);
            ConfigureTest(useBeatmapSkin, false, true);
            AddAssert("is custom hyper dash colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashColour == CatchTestSkin.HYPER_DASH_COLOUR);
            AddAssert("is custom hyper dash after image colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashAfterImageColour == CatchTestSkin.HYPER_DASH_AFTER_IMAGE_COLOUR);
            AddAssert("is custom hyper dash fruit colours", () => ((CatchExposedPlayer)TestPlayer).UsableHyperDashFruitColour == CatchTestSkin.HYPER_DASH_FRUIT_COLOUR);
        }

        protected override ExposedPlayer CreateTestPlayer(bool userHasCustomColours) => new CatchExposedPlayer(userHasCustomColours);

        private class CatchExposedPlayer : ExposedPlayer
        {
            public CatchExposedPlayer(bool userHasCustomColours)
                : base(userHasCustomColours)
            {
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                dependencies.CacheAs<ISkinSource>(new CatchTestSkin(UserHasCustomColours));
                return dependencies;
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

        private class TestJuiceStream : JuiceStream
        {
            public TestJuiceStream(float x)
            {
                X = x;

                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero),
                    new PathControlPoint(new Vector2(30, 0)),
                });
            }
        }

        private class CatchCustomSkinWorkingBeatmap : CustomSkinWorkingBeatmap
        {
            public CatchCustomSkinWorkingBeatmap(AudioManager audio, bool hasColours)
                : base(createBeatmap(new CatchRuleset().RulesetInfo), audio, hasColours)
            {
            }

            protected override ISkin GetSkin() => new CatchTestBeatmapSkin(BeatmapInfo, HasColours);

            private static IBeatmap createBeatmap(RulesetInfo ruleset)
            {
                var beatmap = new Beatmap
                {
                    BeatmapInfo =
                    {
                        Ruleset = ruleset,
                        BaseDifficulty = new BeatmapDifficulty { CircleSize = 3.6f }
                    }
                };

                beatmap.ControlPointInfo.Add(0, new TimingControlPoint());

                // Should produce a hyper-dash (edge case test)
                beatmap.HitObjects.Add(new Fruit { StartTime = 1816, X = 56, NewCombo = true });
                beatmap.HitObjects.Add(new Fruit { StartTime = 2008, X = 308, NewCombo = true });

                double startTime = 3000;

                const float left_x = 0.02f * CatchPlayfield.WIDTH;
                const float right_x = 0.98f * CatchPlayfield.WIDTH;

                createObjects(() => new Fruit { X = left_x });
                createObjects(() => new TestJuiceStream(right_x), 1);
                createObjects(() => new TestJuiceStream(left_x), 1);
                createObjects(() => new Fruit { X = right_x });
                createObjects(() => new Fruit { X = left_x });
                createObjects(() => new Fruit { X = right_x });
                createObjects(() => new TestJuiceStream(left_x), 1);

                beatmap.ControlPointInfo.Add(startTime, new TimingControlPoint
                {
                    BeatLength = 50
                });

                createObjects(() => new TestJuiceStream(left_x)
                {
                    Path = new SliderPath(new[]
                    {
                        new PathControlPoint(Vector2.Zero),
                        new PathControlPoint(new Vector2(512, 0))
                    })
                }, 1);

                return beatmap;

                void createObjects(Func<CatchHitObject> createObject, int count = 3)
                {
                    const float spacing = 140;

                    for (int i = 0; i < count; i++)
                    {
                        var hitObject = createObject();
                        hitObject.StartTime = startTime + i * spacing;
                        beatmap.HitObjects.Add(hitObject);
                    }

                    startTime += 700;
                }
            }
        }

        private class CatchTestBeatmapSkin : TestBeatmapSkin
        {
            public static readonly Color4 HYPER_DASH_COLOUR = Color4.DarkBlue;

            public static readonly Color4 HYPER_DASH_AFTER_IMAGE_COLOUR = Color4.DarkCyan;

            public static readonly Color4 HYPER_DASH_FRUIT_COLOUR = Color4.DarkGoldenrod;

            public CatchTestBeatmapSkin(BeatmapInfo beatmap, bool hasColours)
                : base(beatmap, hasColours)
            {
                if (hasColours)
                {
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDash.ToString(), HYPER_DASH_COLOUR);
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDashAfterImage.ToString(), HYPER_DASH_AFTER_IMAGE_COLOUR);
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDashFruit.ToString(), HYPER_DASH_FRUIT_COLOUR);
                }
            }
        }

        private class CatchTestSkin : TestSkin
        {
            public static readonly Color4 HYPER_DASH_COLOUR = Color4.LightBlue;

            public static readonly Color4 HYPER_DASH_AFTER_IMAGE_COLOUR = Color4.LightCoral;

            public static readonly Color4 HYPER_DASH_FRUIT_COLOUR = Color4.LightCyan;

            public CatchTestSkin(bool hasCustomColours)
                : base(hasCustomColours)
            {
                if (hasCustomColours)
                {
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDash.ToString(), HYPER_DASH_COLOUR);
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDashAfterImage.ToString(), HYPER_DASH_AFTER_IMAGE_COLOUR);
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDashFruit.ToString(), HYPER_DASH_FRUIT_COLOUR);
                }
            }
        }
    }
}
