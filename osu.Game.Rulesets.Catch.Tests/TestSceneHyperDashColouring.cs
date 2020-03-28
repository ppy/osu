// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
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
        [TestCase(false)]
        [TestCase(true)]
        public void TestHyperDashFruitColour(bool legacyFruit)
        {
            DrawableFruit drawableFruit = null;

            AddStep("setup fruit", () =>
            {
                var fruit = new Fruit { IndexInBeatmap = legacyFruit ? 0 : 1, HyperDashTarget = new Banana() };
                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Child = setupSkinHierarchy(() =>
                    drawableFruit = new DrawableFruit(fruit)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(4f),
                    }, false, false);
            });

            AddAssert("default colour", () => checkFruitHyperDashColour(drawableFruit, Catcher.DefaultHyperDashColour, legacyFruit));
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
                var fruit = new Fruit { IndexInBeatmap = legacyFruit ? 0 : 1, HyperDashTarget = new Banana() };
                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Child = setupSkinHierarchy(() =>
                    drawableFruit = new DrawableFruit(fruit)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(4f),
                    }, customCatcherHyperDashColour, true);
            });

            AddAssert("custom colour", () => checkFruitHyperDashColour(drawableFruit, TestLegacySkin.CustomHyperDashFruitColour, legacyFruit));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestCustomHyperDashFruitColourFallback(bool legacyFruit)
        {
            DrawableFruit drawableFruit = null;

            AddStep("setup fruit", () =>
            {
                var fruit = new Fruit { IndexInBeatmap = legacyFruit ? 0 : 1, HyperDashTarget = new Banana() };
                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Child = setupSkinHierarchy(() =>
                    drawableFruit = new DrawableFruit(fruit)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(4f),
                    }, true, false);
            });

            AddAssert("catcher custom colour", () => checkFruitHyperDashColour(drawableFruit, TestLegacySkin.CustomHyperDashColour, legacyFruit));
        }

        private Drawable setupSkinHierarchy(Func<Drawable> getChild, bool customHyperDashCatcherColour = false, bool customHyperDashFruitColour = false, bool customHyperDashAfterColour = false)
        {
            var testSkinProvider = new SkinProvidingContainer(new TestLegacySkin(customHyperDashCatcherColour, customHyperDashFruitColour, customHyperDashAfterColour));

            var legacySkinTransformer = new SkinProvidingContainer(new CatchLegacySkinTransformer(testSkinProvider));

            return testSkinProvider
                .WithChild(legacySkinTransformer
                    .WithChild(getChild.Invoke()));
        }

        private bool checkFruitHyperDashColour(DrawableFruit fruit, Color4 expectedColour, bool isLegacyFruit) =>
            isLegacyFruit
                ? fruit.ChildrenOfType<SkinnableDrawable>().First().Drawable.ChildrenOfType<Sprite>().Any(c => c.Colour == expectedColour)
                : fruit.ChildrenOfType<SkinnableDrawable>().First().Drawable.ChildrenOfType<Circle>().Single(c => c.BorderColour == expectedColour).Any(d => d.Colour == expectedColour);

        private class TestLegacySkin : ISkin
        {
            public static Color4 CustomHyperDashColour { get; } = Color4.Goldenrod;
            public static Color4 CustomHyperDashFruitColour { get; } = Color4.Cyan;
            public static Color4 CustomHyperDashAfterColour { get; } = Color4.Lime;

            private readonly bool customHyperDashCatcherColour;
            private readonly bool customHyperDashFruitColour;
            private readonly bool customHyperDashAfterColour;

            public TestLegacySkin(bool customHyperDashCatcherColour = false, bool customHyperDashFruitColour = false, bool customHyperDashAfterColour = false)
            {
                this.customHyperDashCatcherColour = customHyperDashCatcherColour;
                this.customHyperDashFruitColour = customHyperDashFruitColour;
                this.customHyperDashAfterColour = customHyperDashAfterColour;
            }

            public Drawable GetDrawableComponent(ISkinComponent component) => null;

            public Texture GetTexture(string componentName)
            {
                if (componentName == "fruit-pear")
                {
                    // convince CatchLegacySkinTransformer to use the LegacyFruitPiece for pear fruit.
                    return new Texture(Texture.WhitePixel.TextureGL)
                    {
                        Width = 1,
                        Height = 1,
                        ScaleAdjust = 1 / 96f
                    };
                }

                return null;
            }

            public SampleChannel GetSample(ISampleInfo sampleInfo) => null;

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
            {
                if (lookup is CatchSkinConfiguration config)
                {
                    if (config == CatchSkinConfiguration.HyperDash && customHyperDashCatcherColour)
                        return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashColour));

                    if (config == CatchSkinConfiguration.HyperDashFruit && customHyperDashFruitColour)
                        return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashFruitColour));

                    if (config == CatchSkinConfiguration.HyperDashAfterImage && customHyperDashAfterColour)
                        return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashAfterColour));
                }

                return null;
            }
        }
    }
}
