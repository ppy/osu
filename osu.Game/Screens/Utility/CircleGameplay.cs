// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Utility.SampleComponents;
using osuTK;

namespace osu.Game.Screens.Utility
{
    public partial class CircleGameplay : LatencySampleComponent
    {
        private int nextLocation;

        private readonly List<HitEvent> hitEvents = new List<HitEvent>();

        private double? lastGeneratedBeatTime;

        private Container circles = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new Drawable[]
            {
                circles = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };

            SampleBPM.BindValueChanged(_ =>
            {
                circles.Clear();
                lastGeneratedBeatTime = null;
            });
        }

        protected override void UpdateAtLimitedRate(InputState inputState)
        {
            double beatLength = 60000 / SampleBPM.Value;

            int nextBeat = (int)(Clock.CurrentTime / beatLength) + 1;

            // We want to generate a few hit objects ahead of the current time (to allow them to animate).
            double generateUpTo = (nextBeat + 2) * beatLength;

            while (lastGeneratedBeatTime == null || lastGeneratedBeatTime < generateUpTo)
            {
                double time = ++nextBeat * beatLength;

                if (time <= lastGeneratedBeatTime)
                    continue;

                newBeat(time);
                lastGeneratedBeatTime = time;
            }
        }

        private void newBeat(double time)
        {
            nextLocation++;

            Vector2 location;

            float adjust = SampleVisualSpacing.Value * 0.25f;

            float spacingLow = 0.5f - adjust;
            float spacingHigh = 0.5f + adjust;

            switch (nextLocation % 4)
            {
                default:
                    location = new Vector2(spacingLow, spacingLow);
                    break;

                case 1:
                    location = new Vector2(spacingHigh, spacingHigh);
                    break;

                case 2:
                    location = new Vector2(spacingHigh, spacingLow);
                    break;

                case 3:
                    location = new Vector2(spacingLow, spacingHigh);
                    break;
            }

            circles.Add(new SampleHitCircle(time)
            {
                RelativePositionAxes = Axes.Both,
                Position = location,
                Hit = hit,
            });
        }

        private void hit(HitEvent h)
        {
            hitEvents.Add(h);
        }

        public partial class SampleHitCircle : LatencySampleComponent
        {
            public HitEvent? HitEvent;

            public Action<HitEvent>? Hit { get; set; }

            public readonly double HitTime;

            private CircularContainer approach = null!;
            private Circle circle = null!;

            private const float size = 100;
            private const float duration = 200;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
                => circle.ReceivePositionalInputAt(screenSpacePos);

            public SampleHitCircle(double hitTime)
            {
                HitTime = hitTime;

                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;
                AlwaysPresent = true;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChildren = new Drawable[]
                {
                    circle = new Circle
                    {
                        Colour = OverlayColourProvider.Content1,
                        Size = new Vector2(size),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    approach = new CircularContainer
                    {
                        BorderColour = colours.Blue,
                        Size = new Vector2(size),
                        Masking = true,
                        BorderThickness = 4,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Alpha = 0,
                                AlwaysPresent = true,
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    },
                };
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (HitEvent != null)
                    return false;

                if (Math.Abs(Clock.CurrentTime - HitTime) > duration)
                    return false;

                attemptHit();
                return true;
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (!IsActive.Value)
                    return false;

                if (Math.Abs(Clock.CurrentTime - HitTime) > duration)
                    return false;

                if (IsHovered)
                    attemptHit();
                return base.OnKeyDown(e);
            }

            protected override void UpdateAtLimitedRate(InputState inputState)
            {
                if (HitEvent == null)
                {
                    double preempt = (float)IBeatmapDifficultyInfo.DifficultyRange(SampleApproachRate.Value, 1800, 1200, 450);

                    approach.Scale = new Vector2(1 + 4 * (float)MathHelper.Clamp((HitTime - Clock.CurrentTime) / preempt, 0, 100));
                    Alpha = (float)MathHelper.Clamp((Clock.CurrentTime - HitTime + 600) / 400, 0, 1);

                    if (Clock.CurrentTime > HitTime + duration)
                        Expire();
                }
            }

            private void attemptHit() => Schedule(() =>
            {
                if (HitEvent != null)
                    return;

                // in case it was hit outside of display range, show immediately
                // so the user isn't confused.
                this.FadeIn();

                approach.Expire();

                circle
                    .FadeOut(duration)
                    .ScaleTo(1.5f, duration);

                HitEvent = new HitEvent(Clock.CurrentTime - HitTime, 1.0, HitResult.Good, new HitObject
                {
                    HitWindows = new HitWindows(),
                }, null, null);

                Hit?.Invoke(HitEvent.Value);

                this.Delay(duration).Expire();
            });
        }
    }
}
