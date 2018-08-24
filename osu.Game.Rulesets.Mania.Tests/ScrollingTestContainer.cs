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
        [Cached(Type = typeof(IScrollingInfo))]
        private readonly TestScrollingInfo scrollingInfo = new TestScrollingInfo();

        public ScrollingTestContainer(ScrollingDirection direction)
        {
            scrollingInfo.Direction.Value = direction;
        }

        public void Flip() => scrollingInfo.Direction.Value = scrollingInfo.Direction.Value == ScrollingDirection.Up ? ScrollingDirection.Down : ScrollingDirection.Up;
    }

    public class TestScrollingInfo : IScrollingInfo
    {
        public readonly Bindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();
        IBindable<ScrollingDirection> IScrollingInfo.Direction => Direction;
    }
}
