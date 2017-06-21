// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Drawables
{
    /// <summary>
    /// A basic class that overrides <see cref="DrawableHitObject{TObject, TJudgement}"/> and implements <see cref="IScrollingHitObject"/>.
    /// </summary>
    public abstract class DrawableScrollingHitObject<TObject, TJudgement> : DrawableHitObject<TObject, TJudgement>, IScrollingHitObject
        where TObject : HitObject
        where TJudgement : Judgement
    {
        public BindableDouble LifetimeOffset { get; } = new BindableDouble();

        protected DrawableScrollingHitObject(TObject hitObject)
            : base(hitObject)
        {
        }

        public override double LifetimeStart
        {
            get { return Math.Min(HitObject.StartTime - LifetimeOffset, base.LifetimeStart); }
            set { base.LifetimeStart = value; }
        }

        public override double LifetimeEnd
        {
            get
            {
                var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;
                return Math.Max(endTime + LifetimeOffset, base.LifetimeEnd);
            }
            set { base.LifetimeEnd = value; }
        }

        protected override void AddNested(DrawableHitObject<TObject, TJudgement> h)
        {
            var scrollingHitObject = h as IScrollingHitObject;
            scrollingHitObject?.LifetimeOffset.BindTo(LifetimeOffset);

            base.AddNested(h);
        }
    }
}