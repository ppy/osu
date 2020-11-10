// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract class DrawableManiaHitObject : DrawableHitObject<ManiaHitObject>
    {
        /// <summary>
        /// The <see cref="ManiaAction"/> which causes this <see cref="DrawableManiaHitObject{TObject}"/> to be hit.
        /// </summary>
        protected readonly IBindable<ManiaAction> Action = new Bindable<ManiaAction>();

        protected readonly IBindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

        [Resolved(canBeNull: true)]
        private ManiaPlayfield playfield { get; set; }

        protected override float SamplePlaybackPosition
        {
            get
            {
                if (playfield == null)
                    return base.SamplePlaybackPosition;

                return (float)HitObject.Column / playfield.TotalColumns;
            }
        }

        /// <summary>
        /// Whether this <see cref="DrawableManiaHitObject"/> can be hit, given a time value.
        /// If non-null, judgements will be ignored whilst the function returns false.
        /// </summary>
        public Func<DrawableHitObject, double, bool> CheckHittable;

        protected DrawableManiaHitObject(ManiaHitObject hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] IBindable<ManiaAction> action, [NotNull] IScrollingInfo scrollingInfo)
        {
            if (action != null)
                Action.BindTo(action);

            Direction.BindTo(scrollingInfo.Direction);
            Direction.BindValueChanged(OnDirectionChanged, true);
        }

        private double computedLifetimeStart;

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set
            {
                computedLifetimeStart = value;

                if (!AlwaysAlive)
                    base.LifetimeStart = value;
            }
        }

        private double computedLifetimeEnd;

        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set
            {
                computedLifetimeEnd = value;

                if (!AlwaysAlive)
                    base.LifetimeEnd = value;
            }
        }

        private bool alwaysAlive;

        /// <summary>
        /// Whether this <see cref="DrawableManiaHitObject"/> should always remain alive.
        /// </summary>
        internal bool AlwaysAlive
        {
            get => alwaysAlive;
            set
            {
                if (alwaysAlive == value)
                    return;

                alwaysAlive = value;

                if (value)
                {
                    // Set the base lifetimes directly, to avoid mangling the computed lifetimes
                    base.LifetimeStart = double.MinValue;
                    base.LifetimeEnd = double.MaxValue;
                }
                else
                {
                    LifetimeStart = computedLifetimeStart;
                    LifetimeEnd = computedLifetimeEnd;
                }
            }
        }

        protected virtual void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> e)
        {
            Anchor = Origin = e.NewValue == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Miss:
                    this.FadeOut(150, Easing.In);
                    break;

                case ArmedState.Hit:
                    this.FadeOut();
                    break;
            }
        }

        /// <summary>
        /// Causes this <see cref="DrawableManiaHitObject"/> to get missed, disregarding all conditions in implementations of <see cref="DrawableHitObject.CheckForResult"/>.
        /// </summary>
        public void MissForcefully() => ApplyResult(r => r.Type = r.Judgement.MinResult);
    }

    public abstract class DrawableManiaHitObject<TObject> : DrawableManiaHitObject
        where TObject : ManiaHitObject
    {
        public new readonly TObject HitObject;

        protected DrawableManiaHitObject(TObject hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;
        }
    }
}
