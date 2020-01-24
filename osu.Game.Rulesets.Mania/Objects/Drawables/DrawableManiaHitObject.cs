// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract class DrawableManiaHitObject : DrawableHitObject<ManiaHitObject>
    {
        /// <summary>
        /// Whether this <see cref="DrawableManiaHitObject"/> should always remain alive.
        /// </summary>
        internal bool AlwaysAlive;

        /// <summary>
        /// The <see cref="ManiaAction"/> which causes this <see cref="DrawableManiaHitObject{TObject}"/> to be hit.
        /// </summary>
        protected readonly IBindable<ManiaAction> Action = new Bindable<ManiaAction>();

        protected readonly IBindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

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

        protected override bool ShouldBeAlive => AlwaysAlive || base.ShouldBeAlive;

        protected virtual void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> e)
        {
            Anchor = Origin = e.NewValue == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Miss:
                    this.FadeOut(150, Easing.In);
                    break;

                case ArmedState.Hit:
                    this.FadeOut(150, Easing.OutQuint);
                    break;
            }
        }
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
