// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Tests
{
    /// <summary>
    /// A container which provides a <see cref="ScrollingInfo"/> to children.
    /// </summary>
    public class ScrollingTestContainer : Container
    {
        private readonly ScrollingInfo scrollingInfo;

        public ScrollingTestContainer(ScrollingInfo scrollingInfo)
        {
            this.scrollingInfo = scrollingInfo;
        }

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));
            dependencies.Cache(scrollingInfo);
            return dependencies;
        }
    }
}
