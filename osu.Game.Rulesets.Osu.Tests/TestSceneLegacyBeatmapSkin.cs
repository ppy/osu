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
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
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

        [TestCase(true, true, true)]
        [TestCase(true, false, true)]
        [TestCase(false, true, true)]
        [TestCase(false, false, true)]
        public void TestBeatmapComboColours(bool userHasCustomColours, bool useBeatmapSkin, bool useBeatmapColour)
        {
            ExposedPlayer player = null;

            configureSettings(useBeatmapSkin, useBeatmapColour);
            AddStep("load coloured beatmap", () => player = loadBeatmap(userHasCustomColours, true));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("is beatmap skin colours", () => player.UsableComboColours.SequenceEqual(TestBeatmapSkin.Colours));
        }

        [TestCase(true, false)]
        [TestCase(false, false)]
        public void TestBeatmapComboColoursOverride(bool useBeatmapSkin, bool useBeatmapColour)
        {
            ExposedPlayer player = null;

            configureSettings(useBeatmapSkin, useBeatmapColour);
            AddStep("load coloured beatmap", () => player = loadBeatmap(true, true));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddAssert("is user custom skin colours", () => player.UsableComboColours.SequenceEqual(TestSkin.Colours));
        }

        [TestCase(true, false)]
        [TestCase(false, false)]
        public void TestBeatmapComboColoursOverrideWithDefaultColours(bool useBeatmapSkin, bool useBeatmapColour)
        {
            ExposedPlayer player = null;

            configureSettings(useBeatmapSkin, useBeatmapColour);
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
        }

        private class CustomSkinWorkingBeatmap : ClockBackedTestWorkingBeatmap
        {
            private readonly bool hasColours;

            public CustomSkinWorkingBeatmap(AudioManager audio, bool hasColours)
                : base(new Beatmap
                {
                    BeatmapInfo =
                    {
                        BeatmapSet = new BeatmapSetInfo(),
                        Ruleset = new OsuRuleset().RulesetInfo,
                    },
                    HitObjects = { new HitCircle { Position = new Vector2(256, 192) } }
                }, null, null, audio)
            {
                this.hasColours = hasColours;
            }

            protected override ISkin GetSkin() => new TestBeatmapSkin(BeatmapInfo, hasColours);
        }

        private class TestBeatmapSkin : LegacyBeatmapSkin
        {
            public static Color4[] Colours { get; } =
            {
                new Color4(50, 100, 150, 255),
                new Color4(40, 80, 120, 255),
            };

            public TestBeatmapSkin(BeatmapInfo beatmap, bool hasColours)
                : base(beatmap, new ResourceStore<byte[]>(), null)
            {
                if (hasColours)
                    Configuration.AddComboColours(Colours);
            }
        }

        private class TestSkin : LegacySkin, ISkinSource
        {
            public static Color4[] Colours { get; } =
            {
                new Color4(150, 100, 50, 255),
                new Color4(20, 20, 20, 255),
            };

            public TestSkin(bool hasCustomColours)
                : base(new SkinInfo(), null, null, string.Empty)
            {
                if (hasCustomColours)
                    Configuration.AddComboColours(Colours);
            }

            public event Action SourceChanged
            {
                add { }
                remove { }
            }
        }
    }
}
