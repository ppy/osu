// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using Direction = osu.Game.Rulesets.Catch.UI.Direction;

namespace osu.Game.Rulesets.Catch.Tests
{
    public partial class TestSceneCatchSkinConfiguration : OsuTestScene
    {
        private Catcher catcher;

        private readonly Container container;

        public TestSceneCatchSkinConfiguration()
        {
            Add(container = new Container { RelativeSizeAxes = Axes.Both });
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestCatcherPlateFlipping(bool flip)
        {
            AddStep("setup catcher", () =>
            {
                var skin = new TestSkin { FlipCatcherPlate = flip };
                container.Child = new SkinProvidingContainer(skin)
                {
                    Child = catcher = new Catcher(new DroppedObjectContainer())
                    {
                        Anchor = Anchor.Centre
                    }
                };
            });

            Fruit fruit = new Fruit();

            AddStep("catch fruit", () => catchFruit(fruit, 20));

            float position = 0;

            AddStep("record fruit position", () => position = getCaughtObjectPosition(fruit));

            AddStep("face left", () => catcher.VisualDirection = Direction.Left);

            if (flip)
                AddAssert("fruit position changed", () => !Precision.AlmostEquals(getCaughtObjectPosition(fruit), position));
            else
                AddAssert("fruit position unchanged", () => Precision.AlmostEquals(getCaughtObjectPosition(fruit), position));

            AddStep("face right", () => catcher.VisualDirection = Direction.Right);

            AddAssert("fruit position restored", () => Precision.AlmostEquals(getCaughtObjectPosition(fruit), position));
        }

        private float getCaughtObjectPosition(Fruit fruit)
        {
            var caughtObject = catcher.ChildrenOfType<CaughtObject>().Single(c => c.HitObject == fruit);
            return caughtObject.Parent!.ToSpaceOfOtherDrawable(caughtObject.Position, catcher).X;
        }

        private void catchFruit(Fruit fruit, float x)
        {
            fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            var drawableFruit = new DrawableFruit(fruit) { X = x };
            var judgement = fruit.Judgement;
            catcher.OnNewResult(drawableFruit, new CatchJudgementResult(fruit, judgement)
            {
                Type = judgement.MaxResult
            });
        }

        private class TestSkin : TrianglesSkin
        {
            public bool FlipCatcherPlate { get; set; }

            public TestSkin()
                : base(null!)
            {
            }

            public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
            {
                if (lookup is CatchSkinConfiguration config)
                {
                    if (config == CatchSkinConfiguration.FlipCatcherPlate)
                        return SkinUtils.As<TValue>(new Bindable<bool>(FlipCatcherPlate));
                }

                return base.GetConfig<TLookup, TValue>(lookup);
            }
        }
    }
}
