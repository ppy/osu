// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.MathUtils;
using osu.Framework.Screens.Testing;
using osu.Framework.Timing;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawable;
using osu.Game.Modes.Taiko.UI;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTaikoPlayfield : TestCase
    {
        public override string Description => "Taiko playfield";

        private TaikoPlayfield playfield;

        public TestCaseTaikoPlayfield()
        {
            Clock = new FramedClock();
        }

        public override void Reset()
        {
            base.Reset();

            Clock.ProcessFrame();

            AddButton("Hit!", addHitJudgement);
            AddButton("Miss :(", addMissJudgement);
            AddButton("Add bar line", addBarLine);

            Add(playfield = new TaikoPlayfield
            {
                Y = 200
            });
        }

        private void addHitJudgement()
        {
            TaikoScoreResult score = RNG.Next(2) == 0 ? TaikoScoreResult.Good : TaikoScoreResult.Great;

            playfield.OnJudgement(new DrawableTestHit(new TaikoHitObject())
            {
                X = RNG.NextSingle(score == TaikoScoreResult.Good ? -0.1f : -0.05f, score == TaikoScoreResult.Good ? 0.1f : 0.05f),
                Judgement = new TaikoJudgementInfo
                {
                    Result = HitResult.Hit,
                    Score = score,
                    TimeOffset = 0,
                    ComboAtHit = 1,
                    SecondHit = RNG.Next(10) == 0
                }
            });
        }

        private void addMissJudgement()
        {
            playfield.OnJudgement(new DrawableTestHit(new TaikoHitObject())
            {
                Judgement = new TaikoJudgementInfo
                {
                    Result = HitResult.Miss,
                    TimeOffset = 0,
                    ComboAtHit = 0
                }
            });
        }

        private void addBarLine()
        {
            bool isMajor = RNG.Next(8) == 0;

            BarLine bl = new BarLine
            {
                StartTime = Time.Current + 1000,
                PreEmpt = 1000
            };

            playfield.AddBarLine(isMajor ? new DrawableMajorBarLine(bl) : new DrawableBarLine(bl));
        }

        private class DrawableTestHit : DrawableHitObject<TaikoHitObject, TaikoJudgementInfo>
        {
            public DrawableTestHit(TaikoHitObject hitObject)
                : base(hitObject)
            {
            }

            protected override TaikoJudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo();

            protected override void UpdateState(ArmedState state)
            {
            }

            protected override void Update()
            {
                // Doesn't move
            }
        }
    }
}
