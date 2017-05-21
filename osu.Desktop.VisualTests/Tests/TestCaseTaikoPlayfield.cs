// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using System;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTaikoPlayfield : TestCase
    {
        private const double default_duration = 300;
        private const float scroll_time = 1000;

        public override string Description => "Taiko playfield";

        protected override double TimePerAction => default_duration * 2;

        private readonly Random rng = new Random(1337);
        private TaikoPlayfield playfield;
        private Container playfieldContainer;

        public override void Reset()
        {
            base.Reset();

            AddStep("Hit!", addHitJudgement);
            AddStep("Miss :(", addMissJudgement);
            AddStep("DrumRoll", () => addDrumRoll(false));
            AddStep("Strong DrumRoll", () => addDrumRoll(true));
            AddStep("Swell", () => addSwell());
            AddStep("Centre", () => addCentreHit(false));
            AddStep("Strong Centre", () => addCentreHit(true));
            AddStep("Rim", () => addRimHit(false));
            AddStep("Strong Rim", () => addRimHit(true));
            AddStep("Add bar line", () => addBarLine(false));
            AddStep("Add major bar line", () => addBarLine(true));
            AddStep("Height test 1", () => changePlayfieldSize(1));
            AddStep("Height test 2", () => changePlayfieldSize(2));
            AddStep("Height test 3", () => changePlayfieldSize(3));
            AddStep("Height test 4", () => changePlayfieldSize(4));
            AddStep("Height test 5", () => changePlayfieldSize(5));
            AddStep("Reset height", () => changePlayfieldSize(6));

            var rateAdjustClock = new StopwatchClock(true) { Rate = 1 };

            Add(playfieldContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Height = TaikoPlayfield.DEFAULT_PLAYFIELD_HEIGHT,
                Clock = new FramedClock(rateAdjustClock),
                Children = new[]
                {
                    playfield = new TaikoPlayfield()
                }
            });
        }

        private void changePlayfieldSize(int step)
        {
            // Add new hits
            switch (step)
            {
                case 1:
                    addCentreHit(false);
                    break;
                case 2:
                    addCentreHit(true);
                    break;
                case 3:
                    addDrumRoll(false);
                    break;
                case 4:
                    addDrumRoll(true);
                    break;
                case 5:
                    addSwell(1000);
                    playfieldContainer.Delay(scroll_time - 100);
                    break;
            }

            // Tween playfield height
            switch (step)
            {
                default:
                    playfieldContainer.ResizeTo(new Vector2(1, rng.Next(25, 400)), 500);
                    break;
                case 6:
                    playfieldContainer.ResizeTo(new Vector2(1, TaikoPlayfield.DEFAULT_PLAYFIELD_HEIGHT), 500);
                    break;
            }
        }

        private void addHitJudgement()
        {
            TaikoHitResult hitResult = RNG.Next(2) == 0 ? TaikoHitResult.Good : TaikoHitResult.Great;

            var h = new DrawableTestHit(new Hit())
            {
                X = RNG.NextSingle(hitResult == TaikoHitResult.Good ? -0.1f : -0.05f, hitResult == TaikoHitResult.Good ? 0.1f : 0.05f),
                Judgement = new TaikoJudgement
                {
                    Result = HitResult.Hit,
                    TaikoResult = hitResult,
                    TimeOffset = 0
                }
            };

            playfield.OnJudgement(h);

            if (RNG.Next(10) == 0)
            {
                h.Judgement.SecondHit = true;
                playfield.OnJudgement(h);
            }
        }

        private void addMissJudgement()
        {
            playfield.OnJudgement(new DrawableTestHit(new Hit())
            {
                Judgement = new TaikoJudgement
                {
                    Result = HitResult.Miss,
                    TimeOffset = 0
                }
            });
        }

        private void addBarLine(bool major, double delay = scroll_time)
        {
            BarLine bl = new BarLine
            {
                StartTime = playfield.Time.Current + delay,
                ScrollTime = scroll_time
            };

            playfield.AddBarLine(major ? new DrawableBarLineMajor(bl) : new DrawableBarLine(bl));
        }

        private void addSwell(double duration = default_duration)
        {
            playfield.Add(new DrawableSwell(new Swell
            {
                StartTime = playfield.Time.Current + scroll_time,
                Duration = duration,
                ScrollTime = scroll_time
            }));
        }

        private void addDrumRoll(bool strong, double duration = default_duration)
        {
            addBarLine(true);
            addBarLine(true, scroll_time + duration);

            var d = new DrumRoll
            {
                StartTime = playfield.Time.Current + scroll_time,
                IsStrong = strong,
                Duration = duration,
                ScrollTime = scroll_time,
            };

            playfield.Add(new DrawableDrumRoll(d));
        }

        private void addCentreHit(bool strong)
        {
            Hit h = new Hit
            {
                StartTime = playfield.Time.Current + scroll_time,
                ScrollTime = scroll_time
            };

            if (strong)
                playfield.Add(new DrawableCentreHitStrong(h));
            else
                playfield.Add(new DrawableCentreHit(h));
        }

        private void addRimHit(bool strong)
        {
            Hit h = new Hit
            {
                StartTime = playfield.Time.Current + scroll_time,
                ScrollTime = scroll_time
            };

            if (strong)
                playfield.Add(new DrawableRimHitStrong(h));
            else
                playfield.Add(new DrawableRimHit(h));
        }

        private class DrawableTestHit : DrawableHitObject<TaikoHitObject, TaikoJudgement>
        {
            public DrawableTestHit(TaikoHitObject hitObject)
                : base(hitObject)
            {
            }

            protected override TaikoJudgement CreateJudgement() => new TaikoJudgement();

            protected override void UpdateState(ArmedState state)
            {
            }
        }
    }
}
