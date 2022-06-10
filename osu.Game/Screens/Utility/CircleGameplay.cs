// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable
using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Utility
{
    public class CircleGameplay : CompositeDrawable
    {
        private int nextLocation;

        private OsuSpriteText unstableRate = null!;

        private readonly List<HitEvent> hitEvents = new List<HitEvent>();

        private double? lastGeneratedBeatTime;

        private static readonly BindableDouble beat_length = new BindableDouble(500) { MinValue = 200, MaxValue = 1000 };
        private static readonly BindableDouble approach_rate_milliseconds = new BindableDouble(100) { MinValue = 50, MaxValue = 500 };
        private static readonly BindableFloat spacing = new BindableFloat(0.2f) { MinValue = 0.05f, MaxValue = 0.4f };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Width = 400,
                    Spacing = new Vector2(2),
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new SettingsSlider<double>
                        {
                            LabelText = "time spacing",
                            Current = beat_length
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = "visual spacing",
                            Current = spacing
                        },
                        new SettingsSlider<double>
                        {
                            LabelText = "approach time",
                            Current = approach_rate_milliseconds
                        },
                    }
                },
                unstableRate = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Default.With(size: 24),
                    Y = -100,
                },
            };
        }

        protected override void Update()
        {
            base.Update();

            // We want to generate a few hit objects ahead of the current time (to allow them to animate).

            int nextBeat = (int)(Clock.CurrentTime / beat_length.Value) + 1;

            double generateUpTo = (nextBeat + 2) * beat_length.Value;

            while (lastGeneratedBeatTime == null || lastGeneratedBeatTime < generateUpTo)
            {
                double time = ++nextBeat * beat_length.Value;

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

            float spacingLow = 0.5f - spacing.Value;
            float spacingHigh = 0.5f + spacing.Value;

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

            AddInternal(new SampleHitCircle(time)
            {
                RelativePositionAxes = Axes.Both,
                Position = location,
                Hit = hit,
            });
        }

        private void hit(HitEvent h)
        {
            hitEvents.Add(h);
            unstableRate.Text = $"{hitEvents.CalculateUnstableRate():N1}";
        }

        public class SampleHitCircle : CompositeDrawable
        {
            public HitEvent? HitEvent;

            public Action<HitEvent>? Hit { get; set; }

            public readonly double HitTime;

            private readonly CircularContainer approach;
            private readonly Circle circle;

            private const float size = 100;

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
                => circle.ReceivePositionalInputAt(screenSpacePos);

            public SampleHitCircle(double hitTime)
            {
                HitTime = hitTime;

                Origin = Anchor.Centre;

                AutoSizeAxes = Axes.Both;

                AlwaysPresent = true;

                InternalChildren = new Drawable[]
                {
                    circle = new Circle
                    {
                        Colour = Color4.White,
                        Size = new Vector2(size),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    approach = new CircularContainer
                    {
                        BorderColour = Color4.Yellow,
                        Size = new Vector2(size),
                        Masking = true,
                        BorderThickness = 4,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = Color4.Black,
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

                approach.Expire();

                circle
                    .FadeOut(200)
                    .ScaleTo(1.5f, 200);

                HitEvent = new HitEvent(Clock.CurrentTime - HitTime, HitResult.Good, new HitObject
                {
                    HitWindows = new HitWindows(),
                }, null, null);

                Hit?.Invoke(HitEvent.Value);

                this.Delay(200).Expire();

                return true;
            }

            protected override void Update()
            {
                base.Update();

                approach.Scale = new Vector2(1 + (float)MathHelper.Clamp((HitTime - Clock.CurrentTime) / approach_rate_milliseconds.Value, 0, 100));
                Alpha = (float)MathHelper.Clamp((Clock.CurrentTime - HitTime + 600) / 400, 0, 1);

                if (Clock.CurrentTime > HitTime + 200)
                    Expire();
            }
        }
    }
}
