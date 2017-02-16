// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.Objects.Drawables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Modes;
using OpenTK.Graphics;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseHitObjects : TestCase
    {
        public override string Name => @"Hit Objects";

        private StopwatchClock rateAdjustClock;
        private FramedClock framedClock;

        bool auto = false;

        public TestCaseHitObjects()
        {
            rateAdjustClock = new StopwatchClock(true);
            framedClock = new FramedClock(rateAdjustClock);
            playbackSpeed.ValueChanged += delegate { rateAdjustClock.Rate = playbackSpeed.Value; };
        }

        HitObjectType mode = HitObjectType.Slider;

        BindableNumber<double> playbackSpeed = new BindableDouble(0.5) { MinValue = 0, MaxValue = 1 };
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
                        ControlPoints = new List<Vector2>()
                        {
                            new Vector2(-200, 0),
                            new Vector2(400, 0),
                        },
                        Length = 400,
                        Position = new Vector2(-200, 0),
                        Velocity = 1,
                        TickDistance = 100,
                    }));
                    break;
                case HitObjectType.Spinner:
                    add(new DrawableSpinner(new Spinner
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        Length = 1000,
                        Position = new Vector2(0, 0),
                    }));
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();

            playbackSpeed.TriggerChange();

            AddButton(@"circles", () => load(HitObjectType.Circle));
            AddButton(@"slider", () => load(HitObjectType.Slider));
            AddButton(@"spinner", () => load(HitObjectType.Spinner));

            AddToggle(@"auto", () => { auto = !auto; load(mode); });

            ButtonsContainer.Add(new SpriteText { Text = "Playback Speed" });
            ButtonsContainer.Add(new BasicSliderBar<double>
            {
                Width = 150,
                Height = 10,
                SelectionColor = Color4.Orange,
                Bindable = playbackSpeed
            });

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

        int depth;
        void add(DrawableHitObject h)
        {
            h.Anchor = Anchor.Centre;
            h.Depth = depth++;

            if (auto)
            {
                h.State = ArmedState.Hit;
                h.Judgement = new OsuJudgementInfo { Result = HitResult.Hit };
            }

            playfieldContainer.Add(h);
            var proxyable = h as IDrawableHitObjectWithProxiedApproach;
            if (proxyable != null)
                approachContainer.Add(proxyable.ProxiedLayer.CreateProxy());
        }
    }
}
