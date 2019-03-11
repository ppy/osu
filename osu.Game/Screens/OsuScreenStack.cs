// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens
{
    public class OsuScreenStack : ScreenStack
    {
        [Cached]
        private BackgroundScreenStack backgroundScreenStack;

        public OsuScreenStack(IScreen baseScreen)
            : base(baseScreen)
        {
            backgroundScreenStack = new BackgroundScreenStack { RelativeSizeAxes = Axes.Both };
            InternalChild = new ParallaxContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = backgroundScreenStack,
            };
        }
    }
}
