// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
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

        [Test]
        public void TestHyperDashFruitColour()
        {
            DrawableFruit drawableFruit = null;

            AddStep("setup hyper-dash fruit", () =>
            {
                var fruit = new Fruit { HyperDashTarget = new Banana() };
                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Child = setupSkinHierarchy(drawableFruit = new DrawableFruit(fruit)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(4f),
                }, false, false, false);
            });

            AddAssert("hyper-dash fruit has default colour", () => checkLegacyFruitHyperDashColour(drawableFruit, Catcher.DefaultHyperDashColour));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestCustomHyperDashFruitColour(bool customCatcherHyperDashColour)
        {
            DrawableFruit drawableFruit = null;

            AddStep("setup hyper-dash fruit", () =>
            {
                var fruit = new Fruit { HyperDashTarget = new Banana() };
                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Child = setupSkinHierarchy(drawableFruit = new DrawableFruit(fruit)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(4f),
                }, customCatcherHyperDashColour, false, true);
            });

            AddAssert("hyper-dash fruit use fruit colour from skin", () => checkLegacyFruitHyperDashColour(drawableFruit, TestSkin.CustomHyperDashFruitColour));
        }

        [Test]
        public void TestCustomHyperDashFruitColourFallback()
        {
            DrawableFruit drawableFruit = null;

            AddStep("setup hyper-dash fruit", () =>
            {
                var fruit = new Fruit { HyperDashTarget = new Banana() };
                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Child = setupSkinHierarchy(
                    drawableFruit = new DrawableFruit(fruit)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(4f),
                    }, true, false, false);
            });

            AddAssert("hyper-dash fruit colour falls back to catcher colour from skin", () => checkLegacyFruitHyperDashColour(drawableFruit, TestSkin.CustomHyperDashColour));
        }

        private Drawable setupSkinHierarchy(Drawable child, bool customCatcherColour, bool customAfterColour, bool customFruitColour)
        {
            var legacySkinProvider = new SkinProvidingContainer(skins.GetSkin(DefaultLegacySkin.Info));
            var testSkinProvider = new SkinProvidingContainer(new TestSkin(customCatcherColour, customAfterColour, customFruitColour));
            var legacySkinTransformer = new SkinProvidingContainer(new CatchLegacySkinTransformer(testSkinProvider));

            return legacySkinProvider
                .WithChild(testSkinProvider
                    .WithChild(legacySkinTransformer
                        .WithChild(child)));
        }

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

            public TestSkin(bool customCatcherColour, bool customAfterColour, bool customFruitColour)
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
                if (lookup is CatchSkinColour config)
                {
                    if (config == CatchSkinColour.HyperDash && customCatcherColour)
                        return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashColour));

                    if (config == CatchSkinColour.HyperDashFruit && customFruitColour)
                        return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashFruitColour));

                    if (config == CatchSkinColour.HyperDashAfterImage && customAfterColour)
                        return SkinUtils.As<TValue>(new Bindable<Color4>(CustomHyperDashAfterColour));
                }

                return null;
            }
        }
    }
}
