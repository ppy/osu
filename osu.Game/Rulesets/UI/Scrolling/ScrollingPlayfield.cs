// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.UI.Scrolling
{
    /// <summary>
    /// A type of <see cref="Playfield"/> specialized towards scrolling <see cref="DrawableHitObject"/>s.
    /// </summary>
    public abstract class ScrollingPlayfield : Playfield
    {
        protected readonly IBindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Direction.BindTo(scrollingInfo.Direction);
        }

        protected sealed override HitObjectContainer CreateHitObjectContainer() => new ScrollingHitObjectContainer();
    }
}
