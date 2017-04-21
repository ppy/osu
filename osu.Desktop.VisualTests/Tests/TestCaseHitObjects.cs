// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using System.Collections.Generic;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseHitObjects : TestCase
    {
        private readonly FramedClock framedClock;

        private bool auto;

        public TestCaseHitObjects()
        {
            var rateAdjustClock = new StopwatchClock(true);
            framedClock = new FramedClock(rateAdjustClock);
            playbackSpeed.ValueChanged += delegate { rateAdjustClock.Rate = playbackSpeed.Value; };
        }

        private HitObjectType mode = HitObjectType.Slider;

        private readonly BindableNumber<double> playbackSpeed = new BindableDouble(0.5) { MinValue = 0, MaxValue = 1 };
        private Container playfieldContainer;
        private Container approachContainer;

        private void load(HitObjectType mode)
        {
            this.mode = mode;

            switch (mode)
            {
                case HitObjectType.Circle:
                    const int count = 10;

                    for (int i = 0; i < count; i++)
                    {
                        var h = new HitCircle
                        {
                            StartTime = framedClock.CurrentTime + 600 + i * 80,
                            Position = new Vector2((i - count / 2) * 14),
                        };

                        add(new DrawableHitCircle(h));
                    }
                    break;
                case HitObjectType.Slider:
                    add(new DrawableSlider(new Slider
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        ControlPoints = new List<Vector2>
                        {
                            new Vector2(-200, 0),
                            new Vector2(400, 0),
                        },
                        Distance = 400,
                        Position = new Vector2(-200, 0),
                        Velocity = 1,
                        TickDistance = 100,
                    }));
                    break;
                case HitObjectType.Spinner:
                    add(new DrawableSpinner(new Spinner
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        EndTime = framedClock.CurrentTime + 1600,
                        Position = new Vector2(0, 0),
                    }));
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();

            playbackSpeed.TriggerChange();

            AddStep(@"circles", () => load(HitObjectType.Circle));
            AddStep(@"slider", () => load(HitObjectType.Slider));
            AddStep(@"spinner", () => load(HitObjectType.Spinner));

            AddToggleStep(@"auto", state => { auto = state; load(mode); });

            BasicSliderBar<double> sliderBar;
            Add(new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new SpriteText { Text = "Playback Speed" },
                    sliderBar = new BasicSliderBar<double>
                    {
                        Width = 150,
                        Height = 10,
                        SelectionColor = Color4.Orange,
                    }
                }
            });

            sliderBar.Current.BindTo(playbackSpeed);

            framedClock.ProcessFrame();

            var clockAdjustContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Clock = framedClock,
                Children = new[]
                {
                    playfieldContainer = new Container { RelativeSizeAxes = Axes.Both },
                    approachContainer = new Container { RelativeSizeAxes = Axes.Both }
                }
            };

            Add(clockAdjustContainer);

            load(mode);
        }

        private int depth;

        private void add(DrawableOsuHitObject h)
        {
            h.Anchor = Anchor.Centre;
            h.Depth = depth++;

            if (auto)
            {
                h.State = ArmedState.Hit;
                h.Judgement = new OsuJudgement { Result = HitResult.Hit };
            }

            playfieldContainer.Add(h);
            var proxyable = h as IDrawableHitObjectWithProxiedApproach;
            if (proxyable != null)
                approachContainer.Add(proxyable.ProxiedLayer.CreateProxy());
        }

        private enum HitObjectType
        {
            Circle,
            Slider,
            Spinner
        }
    }
}
