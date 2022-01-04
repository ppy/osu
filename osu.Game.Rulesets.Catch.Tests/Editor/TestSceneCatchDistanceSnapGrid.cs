// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Edit;
using osu.Game.Rulesets.Catch.Edit.Blueprints.Components;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public class TestSceneCatchDistanceSnapGrid : OsuManualInputManagerTestScene
    {
        private readonly ManualClock manualClock = new ManualClock();

        [Cached(typeof(Playfield))]
        private readonly CatchPlayfield playfield;

        private ScrollingHitObjectContainer hitObjectContainer => playfield.HitObjectContainer;

        private readonly CatchDistanceSnapGrid distanceGrid;

        private readonly FruitOutline fruitOutline;

        private readonly Fruit fruit = new Fruit();

        public TestSceneCatchDistanceSnapGrid()
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Width = 500,

                Children = new Drawable[]
                {
                    new ScrollingTestContainer(ScrollingDirection.Down)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = playfield = new CatchPlayfield(new BeatmapDifficulty())
                        {
                            RelativeSizeAxes = Axes.Both,
                            Clock = new FramedClock(manualClock)
                        }
                    },
                    distanceGrid = new CatchDistanceSnapGrid(new double[] { 0, -1, 1 }),
                    fruitOutline = new FruitOutline()
                },
            };
        }

        protected override void Update()
        {
            base.Update();

            distanceGrid.StartTime = 100;
            distanceGrid.StartX = 250;

            Vector2 screenSpacePosition = InputManager.CurrentState.Mouse.Position;

            var result = distanceGrid.GetSnappedPosition(screenSpacePosition);

            if (result != null)
            {
                fruit.OriginalX = hitObjectContainer.ToLocalSpace(result.ScreenSpacePosition).X;

                if (result.Time != null)
                    fruit.StartTime = result.Time.Value;
            }

            fruitOutline.Position = CatchHitObjectUtils.GetStartPosition(hitObjectContainer, fruit);
            fruitOutline.UpdateFrom(fruit);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            manualClock.CurrentTime -= e.ScrollDelta.Y * 50;
            return true;
        }
    }
}
