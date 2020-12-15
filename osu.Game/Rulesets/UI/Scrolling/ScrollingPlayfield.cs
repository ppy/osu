// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.UI.Scrolling
{
    /// <summary>
    /// A type of <see cref="Playfield"/> specialized towards scrolling <see cref="DrawableHitObject"/>s.
    /// </summary>
    public abstract class ScrollingPlayfield : Playfield
    {
        protected readonly IBindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

        public new ScrollingHitObjectContainer HitObjectContainer => (ScrollingHitObjectContainer)base.HitObjectContainer;

        [Resolved]
        protected IScrollingInfo ScrollingInfo { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Direction.BindTo(ScrollingInfo.Direction);
        }

        /// <summary>
        /// Given a position in screen space, return the time within this column.
        /// </summary>
        public virtual double TimeAtScreenSpacePosition(Vector2 screenSpacePosition) => HitObjectContainer.TimeAtScreenSpacePosition(screenSpacePosition);

        /// <summary>
        /// Given a time, return the screen space position within this column.
        /// </summary>
        public virtual Vector2 ScreenSpacePositionAtTime(double time) => HitObjectContainer.ScreenSpacePositionAtTime(time);

        protected sealed override HitObjectContainer CreateHitObjectContainer() => new ScrollingHitObjectContainer();
    }
}
