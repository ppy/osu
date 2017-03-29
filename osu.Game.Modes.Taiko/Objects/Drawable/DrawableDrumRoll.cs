// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;
using System.Linq;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableDrumRoll : DrawableTaikoHitObject
    {
        private readonly DrumRoll drumRoll;

        public DrawableDrumRoll(DrumRoll drumRoll)
            : base(drumRoll)
        {
            this.drumRoll = drumRoll;

            RelativeSizeAxes = Axes.X;
            Width = (float)(drumRoll.Duration / drumRoll.PreEmpt);

            Add(new DrumRollCirclePiece(CreateCirclePiece()));

            foreach (var tick in drumRoll.Ticks)
            {
                var newTick = new DrawableDrumRollTick(tick)
                {
                    X = (float)((tick.StartTime - HitObject.StartTime) / drumRoll.Duration)
                };

                AddNested(newTick);
                Add(newTick);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // This is naive, however it's based on the reasoning that the hit target
            // is further than mid point of the play field, so the time taken to scroll in should always
            // be greater than the time taken to scroll out to the left of the screen.
            // Thus, using PreEmpt here is enough for the drum roll to completely scroll out.
            LifetimeEnd = drumRoll.EndTime + drumRoll.PreEmpt;
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (userTriggered)
                return;

            if (Judgement.TimeOffset < 0)
                return;

            int countHit = NestedHitObjects.Count(o => o.Judgement.Result == HitResult.Hit);

            if (countHit > drumRoll.RequiredGoodHits)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.TaikoResult = countHit >= drumRoll.RequiredGreatHits ? TaikoHitResult.Great : TaikoHitResult.Good;
            }
            else
                Judgement.Result = HitResult.Miss;
        }

        protected override void UpdateState(ArmedState state)
        {
        }

        protected virtual CirclePiece CreateCirclePiece() => new CirclePiece();
    }
}
