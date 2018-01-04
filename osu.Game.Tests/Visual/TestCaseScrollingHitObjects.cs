// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Tests.Visual
{
    public class TestCaseScrollingHitObjects : OsuTestCase
    {
        public TestCaseScrollingHitObjects()
        {
            AddStep("Vertically-scrolling", () => createPlayfield(Direction.Vertical));
            AddStep("Horizontally-scrolling", () => createPlayfield(Direction.Horizontal));

            AddStep("Add hitobject", addHitObject);
        }

        private TestPlayfield playfield;
        private void createPlayfield(Direction scrollingDirection)
        {
            if (playfield != null)
                Remove(playfield);
            Add(playfield = new TestPlayfield(scrollingDirection));
        }

        private void addHitObject()
        {
            playfield.Add(new TestDrawableHitObject(new HitObject { StartTime = Time.Current + 5000 })
            {
                Anchor = playfield.ScrollingDirection == Direction.Horizontal ? Anchor.CentreRight : Anchor.BottomCentre
            });
        }

        private class ScrollingHitObjectContainer : Playfield.HitObjectContainer
        {
            public double TimeRange = 5000;

            private readonly Direction scrollingDirection;

            public ScrollingHitObjectContainer(Direction scrollingDirection)
            {
                this.scrollingDirection = scrollingDirection;

                RelativeSizeAxes = Axes.Both;
            }

            protected override void Update()
            {
                base.Update();

                foreach (var obj in Objects)
                {
                    var relativePosition = (Time.Current - obj.HitObject.StartTime) / TimeRange;

                    // Todo: We may need to consider scale here
                    var finalPosition = (float)relativePosition * DrawSize;

                    switch (scrollingDirection)
                    {
                        case Direction.Horizontal:
                            obj.X = finalPosition.X;
                            break;
                        case Direction.Vertical:
                            obj.Y = finalPosition.Y;
                            break;
                    }
                }
            }
        }

        private class TestPlayfield : Playfield
        {
            public readonly Direction ScrollingDirection;

            public TestPlayfield(Direction scrollingDirection)
            {
                ScrollingDirection = scrollingDirection;

                HitObjects = new ScrollingHitObjectContainer(scrollingDirection);
            }
        }

        private class TestDrawableHitObject : DrawableHitObject<HitObject>
        {
            public TestDrawableHitObject(HitObject hitObject)
                : base(hitObject)
            {
                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;

                Add(new Box { Size = new Vector2(75) });
            }

            protected override void UpdateState(ArmedState state)
            {
            }
        }
    }
}
