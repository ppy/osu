// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Graphics;
using OpenTK.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public abstract class DrawableTaikoHitObject : DrawableHitObject<TaikoHitObject>
    {
        /// <summary>
        /// The colour to be shown when this HitObject is hit.
        /// </summary>
        public abstract Color4 ExplodeColour { get; protected set; }

        public override bool ExpireOnStateChange => false;

        public DrawableTaikoHitObject(TaikoHitObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.X;
        }

        protected override JudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo { MaxScore = TaikoScoreResult.Great };

        /// <summary>
        /// Todo: Remove
        /// </summary>
        protected override void LoadComplete()
        {
            if (Judgement == null)
                Judgement = CreateJudgementInfo();

            UpdateState(State);

            // Very naive, but should be enough, given that notes scroll more than 50% of the stage
            // before start time, so they should scroll off the screen before start time + preempt
            TaikoHitObject tho = HitObject as TaikoHitObject;
            LifetimeStart = HitObject.StartTime - tho.PreEmpt * 2;
            LifetimeEnd = HitObject.EndTime + tho.PreEmpt;
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded)
                return;

            TaikoHitObject tho = HitObject as TaikoHitObject;

            Flush();
        }

        protected virtual void MoveToOffset(double time)
        {
            TaikoHitObject tho = HitObject as TaikoHitObject;
            MoveToX((float)((tho.StartTime - time) / tho.PreEmpt));
        }

        protected override void Update()
        {
            MoveToOffset(Time.Current);
        }
    }
}
