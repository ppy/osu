// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.MathUtils;
using osu.Framework.Screens.Testing;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.UI;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTaikoPlayfield : TestCase
    {
        public override string Description => "Taiko playfield";

        private TaikoPlayfield playfield;

        public override void Reset()
        {
            base.Reset();

            AddButton("Hit!", addHitJudgement);
            AddButton("Miss :(", addMissJudgement);

            Add(playfield = new TaikoPlayfield
            {
                Y = 200
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
                    ComboAtHit = 1,
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
                    TimeOffset = 0,
                    ComboAtHit = 0
                }
            });
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
