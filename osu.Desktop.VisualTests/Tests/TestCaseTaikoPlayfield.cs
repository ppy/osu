// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using osu.Framework.Testing;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Modes.Taiko.UI;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTaikoPlayfield : TestCase
    {
        public override string Description => "Taiko playfield";

        private TaikoPlayfield playfield;

        protected override double TimePerAction => 500;

        public override void Reset()
        {
            base.Reset();

            AddStep("Hit!", addHitJudgement);
            AddStep("Miss :(", addMissJudgement);
            AddStep("DrumRoll", () => addDrumRoll(false));
            AddStep("Strong DrumRoll", () => addDrumRoll(true));
            AddStep("Swell", addSwell);
            AddStep("Centre", () => addCentreHit(false));
            AddStep("Strong Centre", () => addCentreHit(true));
            AddStep("Rim", () => addRimHit(false));
            AddStep("Strong Rim", () => addRimHit(true));
            AddStep("Add bar line", () => addBarLine(false));
            AddStep("Add major bar line", () => addBarLine(true));

            Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                Y = 200,
                Children = new[]
                {
                    playfield = new TaikoPlayfield()
                }
            });
        }

        private void addHitJudgement()
        {
            TaikoHitResult hitResult = RNG.Next(2) == 0 ? TaikoHitResult.Good : TaikoHitResult.Great;

            playfield.OnJudgement(new DrawableTestHit(new Hit())
            {
                X = RNG.NextSingle(hitResult == TaikoHitResult.Good ? -0.1f : -0.05f, hitResult == TaikoHitResult.Good ? 0.1f : 0.05f),
                Judgement = new TaikoJudgement
                {
                    Result = HitResult.Hit,
                    TaikoResult = hitResult,
                    TimeOffset = 0,
                    SecondHit = RNG.Next(10) == 0
                }
            });
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

        private void addBarLine(bool major)
        {
            BarLine bl = new BarLine
            {
                StartTime = Time.Current + 1000,
                PreEmpt = 1000
            };

            playfield.AddBarLine(major ? new DrawableMajorBarLine(bl) : new DrawableBarLine(bl));
        }

        private void addSwell()
        {
            playfield.Add(new DrawableSwell(new Swell
            {
                StartTime = Time.Current + 1000,
                EndTime = Time.Current + 1000,
                PreEmpt = 1000
            }));
        }
        
        private void addDrumRoll(bool strong)
        {
            var d = new DrumRoll
            {
                StartTime = Time.Current + 1000,
                Distance = 1000,
                PreEmpt = 1000,
            };

            playfield.Add(strong ? new DrawableStrongDrumRoll(d) : new DrawableDrumRoll(d));
        }

        private void addCentreHit(bool strong)
        {
            Hit h = new Hit
            {
                StartTime = Time.Current + 1000,
                PreEmpt = 1000
            };

            if (strong)
                playfield.Add(new DrawableStrongCentreHit(h));
            else
                playfield.Add(new DrawableCentreHit(h));
        }

        private void addRimHit(bool strong)
        {
            Hit h = new Hit
            {
                StartTime = Time.Current + 1000,
                PreEmpt = 1000
            };

            if (strong)
                playfield.Add(new DrawableStrongRimHit(h));
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
