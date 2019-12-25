// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneLegacyBeatmapSkin : OsuTestScene
    {
        [Resolved]
        private AudioManager audio { get; set; }

        [Test]
        public void TestBeatmapComboColours()
        {
            ExposedPlayer player = null;
            IReadOnlyList<Color4> colours = null;

            AddStep("load beatmap", () => player = loadBeatmap(false, true));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddStep("retrieve combo colours", () => colours = player.BeatmapSkin.GetConfig<GlobalSkinConfiguration, IReadOnlyList<Color4>>(GlobalSkinConfiguration.ComboColours)?.Value);
            AddAssert("is beatmap skin colours", () => colours.SequenceEqual(TestBeatmapSkin.Colours));
        }

        [Test]
        public void TestEmptyBeatmapComboColours()
        {
            ExposedPlayer player = null;
            IReadOnlyList<Color4> colours = null;

            AddStep("load no-colour beatmap", () => player = loadBeatmap(false, false));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddStep("retrieve combo colours", () => colours = player.BeatmapSkin.GetConfig<GlobalSkinConfiguration, IReadOnlyList<Color4>>(GlobalSkinConfiguration.ComboColours)?.Value);
            AddAssert("is default user skin colours", () => colours.SequenceEqual(SkinConfiguration.DefaultComboColours));
        }

        [Test]
        public void TestEmptyBeatmapCustomSkinColours()
        {
            ExposedPlayer player = null;
            IReadOnlyList<Color4> colours = null;

            AddStep("load no-colour beatmap", () => player = loadBeatmap(true, false));
            AddUntilStep("wait for player", () => player.IsLoaded);

            AddStep("retrieve combo colours", () => colours = player.BeatmapSkin.GetConfig<GlobalSkinConfiguration, IReadOnlyList<Color4>>(GlobalSkinConfiguration.ComboColours)?.Value);
            AddAssert("is custom user skin colours", () => colours.SequenceEqual(TestSkin.Colours));
        }

        private ExposedPlayer loadBeatmap(bool userHasCustomColours, bool beatmapHasColours)
        {
            ExposedPlayer player;

            Beatmap.Value = new CustomSkinWorkingBeatmap(audio, beatmapHasColours);
            Child = new OsuScreenStack(player = new ExposedPlayer(userHasCustomColours)) { RelativeSizeAxes = Axes.Both };

            return player;
        }

        private class ExposedPlayer : Player
        {
            private readonly bool userHasCustomColours;

            public ExposedPlayer(bool userHasCustomColours)
                : base(false, false)
            {
                this.userHasCustomColours = userHasCustomColours;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                dependencies.CacheAs<ISkinSource>(new TestSkin(userHasCustomColours));
                return dependencies;
            }

            public BeatmapSkinProvidingContainer BeatmapSkin => GameplayClockContainer.OfType<ScalingContainer>().First().OfType<BeatmapSkinProvidingContainer>().First();
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
