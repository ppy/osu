// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneLegacyBeatmapSkin : ScreenTestScene
    {
        [Resolved]
        private AudioManager audio { get; set; }

        private readonly Bindable<bool> beatmapSkins = new Bindable<bool>();
        private readonly Bindable<bool> beatmapColours = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.BeatmapSkins, beatmapSkins);
            config.BindWith(OsuSetting.BeatmapColours, beatmapColours);
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void TestBeatmapComboColours(bool userHasCustomColours, bool useBeatmapSkin)
        {
            ExposedPlayer player = null;

            configureSettings(useBeatmapSkin, true);
            AddStep("load coloured beatmap", () => player = loadBeatmap(userHasCustomColours, true));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("is beatmap skin colours", () => player.UsableComboColours.SequenceEqual(TestBeatmapSkin.Colours));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBeatmapComboColoursOverride(bool useBeatmapSkin)
        {
            ExposedPlayer player = null;

            configureSettings(useBeatmapSkin, false);
            AddStep("load coloured beatmap", () => player = loadBeatmap(true, true));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("is user custom skin colours", () => player.UsableComboColours.SequenceEqual(TestSkin.Colours));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBeatmapComboColoursOverrideWithDefaultColours(bool useBeatmapSkin)
        {
            ExposedPlayer player = null;

            configureSettings(useBeatmapSkin, false);
            AddStep("load coloured beatmap", () => player = loadBeatmap(false, true));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("is default user skin colours", () => player.UsableComboColours.SequenceEqual(SkinConfiguration.DefaultComboColours));
        }

        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public void TestBeatmapNoComboColours(bool useBeatmapSkin, bool useBeatmapColour)
        {
            ExposedPlayer player = null;

            configureSettings(useBeatmapSkin, useBeatmapColour);
            AddStep("load no-colour beatmap", () => player = loadBeatmap(false, false));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("is default user skin colours", () => player.UsableComboColours.SequenceEqual(SkinConfiguration.DefaultComboColours));
        }

        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public void TestBeatmapNoComboColoursSkinOverride(bool useBeatmapSkin, bool useBeatmapColour)
        {
            ExposedPlayer player = null;

            configureSettings(useBeatmapSkin, useBeatmapColour);
            AddStep("load custom-skin colour", () => player = loadBeatmap(true, false));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("is custom user skin colours", () => player.UsableComboColours.SequenceEqual(TestSkin.Colours));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBeatmapHyperDashColours(bool useBeatmapSkin)
        {
            ExposedPlayer player = null;

            configureSettings(useBeatmapSkin, true);
            AddStep("load custom-skin colour", () => player = loadBeatmap(true, true));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("is custom hyper dash colours", () => player.UsableHyperDashColour == TestBeatmapSkin.HYPER_DASH_COLOUR);
            AddAssert("is custom hyper dash after image colours", () => player.UsableHyperDashAfterImageColour == TestBeatmapSkin.HYPER_DASH_AFTER_IMAGE_COLOUR);
            AddAssert("is custom hyper dash fruit colours", () => player.UsableHyperDashFruitColour == TestBeatmapSkin.HYPER_DASH_FRUIT_COLOUR);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestBeatmapHyperDashColoursOverride(bool useBeatmapSkin)
        {
            ExposedPlayer player = null;

            configureSettings(useBeatmapSkin, false);
            AddStep("load custom-skin colour", () => player = loadBeatmap(true, true));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("is custom hyper dash colours", () => player.UsableHyperDashColour == TestSkin.HYPER_DASH_COLOUR);
            AddAssert("is custom hyper dash after image colours", () => player.UsableHyperDashAfterImageColour == TestSkin.HYPER_DASH_AFTER_IMAGE_COLOUR);
            AddAssert("is custom hyper dash fruit colours", () => player.UsableHyperDashFruitColour == TestSkin.HYPER_DASH_FRUIT_COLOUR);
        }

        private ExposedPlayer loadBeatmap(bool userHasCustomColours, bool beatmapHasColours)
        {
            ExposedPlayer player;

            Beatmap.Value = new CustomSkinWorkingBeatmap(audio, beatmapHasColours);

            LoadScreen(player = new ExposedPlayer(userHasCustomColours));

            return player;
        }

        private void configureSettings(bool beatmapSkins, bool beatmapColours)
        {
            AddStep($"{(beatmapSkins ? "enable" : "disable")} beatmap skins", () =>
            {
                this.beatmapSkins.Value = beatmapSkins;
            });
            AddStep($"{(beatmapColours ? "enable" : "disable")} beatmap colours", () =>
            {
                this.beatmapColours.Value = beatmapColours;
            });
        }

        private class ExposedPlayer : Player
        {
            private readonly bool userHasCustomColours;

            public ExposedPlayer(bool userHasCustomColours)
                : base(new PlayerConfiguration
                {
                    AllowPause = false,
                    ShowResults = false,
                })
            {
                this.userHasCustomColours = userHasCustomColours;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                dependencies.CacheAs<ISkinSource>(new TestSkin(userHasCustomColours));
                return dependencies;
            }

            public IReadOnlyList<Color4> UsableComboColours =>
                GameplayClockContainer.ChildrenOfType<BeatmapSkinProvidingContainer>()
                                      .First()
                                      .GetConfig<GlobalSkinColours, IReadOnlyList<Color4>>(GlobalSkinColours.ComboColours)?.Value;

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

        private class CustomSkinWorkingBeatmap : ClockBackedTestWorkingBeatmap
        {
            private readonly bool hasColours;

            public CustomSkinWorkingBeatmap(AudioManager audio, bool hasColours)
                : base(createBeatmap(new CatchRuleset().RulesetInfo), null, null, audio)
            {
                this.hasColours = hasColours;
            }

            protected override ISkin GetSkin() => new TestBeatmapSkin(BeatmapInfo, hasColours);

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

        private class TestBeatmapSkin : LegacyBeatmapSkin
        {
            public static Color4[] Colours { get; } =
            {
                new Color4(50, 100, 150, 255),
                new Color4(40, 80, 120, 255),
            };

            public static readonly Color4 HYPER_DASH_COLOUR = Color4.DarkBlue;

            public static readonly Color4 HYPER_DASH_AFTER_IMAGE_COLOUR = Color4.DarkCyan;

            public static readonly Color4 HYPER_DASH_FRUIT_COLOUR = Color4.DarkGoldenrod;

            public TestBeatmapSkin(BeatmapInfo beatmap, bool hasColours)
                : base(beatmap, new ResourceStore<byte[]>(), null)
            {
                if (hasColours)
                {
                    Configuration.AddComboColours(Colours);
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDash.ToString(), HYPER_DASH_COLOUR);
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDashAfterImage.ToString(), HYPER_DASH_AFTER_IMAGE_COLOUR);
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDashFruit.ToString(), HYPER_DASH_FRUIT_COLOUR);
                }
            }
        }

        private class TestSkin : LegacySkin, ISkinSource
        {
            public static Color4[] Colours { get; } =
            {
                new Color4(150, 100, 50, 255),
                new Color4(20, 20, 20, 255),
            };

            public static readonly Color4 HYPER_DASH_COLOUR = Color4.LightBlue;

            public static readonly Color4 HYPER_DASH_AFTER_IMAGE_COLOUR = Color4.LightCoral;

            public static readonly Color4 HYPER_DASH_FRUIT_COLOUR = Color4.LightCyan;

            public TestSkin(bool hasCustomColours)
                : base(new SkinInfo(), new ResourceStore<byte[]>(), null, string.Empty)
            {
                if (hasCustomColours)
                {
                    Configuration.AddComboColours(Colours);
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDash.ToString(), HYPER_DASH_COLOUR);
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDashAfterImage.ToString(), HYPER_DASH_AFTER_IMAGE_COLOUR);
                    Configuration.CustomColours.Add(CatchSkinColour.HyperDashFruit.ToString(), HYPER_DASH_FRUIT_COLOUR);
                }
            }

            public event Action SourceChanged
            {
                add { }
                remove { }
            }
        }
    }
}
