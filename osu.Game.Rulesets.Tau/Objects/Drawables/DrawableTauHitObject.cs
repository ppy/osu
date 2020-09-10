using System;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Tau.Objects.Drawables
{
    public abstract class DrawableTauHitObject : DrawableHitObject<TauHitObject>
    {
        protected DrawableTauHitObject(TauHitObject obj)
            : base(obj)
        {
        }

        public Func<DrawableTauHitObject, bool> CheckValidation;

        /// <summary>
        /// A list of keys which can result in hits for this HitObject.
        /// </summary>
        protected virtual TauAction[] HitActions { get; set; } =
        {
            TauAction.RightButton,
            TauAction.LeftButton,
        };

        /// <summary>
        /// The action that caused this <see cref="DrawableTauHitObject"/> to be hit.
        /// </summary>
        protected TauAction? HitAction { get; set; }

        protected override double InitialLifetimeOffset => HitObject.TimePreempt;
    }
}
