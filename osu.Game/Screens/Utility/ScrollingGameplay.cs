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
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Utility.SampleComponents;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Utility
{
    public partial class ScrollingGameplay : LatencySampleComponent
    {
        private const float judgement_position = 0.8f;
        private const float bar_height = 20;

        private int nextLocation;

        private readonly List<HitEvent> hitEvents = new List<HitEvent>();

        private double? lastGeneratedBeatTime;

        private Container circles = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Name = "judgement bar",
                    Colour = OverlayColourProvider.Content2,
                    RelativeSizeAxes = Axes.X,
                    RelativePositionAxes = Axes.Y,
                    Y = judgement_position,
                    Height = bar_height,
                },
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
            const float columns = 4;

            float adjustedXPos = ((1f + nextLocation++ % columns) - columns / 2) / columns;

            circles.Add(new SampleNote(time)
            {
                RelativePositionAxes = Axes.Both,
                X = 0.5f + SampleVisualSpacing.Value * (adjustedXPos * 0.5f),
                Scale = new Vector2(0.4f + (0.8f * SampleVisualSpacing.Value), 1),
                Hit = hit,
            });
        }

        private void hit(HitEvent h)
        {
            hitEvents.Add(h);
        }

        public partial class SampleNote : LatencySampleComponent
        {
            public HitEvent? HitEvent;

            public Action<HitEvent>? Hit { get; set; }

            public readonly double HitTime;

            private Box box = null!;

            private const float size = 100;
            private const float duration = 200;

            public SampleNote(double hitTime)
            {
                HitTime = hitTime;

                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;
                AlwaysPresent = true;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    box = new Box
                    {
                        Colour = OverlayColourProvider.Content1,
                        Size = new Vector2(size, bar_height),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                };
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (!IsActive.Value)
                    return false;

                if (Math.Abs(Clock.CurrentTime - HitTime) > duration)
                    return false;

                // Allow using any key that isn't used by the latency certifier itself.
                switch (e.Key)
                {
                    case Key.Space:
                    case Key.Number1:
                    case Key.Number2:
                    case Key.Tab:
                        return false;
                }

                attemptHit();
                return true;
            }

            protected override void UpdateAtLimitedRate(InputState inputState)
            {
                if (HitEvent == null)
                {
                    double preempt = (float)IBeatmapDifficultyInfo.DifficultyRange(SampleApproachRate.Value, 1800, 1200, 450);

                    Alpha = (float)MathHelper.Clamp((Clock.CurrentTime - HitTime + 600) / 400, 0, 1);
                    Y = judgement_position - (float)((HitTime - Clock.CurrentTime) / preempt);

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

                box
                    .FadeOut(duration / 2)
                    .ScaleTo(1.5f, duration / 2);

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
