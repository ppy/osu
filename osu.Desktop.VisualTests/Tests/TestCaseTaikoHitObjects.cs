// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework;
using osu.Framework.Screens.Testing;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Modes;
using OpenTK.Graphics;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Modes.Taiko.UI;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseTaikoHitObjects : TestCase
    {
        public override string Name => @"Hit Objects (Taiko)";

        private StopwatchClock rateAdjustClock;
        private FramedClock framedClock;

        bool auto = false;

        public TestCaseTaikoHitObjects()
        {
            rateAdjustClock = new StopwatchClock(true);
            framedClock = new FramedClock(rateAdjustClock);
            playbackSpeed.ValueChanged += delegate { rateAdjustClock.Rate = playbackSpeed.Value; };
        }

        HitObjectType mode = HitObjectType.Don;

        BindableNumber<double> playbackSpeed = new BindableDouble(0.5) { MinValue = 0, MaxValue = 1 };
        private TaikoPlayfield playfield;

        private void load(HitObjectType mode)
        {
            this.mode = mode;

            switch (mode)
            {
                case HitObjectType.Don:
                    const int count = 10;

                    for (int i = 0; i < count; i++)
                    {
                        var h = new HitCircle
                        {
                            StartTime = framedClock.CurrentTime + 600 + i * 80,
                            PreEmpt = 500
                        };

                        add(new DrawableHitCircleDon(h));
                    }
                    break;
                case HitObjectType.Katsu:
                    for (int i = 0; i < count; i++)
                    {
                        var h = new HitCircle
                        {
                            StartTime = framedClock.CurrentTime + 600 + i * 80,
                            PreEmpt = 500
                        };

                        add(new DrawableHitCircleKatsu(h));
                    }
                    break;
                case HitObjectType.DonFinisher:
                    for (int i = 0; i < count; i++)
                    {
                        var h = new HitCircle
                        {
                            StartTime = framedClock.CurrentTime + 600 + i * 80,
                            PreEmpt = 500
                        };

                        add(new DrawableHitCircleDonFinisher(h));
                    }
                    break;
                case HitObjectType.KatsuFinisher:
                    for (int i = 0; i < count; i++)
                    {
                        var h = new HitCircle
                        {
                            StartTime = framedClock.CurrentTime + 600 + i * 80,
                            PreEmpt = 500
                        };

                        add(new DrawableHitCircleKatsuFinisher(h));
                    }
                    break;
                case HitObjectType.DrumRoll:
                    add(new DrawableDrumRoll(new DrumRoll
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        Length = 100 * (1000 / 500) - 10,
                        Velocity = 1,
                        TickDistance = 100 * (1000 / 500),
                        PreEmpt = 500
                    }));

                    for (int i = 0; i <= 400; i += 100)
                    {
                        add(new DrawableHitCircleDon(new HitCircle
                        {
                            StartTime = framedClock.CurrentTime + 600 + i,
                            PreEmpt = 500
                        }));
                    }
                    break;
                case HitObjectType.DrumRollFinisher:
                    add(new DrawableDrumRollFinisher(new DrumRoll
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        Length = 1600,
                        Velocity = 1,
                        TickDistance = 100,
                        PreEmpt = 500
                    }));
                    break;
                case HitObjectType.Spinner:
                    add(new DrawableSpinner(new Spinner
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        Length = 1000,
                        PreEmpt = 500
                    }));
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();

            playbackSpeed.TriggerChange();

            AddButton(@"dons", () => load(HitObjectType.Don));
            AddButton(@"don finishers", () => load(HitObjectType.DonFinisher));
            AddButton(@"katsus", () => load(HitObjectType.Katsu));
            AddButton(@"katsu finishers", () => load(HitObjectType.KatsuFinisher));
            AddButton(@"drum roll", () => load(HitObjectType.DrumRoll));
            AddButton(@"drum roll finisher", () => load(HitObjectType.DrumRollFinisher));
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
                    playfield = new TaikoPlayfield(),
                }
            };

            Add(clockAdjustContainer);

            load(mode);
        }

        int depth;
        void add(DrawableHitObject h)
        {
            h.Depth = depth++;

            if (auto)
            {
                h.State = ArmedState.Hit;
                h.Judgement = new TaikoJudgementInfo { Result = HitResult.Hit };
            }

            playfield.HitObjects.Add(h);
        }

        enum HitObjectType
        {
            Don,
            Katsu,
            DonFinisher,
            KatsuFinisher,
            DrumRoll,
            DrumRollFinisher,
            Spinner
        }
    }
}
