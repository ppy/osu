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

            AddAssert("hyper-dash fruit has default colour", () => checkLegacyFruitHyperDashColour(drawableFruit, Catcher.DEFAULT_HYPER_DASH_COLOUR));
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

            AddAssert("hyper-dash fruit use fruit colour from skin", () => checkLegacyFruitHyperDashColour(drawableFruit, TestSkin.CUSTOM_HYPER_DASH_FRUIT_COLOUR));
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

            AddAssert("hyper-dash fruit colour falls back to catcher colour from skin", () => checkLegacyFruitHyperDashColour(drawableFruit, TestSkin.CUSTOM_HYPER_DASH_COLOUR));
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

        private class TestSkin : LegacySkin
        {
            public static readonly Color4 CUSTOM_HYPER_DASH_COLOUR = Color4.Goldenrod;
            public static readonly Color4 CUSTOM_HYPER_DASH_AFTER_COLOUR = Color4.Lime;
            public static readonly Color4 CUSTOM_HYPER_DASH_FRUIT_COLOUR = Color4.Cyan;

            public TestSkin(bool customCatcherColour, bool customAfterColour, bool customFruitColour)
                : base(new SkinInfo(), null, null, string.Empty)
            {
                if (customCatcherColour)
                    Configuration.CustomColours[CatchSkinColour.HyperDash.ToString()] = CUSTOM_HYPER_DASH_COLOUR;

                if (customAfterColour)
                    Configuration.CustomColours[CatchSkinColour.HyperDashAfterImage.ToString()] = CUSTOM_HYPER_DASH_AFTER_COLOUR;

                if (customFruitColour)
                    Configuration.CustomColours[CatchSkinColour.HyperDashFruit.ToString()] = CUSTOM_HYPER_DASH_FRUIT_COLOUR;
            }
        }
    }
}
