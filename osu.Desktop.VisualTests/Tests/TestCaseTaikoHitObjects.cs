// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens.Testing;
using osu.Framework.Timing;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawables.Bashes;
using osu.Game.Modes.Taiko.Objects.Drawables.DrumRolls;
using osu.Game.Modes.Taiko.Objects.Drawables.Hits;
using osu.Game.Modes.Taiko.UI;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseTaikoHitObjects : TestCase
    {
        private StopwatchClock rateAdjustClock;
        private FramedClock framedClock;

        bool auto = false;

        public TestCaseTaikoHitObjects()
        {
            rateAdjustClock = new StopwatchClock(true);
            framedClock = new FramedClock(rateAdjustClock);
            playbackSpeed.ValueChanged += delegate { rateAdjustClock.Rate = playbackSpeed.Value; };
        }

        HitObjectType mode = HitObjectType.Bash;

        BindableNumber<double> playbackSpeed = new BindableDouble(0.5) { MinValue = 0, MaxValue = 1 };
        private TaikoPlayfield playfield;

        private void load(HitObjectType mode)
        {
            this.mode = mode;

            switch (mode)
            {
                case HitObjectType.CentreHit:
                    const int count = 10;

                    for (int i = 0; i < count; i++)
                    {
                        var h = new TaikoHitObject
                        {
                            StartTime = framedClock.CurrentTime + 600 + i * 80,
                            PreEmpt = 500
                        };

                        add(new DrawableCentreHit(h));
                    }
                    break;
                case HitObjectType.RimHit:
                    for (int i = 0; i < count; i++)
                    {
                        var h = new TaikoHitObject
                        {
                            StartTime = framedClock.CurrentTime + 600 + i * 80,
                            PreEmpt = 500
                        };

                        add(new DrawableRimHit(h));
                    }
                    break;
                case HitObjectType.CentreHitFinisher:
                    for (int i = 0; i < count; i++)
                    {
                        var h = new TaikoHitObject
                        {
                            StartTime = framedClock.CurrentTime + 600 + i * 80,
                            PreEmpt = 500
                        };

                        add(new DrawableCentreHitFinisher(h));
                    }
                    break;
                case HitObjectType.RimHitFinisher:
                    for (int i = 0; i < count; i++)
                    {
                        var h = new TaikoHitObject
                        {
                            StartTime = framedClock.CurrentTime + 600 + i * 80,
                            PreEmpt = 500
                        };

                        add(new DrawableRimHitFinisher(h));
                    }
                    break;
                case HitObjectType.DrumRoll:
                    add(new DrawableDrumRoll(new DrumRoll
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        Length = 1600,
                        Velocity = 1,
                        TickTimeDistance = 50,
                        PreEmpt = 500
                    }));
                    break;
                case HitObjectType.DrumRollFinisher:
                    add(new DrawableDrumRollFinisher(new DrumRoll
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        Length = 1600,
                        Velocity = 1,
                        TickTimeDistance = 50,
                        PreEmpt = 500
                    }));
                    break;
                case HitObjectType.Bash:
                    add(new DrawableBash(new Bash
                    {
                        StartTime = framedClock.CurrentTime + 600,
                        Length = 1000,
                        PreEmpt = 500,
                        RequiredHits = 20
                    }));
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();

            playbackSpeed.TriggerChange();

            AddButton(@"centre hits", () => load(HitObjectType.CentreHit));
            AddButton(@"centre hit finishers", () => load(HitObjectType.CentreHitFinisher));
            AddButton(@"rim hits", () => load(HitObjectType.RimHit));
            AddButton(@"rim hit finishers", () => load(HitObjectType.RimHitFinisher));
            AddButton(@"drum roll", () => load(HitObjectType.DrumRoll));
            AddButton(@"drum roll finisher", () => load(HitObjectType.DrumRollFinisher));
            AddButton(@"bash", () => load(HitObjectType.Bash));

            AddToggle(@"auto", state => { auto = !auto; load(mode); });

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
                Children = new Drawable[]
                {
                    playfield = new TaikoPlayfield()
                    {
                        RelativePositionAxes = Axes.Y,
                        Position = new Vector2(160, 0.4f)
                    },
                }
            };

            Add(clockAdjustContainer);

            load(mode);
        }

        int depth;
        void add(DrawableHitObject<TaikoHitObject> h)
        {
            h.Depth = depth++;

            if (auto)
            {
                h.State = ArmedState.Hit;
                h.Judgement = new TaikoJudgementInfo { Result = HitResult.Hit };
            }

            playfield.Add(h);
        }

        enum HitObjectType
        {
            CentreHit,
            RimHit,
            CentreHitFinisher,
            RimHitFinisher,
            DrumRoll,
            DrumRollFinisher,
            Bash
        }
    }
}
