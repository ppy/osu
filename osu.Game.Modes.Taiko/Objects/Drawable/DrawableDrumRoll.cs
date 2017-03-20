// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using System.Linq;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableDrumRoll : DrawableTaikoHitObject
    {
        private DrumRoll drumRoll;

        private Container tickContainer;

        public DrawableDrumRoll(DrumRoll drumRoll)
            : base(drumRoll)
        {
            this.drumRoll = drumRoll;

            RelativeSizeAxes = Axes.X;
            Width = (float)(drumRoll.Duration / HitObject.PreEmpt);

            Add(tickContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                Depth = -1
            });

            int tickIndex = 0;
            foreach (var tick in drumRoll.Ticks)
            {
                var newTick = new DrawableDrumRollTick(tick)
                {
                    Depth = tickIndex,
                    X = (float)((tick.StartTime - HitObject.StartTime) / drumRoll.Duration)
                };

                AddNested(newTick);
                tickContainer.Add(newTick);

                tickIndex++;
            }
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
                Judgement.Score = countHit >= drumRoll.RequiredGreatHits ? TaikoScoreResult.Great : TaikoScoreResult.Good;
            }
            else
                Judgement.Result = HitResult.Miss;
        }

        protected override ScrollingCirclePiece CreateCircle() => new DrumRollCirclePiece
        {
            RelativeSizeAxes = Axes.X
        };
    }
}
