// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public abstract class DrawableTaikoHitObject : DrawableHitObject<TaikoHitObject, TaikoJudgement>
    {
        protected DrawableTaikoHitObject(TaikoHitObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.X;

            Children = new[]
            {
                CreateCircle()
            };
        }

        protected override void LoadComplete()
        {
            LifetimeStart = HitObject.StartTime - HitObject.PreEmpt * 2;
            LifetimeEnd = HitObject.StartTime + HitObject.PreEmpt;

            base.LoadComplete();
        }

        protected override TaikoJudgement CreateJudgement() => new TaikoJudgement();

        /// <summary>
        /// Sets the scroll position of the DrawableHitObject relative to the offset between
        /// a time value and the HitObject's StartTime.
        /// </summary>
        /// <param name="time"></param>
        protected virtual void UpdateScrollPosition(double time)
        {
            MoveToX((float)((HitObject.StartTime - time) / HitObject.PreEmpt));
        }

        protected override void Update()
        {
            UpdateScrollPosition(Time.Current);
        }

        protected abstract CirclePiece CreateCircle();
    }
}
