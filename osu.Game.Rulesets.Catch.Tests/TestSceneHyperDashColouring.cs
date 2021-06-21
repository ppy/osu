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
using osu.Game.Rulesets.Catch.Skinning.Legacy;
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
        public void TestDefaultCatcherColour()
        {
            var skin = new TestSkin();

            checkHyperDashCatcherColour(skin, Catcher.DEFAULT_HYPER_DASH_COLOUR);
        }

        [Test]
        public void TestCustomCatcherColour()
        {
            var skin = new TestSkin
            {
                HyperDashColour = Color4.Goldenrod
            };

            checkHyperDashCatcherColour(skin, skin.HyperDashColour);
        }

        [Test]
        public void TestCustomEndGlowColour()
        {
            var skin = new TestSkin
            {
                HyperDashAfterImageColour = Color4.Lime
            };

            checkHyperDashCatcherColour(skin, Catcher.DEFAULT_HYPER_DASH_COLOUR, skin.HyperDashAfterImageColour);
        }

        [Test]
        public void TestCustomEndGlowColourPriority()
        {
            var skin = new TestSkin
            {
                HyperDashColour = Color4.Goldenrod,
                HyperDashAfterImageColour = Color4.Lime
            };

            checkHyperDashCatcherColour(skin, skin.HyperDashColour, skin.HyperDashAfterImageColour);
        }

        [Test]
        public void TestDefaultFruitColour()
        {
            var skin = new TestSkin();

            checkHyperDashFruitColour(skin, Catcher.DEFAULT_HYPER_DASH_COLOUR);
        }

        [Test]
        public void TestCustomFruitColour()
        {
            var skin = new TestSkin
            {
                HyperDashFruitColour = Color4.Cyan
            };

            checkHyperDashFruitColour(skin, skin.HyperDashFruitColour);
        }

        [Test]
        public void TestCustomFruitColourPriority()
        {
            var skin = new TestSkin
            {
                HyperDashColour = Color4.Goldenrod,
                HyperDashFruitColour = Color4.Cyan
            };

            checkHyperDashFruitColour(skin, skin.HyperDashFruitColour);
        }

        [Test]
        public void TestFruitColourFallback()
        {
            var skin = new TestSkin
            {
                HyperDashColour = Color4.Goldenrod
            };

            checkHyperDashFruitColour(skin, skin.HyperDashColour);
        }

        private void checkHyperDashCatcherColour(ISkin skin, Color4 expectedCatcherColour, Color4? expectedEndGlowColour = null)
        {
            CatcherArea catcherArea = null;
            CatcherTrailDisplay trails = null;

            AddStep("create hyper-dashing catcher", () =>
            {
                Child = setupSkinHierarchy(catcherArea = new TestCatcherArea
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }, skin);
            });

            AddStep("get trails container", () =>
            {
                trails = catcherArea.OfType<CatcherTrailDisplay>().Single();
                catcherArea.MovableCatcher.SetHyperDashState(2);
            });

            AddUntilStep("catcher colour is correct", () => catcherArea.MovableCatcher.Colour == expectedCatcherColour);

            AddAssert("catcher trails colours are correct", () => trails.HyperDashTrailsColour == expectedCatcherColour);
            AddAssert("catcher end-glow colours are correct", () => trails.EndGlowSpritesColour == (expectedEndGlowColour ?? expectedCatcherColour));

            AddStep("finish hyper-dashing", () =>
            {
                catcherArea.MovableCatcher.SetHyperDashState(1);
                catcherArea.MovableCatcher.FinishTransforms();
            });

            AddAssert("catcher colour returned to white", () => catcherArea.MovableCatcher.Colour == Color4.White);
        }

        private void checkHyperDashFruitColour(ISkin skin, Color4 expectedColour)
        {
            DrawableFruit drawableFruit = null;

            AddStep("create hyper-dash fruit", () =>
            {
                var fruit = new Fruit { HyperDashTarget = new Banana() };
                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Child = setupSkinHierarchy(drawableFruit = new DrawableFruit(fruit)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(4f),
                }, skin);
            });

            AddAssert("hyper-dash colour is correct", () => checkLegacyFruitHyperDashColour(drawableFruit, expectedColour));
        }

        private Drawable setupSkinHierarchy(Drawable child, ISkin skin)
        {
            var legacySkinProvider = new SkinProvidingContainer(skins.GetSkin(DefaultLegacySkin.Info));
            var testSkinProvider = new SkinProvidingContainer(skin);
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
            public Color4 HyperDashColour
            {
                get => Configuration.CustomColours[CatchSkinColour.HyperDash.ToString()];
                set => Configuration.CustomColours[CatchSkinColour.HyperDash.ToString()] = value;
            }

            public Color4 HyperDashAfterImageColour
            {
                get => Configuration.CustomColours[CatchSkinColour.HyperDashAfterImage.ToString()];
                set => Configuration.CustomColours[CatchSkinColour.HyperDashAfterImage.ToString()] = value;
            }

            public Color4 HyperDashFruitColour
            {
                get => Configuration.CustomColours[CatchSkinColour.HyperDashFruit.ToString()];
                set => Configuration.CustomColours[CatchSkinColour.HyperDashFruit.ToString()] = value;
            }

            public TestSkin()
                : base(new SkinInfo(), null, null, string.Empty)
            {
            }
        }

        private class TestCatcherArea : CatcherArea
        {
            [Cached]
            private readonly DroppedObjectContainer droppedObjectContainer;

            public TestCatcherArea()
            {
                Scale = new Vector2(4f);

                AddInternal(droppedObjectContainer = new DroppedObjectContainer());
            }
        }
    }
}
