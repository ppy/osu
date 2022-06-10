// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
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

        private int? lastGeneratedBeat;

        private const double beat_length = 500;
        private const double approach_rate_milliseconds = 100;
        private const float spacing = 0.1f;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new Drawable[]
            {
                unstableRate = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Default.With(size: 24)
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            int nextBeat = (int)(Clock.CurrentTime / beat_length);

            if (lastGeneratedBeat == null || nextBeat != lastGeneratedBeat)
            {
                // generate four beats ahead to allow time for beats to display.
                newBeat(nextBeat + 4);
                lastGeneratedBeat = nextBeat;
            }
        }

        private void newBeat(int index)
        {
            nextLocation++;

            Vector2 location;

            const float spacing_low = 0.5f - spacing;
            const float spacing_high = 0.5f + spacing;

            switch (nextLocation % 4)
            {
                default:
                    location = new Vector2(spacing_low, spacing_low);
                    break;

                case 1:
                    location = new Vector2(spacing_high, spacing_high);
                    break;

                case 2:
                    location = new Vector2(spacing_high, spacing_low);
                    break;

                case 3:
                    location = new Vector2(spacing_low, spacing_high);
                    break;
            }

            AddInternal(new SampleHitCircle(index * beat_length)
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

                approach.Scale = new Vector2(1 + (float)MathHelper.Clamp((HitTime - Clock.CurrentTime) / approach_rate_milliseconds, 0, 100));
                Alpha = (float)MathHelper.Clamp((Clock.CurrentTime - HitTime + 600) / 400, 0, 1);

                if (Clock.CurrentTime > HitTime + 200)
                    Expire();
            }
        }
    }
}
