// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneHyperDashColouring : OsuTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; }

        [TestCase(false)]
        [TestCase(true)]
        public void TestHyperDashFruitColour(bool legacyFruit)
        {
            DrawableFruit drawableFruit = null;

            AddStep("setup fruit", () =>
            {
                var fruit = new Fruit { HyperDashTarget = new Banana() };
                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Child = setupSkinHierarchy(drawableFruit = new DrawableFruit(fruit)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(4f),
                }, false, false, false, legacyFruit);
            });

            AddAssert("default colour", () =>
                legacyFruit
                    ? checkLegacyFruitHyperDashColour(drawableFruit, Catcher.DefaultHyperDashColour)
                    : checkFruitHyperDashColour(drawableFruit, Catcher.DefaultHyperDashColour));
        }

        [TestCase(false, true)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(true, false)]
        public void TestCustomHyperDashFruitColour(bool legacyFruit, bool customCatcherHyperDashColour)
        {
            DrawableFruit drawableFruit = null;

            AddStep("setup fruit", () =>
            {
                var fruit = new Fruit { HyperDashTarget = new Banana() };
                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Child = setupSkinHierarchy(drawableFruit = new DrawableFruit(fruit)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(4f),
                }, customCatcherHyperDashColour, false, true, legacyFruit);
            });

            AddAssert("custom colour", () =>
                legacyFruit
                    ? checkLegacyFruitHyperDashColour(drawableFruit, TestSkin.CustomHyperDashFruitColour)
                    : checkFruitHyperDashColour(drawableFruit, TestSkin.CustomHyperDashFruitColour));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestCustomHyperDashFruitColourFallback(bool legacyFruit)
        {
            DrawableFruit drawableFruit = null;

            AddStep("setup fruit", () =>
            {
                var fruit = new Fruit { HyperDashTarget = new Banana() };
                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Child = setupSkinHierarchy(
                    drawableFruit = new DrawableFruit(fruit)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(4f),
                    }, true, false, false, legacyFruit);
            });

            AddAssert("catcher custom colour", () =>
                legacyFruit
                    ? checkLegacyFruitHyperDashColour(drawableFruit, TestSkin.CustomHyperDashColour)
                    : checkFruitHyperDashColour(drawableFruit, TestSkin.CustomHyperDashColour));
        }

        private Drawable setupSkinHierarchy(Drawable child, bool customCatcherColour = false, bool customAfterColour = false, bool customFruitColour = false, bool legacySkin = true)
        {
            var testSkinProvider = new SkinProvidingContainer(new TestSkin(customCatcherColour, customAfterColour, customFruitColour));

            if (legacySkin)
            {
                var legacySkinProvider = new SkinProvidingContainer(skins.GetSkin(DefaultLegacySkin.Info));
                var legacySkinTransformer = new SkinProvidingContainer(new CatchLegacySkinTransformer(testSkinProvider));

                return legacySkinProvider
                    .WithChild(testSkinProvider
                        .WithChild(legacySkinTransformer
                            .WithChild(child)));
            }

            return testSkinProvider.WithChild(child);
        }

        private bool checkFruitHyperDashColour(DrawableFruit fruit, Color4 expectedColour) =>
            fruit.ChildrenOfType<SkinnableDrawable>().First().Drawable.ChildrenOfType<Circle>().Single(c => c.BorderColour == expectedColour).Any(d => d.Colour == expectedColour);

        private bool checkLegacyFruitHyperDashColour(DrawableFruit fruit, Color4 expectedColour) =>
            fruit.ChildrenOfType<SkinnableDrawable>().First().Drawable.ChildrenOfType<Sprite>().Any(c => c.Colour == expectedColour);

        private class TestSkin : ISkin
        {
            public static Color4 CustomHyperDashColour { get; } = Color4.Goldenrod;
            public static Color4 CustomHyperDashFruitColour { get; } = Color4.Cyan;
            public static Color4 CustomHyperDashAfterColour { get; } = Color4.Lime;

            private readonly bool customCatcherColour;
            private readonly bool customAfterColour;
            private readonly bool customFruitColour;

            public TestSkin(bool customCatcherColour = false, bool customAfterColour = false, bool customFruitColour = false)
            {
                this.customCatcherColour = customCatcherColour;
                this.customAfterColour = customAfterColour;
                this.customFruitColour = customFruitColour;
            }

            public Drawable GetDrawableComponent(ISkinComponent component) => null;

            public Texture GetTexture(string componentName) => null;

            public SampleChannel GetSample(ISampleInfo sampleInfo) => null;

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
            {
                if (lookup is CatchSkinConfiguration config)
                {
                    if (config == CatchSkinConfiguration.HyperDash && customCatcherColour)
                        return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashColour));

                    if (config == CatchSkinConfiguration.HyperDashFruit && customFruitColour)
                        return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashFruitColour));

                    if (config == CatchSkinConfiguration.HyperDashAfterImage && customAfterColour)
                        return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashAfterColour));
                }

                return null;
            }
        }
    }
}
