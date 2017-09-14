// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Drawables
{
    /// <summary>
    /// A basic class that overrides <see cref="DrawableHitObject{TObject}"/> and implements <see cref="IScrollingHitObject"/>.
    /// This object does not need to have its <see cref="Drawable.RelativePositionAxes"/> set to be able to scroll, as this will
    /// will be set by the scrolling container that contains it.
    /// </summary>
    public abstract class DrawableScrollingHitObject<TObject> : DrawableHitObject<TObject>, IScrollingHitObject
        where TObject : HitObject
    {
        public BindableDouble LifetimeOffset { get; } = new BindableDouble();

        Axes IScrollingHitObject.ScrollingAxes
        {
            set
            {
                RelativePositionAxes |= value;

                if ((value & Axes.X) > 0)
                    X = (float)HitObject.StartTime;
                if ((value & Axes.Y) > 0)
                    Y = (float)HitObject.StartTime;
            }
        }

        public override bool RemoveWhenNotAlive => false;
        protected override bool RequiresChildrenUpdate => true;

        protected DrawableScrollingHitObject(TObject hitObject)
            : base(hitObject)
        {
        }

        private double? lifetimeStart;
        public override double LifetimeStart
        {
            get { return lifetimeStart ?? HitObject.StartTime - LifetimeOffset; }
            set { lifetimeStart = value; }
        }

        private double? lifetimeEnd;
        public override double LifetimeEnd
        {
            get
            {
                var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;
                return lifetimeEnd ?? endTime + LifetimeOffset;
            }
            set { lifetimeEnd = value; }
        }

        protected override void AddNested(DrawableHitObject<TObject> h)
        {
            var scrollingHitObject = h as IScrollingHitObject;
            scrollingHitObject?.LifetimeOffset.BindTo(LifetimeOffset);

            base.AddNested(h);
        }
    }
}