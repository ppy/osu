// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable
using System;
using System.Collections.Generic;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Utility
{
    public class CircleGameplay : BeatSyncedContainer
    {
        private int nextLocation;

        private OsuSpriteText unstableRate = null!;

        private readonly List<HitEvent> hitEvents = new List<HitEvent>();

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

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            nextLocation++;

            Vector2 location;

            const float spacing = 0.1f;

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

            AddInternal(new SampleHitCircle(Clock.CurrentTime + timingPoint.BeatLength)
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

                approach.Scale = new Vector2((float)MathHelper.Clamp((HitTime - Clock.CurrentTime) / 60, 1, 100));

                if (Clock.CurrentTime > HitTime + 80)
                    Expire();
            }
        }
    }
}
