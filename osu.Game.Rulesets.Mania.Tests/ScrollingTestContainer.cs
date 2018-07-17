// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Tests
{
    /// <summary>
    /// A container which provides a <see cref="IScrollingInfo"/> to children.
    /// </summary>
    public class ScrollingTestContainer : Container
    {
        private readonly ScrollingDirection direction;

        public ScrollingTestContainer(ScrollingDirection direction)
        {
            this.direction = direction;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs<IScrollingInfo>(new ScrollingInfo { Direction = { Value = direction }});
            return dependencies;
        }

        private class ScrollingInfo : IScrollingInfo
        {
            public readonly Bindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();
            IBindable<ScrollingDirection> IScrollingInfo.Direction => Direction;
        }
    }
}
