// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Audio;
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
            ISkin gameplaySkin = null;
            IReadOnlyList<Color4> colours = null;

            AddStep("load beatmap", () => player = loadBeatmap(false, true));
            AddUntilStep("wait for player", () => player.IsLoaded);
            AddStep("attach skin requester", () => gameplaySkin = addSkinRequester(player));

            AddStep("retrieve combo colours", () => colours = gameplaySkin.GetConfig<GlobalSkinConfiguration, IReadOnlyList<Color4>>(GlobalSkinConfiguration.ComboColours)?.Value);
            AddAssert("is beatmap colours", () => colours.SequenceEqual(TestBeatmapSkin.Colours));
        }

        [Test]
        public void TestEmptyBeatmapComboColours()
        {
            ExposedPlayer player = null;
            ISkin gameplaySkin = null;
            IReadOnlyList<Color4> colours = null;

            AddStep("load no-colour beatmap", () => player = loadBeatmap(false, false));
            AddUntilStep("wait for player", () => player.IsLoaded);
            AddStep("attach skin requester", () => gameplaySkin = addSkinRequester(player));

            AddStep("retrieve combo colours", () => colours = gameplaySkin.GetConfig<GlobalSkinConfiguration, IReadOnlyList<Color4>>(GlobalSkinConfiguration.ComboColours)?.Value);
            AddAssert("is default skin colours", () => colours.SequenceEqual(SkinConfiguration.DefaultComboColours));
        }

        [Test]
        public void TestEmptyBeatmapCustomSkinColours()
        {
            ExposedPlayer player = null;
            ISkin gameplaySkin = null;
            IReadOnlyList<Color4> colours = null;

            AddStep("load no-colour beatmap", () => player = loadBeatmap(true, false));
            AddUntilStep("wait for player", () => player.IsLoaded);
            AddStep("attach skin requester", () => gameplaySkin = addSkinRequester(player));

            AddStep("retrieve combo colours", () => colours = gameplaySkin.GetConfig<GlobalSkinConfiguration, IReadOnlyList<Color4>>(GlobalSkinConfiguration.ComboColours)?.Value);
            AddAssert("is custom skin colours", () => colours.SequenceEqual(TestSkin.Colours));
        }

        private ExposedPlayer loadBeatmap(bool skinHasCustomColours, bool beatmapHasCustomColours)
        {
            ExposedPlayer player;

            Beatmap.Value = new CustomSkinWorkingBeatmap(audio, beatmapHasCustomColours);
            Child = new SkinProvidingContainer(new TestSkin(skinHasCustomColours))
                .WithChild(new OsuScreenStack(player = new ExposedPlayer()) { RelativeSizeAxes = Axes.Both });

            return player;
        }

        private ISkin addSkinRequester(ExposedPlayer player)
        {
            SkinRequester skin;
            player.BeatmapSkinContainer.Add(skin = new SkinRequester());
            return skin;
        }

        private class SkinRequester : Component, ISkin
        {
            [Resolved]
            private ISkinSource skin { get; set; }

            public Drawable GetDrawableComponent(ISkinComponent component) => skin.GetDrawableComponent(component);

            public Texture GetTexture(string componentName) => skin.GetTexture(componentName);

            public SampleChannel GetSample(ISampleInfo sampleInfo) => skin.GetSample(sampleInfo);

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => skin.GetConfig<TLookup, TValue>(lookup);
        }

        private class ExposedPlayer : Player
        {
            public ExposedPlayer()
                : base(false, false)
            {
            }

            public BeatmapSkinProvidingContainer BeatmapSkinContainer => GameplayClockContainer.OfType<ScalingContainer>().First().OfType<BeatmapSkinProvidingContainer>().First();
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

        private class TestSkin : LegacySkin
        {
            public static Color4[] Colours { get; } =
            {
                new Color4(10, 100, 150, 255),
                new Color4(20, 20, 20, 255),
            };

            public TestSkin(bool hasCustomColours)
                : base(new SkinInfo(), null, null, string.Empty)
            {
                if (hasCustomColours)
                    Configuration.AddComboColours(Colours);
            }
        }
    }
}
