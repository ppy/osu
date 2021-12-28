// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneFruitObjects : CatchSkinnableTestScene
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("show pear", () => SetContents(_ => createDrawableFruit(0)));
            AddStep("show grape", () => SetContents(_ => createDrawableFruit(1)));
            AddStep("show pineapple / apple", () => SetContents(_ => createDrawableFruit(2)));
            AddStep("show raspberry / orange", () => SetContents(_ => createDrawableFruit(3)));

            AddStep("show banana", () => SetContents(_ => createDrawableBanana()));

            AddStep("show droplet", () => SetContents(_ => createDrawableDroplet()));
            AddStep("show tiny droplet", () => SetContents(_ => createDrawableTinyDroplet()));

            AddStep("show hyperdash pear", () => SetContents(_ => createDrawableFruit(0, true)));
            AddStep("show hyperdash grape", () => SetContents(_ => createDrawableFruit(1, true)));
            AddStep("show hyperdash pineapple / apple", () => SetContents(_ => createDrawableFruit(2, true)));
            AddStep("show hyperdash raspberry / orange", () => SetContents(_ => createDrawableFruit(3, true)));

            AddStep("show hyperdash droplet", () => SetContents(_ => createDrawableDroplet(true)));
        }

        private Drawable createDrawableFruit(int indexInBeatmap, bool hyperDash = false) =>
            new TestDrawableCatchHitObjectSpecimen(new DrawableFruit(new Fruit
            {
                IndexInBeatmap = indexInBeatmap,
                HyperDashBindable = { Value = hyperDash }
            }));

        private Drawable createDrawableBanana() =>
            new TestDrawableCatchHitObjectSpecimen(new DrawableBanana(new Banana()));

        private Drawable createDrawableDroplet(bool hyperDash = false) =>
            new TestDrawableCatchHitObjectSpecimen(new DrawableDroplet(new Droplet
            {
                HyperDashBindable = { Value = hyperDash }
            }));

        private Drawable createDrawableTinyDroplet() => new TestDrawableCatchHitObjectSpecimen(new DrawableTinyDroplet(new TinyDroplet()));
    }

    public class TestDrawableCatchHitObjectSpecimen : CompositeDrawable
    {
        public readonly ManualClock ManualClock;

        public TestDrawableCatchHitObjectSpecimen(DrawableCatchHitObject d)
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            ManualClock = new ManualClock();
            Clock = new FramedClock(ManualClock);

            var hitObject = d.HitObject;
            hitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            hitObject.Scale = 1.5f;
            hitObject.StartTime = 500;

            d.Anchor = Anchor.Centre;
            d.HitObjectApplied += _ =>
            {
                d.LifetimeStart = double.NegativeInfinity;
                d.LifetimeEnd = double.PositiveInfinity;
            };

            InternalChild = d;
        }
    }
}
