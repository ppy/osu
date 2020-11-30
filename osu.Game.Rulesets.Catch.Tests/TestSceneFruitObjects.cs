// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneFruitObjects : CatchSkinnableTestScene
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("show pear", () => SetContents(() => createDrawableFruit(0)));
            AddStep("show grape", () => SetContents(() => createDrawableFruit(1)));
            AddStep("show pineapple / apple", () => SetContents(() => createDrawableFruit(2)));
            AddStep("show raspberry / orange", () => SetContents(() => createDrawableFruit(3)));

            AddStep("show banana", () => SetContents(createDrawableBanana));

            AddStep("show droplet", () => SetContents(() => createDrawableDroplet()));
            AddStep("show tiny droplet", () => SetContents(createDrawableTinyDroplet));

            AddStep("show hyperdash pear", () => SetContents(() => createDrawableFruit(0, true)));
            AddStep("show hyperdash grape", () => SetContents(() => createDrawableFruit(1, true)));
            AddStep("show hyperdash pineapple / apple", () => SetContents(() => createDrawableFruit(2, true)));
            AddStep("show hyperdash raspberry / orange", () => SetContents(() => createDrawableFruit(3, true)));

            AddStep("show hyperdash droplet", () => SetContents(() => createDrawableDroplet(true)));
        }

        private Drawable createDrawableFruit(int indexInBeatmap, bool hyperdash = false) =>
            SetProperties(new DrawableFruit(new Fruit
            {
                IndexInBeatmap = indexInBeatmap,
                HyperDashBindable = { Value = hyperdash }
            }));

        private Drawable createDrawableBanana() =>
            SetProperties(new DrawableBanana(new Banana()));

        private Drawable createDrawableDroplet(bool hyperdash = false) =>
            SetProperties(new DrawableDroplet(new Droplet
            {
                HyperDashBindable = { Value = hyperdash }
            }));

        private Drawable createDrawableTinyDroplet() => SetProperties(new DrawableTinyDroplet(new TinyDroplet()));

        protected virtual DrawableCatchHitObject SetProperties(DrawableCatchHitObject d)
        {
            var hitObject = d.HitObject;
            hitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 0 });
            hitObject.StartTime = 1000000000000;
            hitObject.Scale = 1.5f;

            d.Anchor = Anchor.Centre;
            d.RelativePositionAxes = Axes.None;
            d.Position = Vector2.Zero;
            d.HitObjectApplied += _ =>
            {
                d.LifetimeStart = double.NegativeInfinity;
                d.LifetimeEnd = double.PositiveInfinity;
            };
            return d;
        }
    }
}
