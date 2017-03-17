// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableTaikoHitObject : DrawableHitObject<TaikoHitObject, TaikoJudgementInfo>
    {
        public DrawableTaikoHitObject(TaikoHitObject hitObject)
            : base(hitObject)
        {
            LifetimeStart = HitObject.StartTime - HitObject.PreEmpt;
            LifetimeEnd = HitObject.StartTime + HitObject.PreEmpt;
        }

        protected override TaikoJudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo();

        protected override void UpdateState(ArmedState state)
        {
        }

        protected void UpdateScrollPosition(double time)
        {
            MoveToX((float)((HitObject.StartTime - time) / HitObject.PreEmpt));
        }

        protected override void Update()
        {
            UpdateScrollPosition(Time.Current);
        }
    }
}
