// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens
{
    public partial class OsuScreenStack : ScreenStack
    {
        [Cached]
        private BackgroundScreenStack backgroundScreenStack;

        private readonly ParallaxContainer parallaxContainer;

        protected float ParallaxAmount => parallaxContainer.ParallaxAmount;

        public OsuScreenStack()
        {
            InternalChild = parallaxContainer = new ParallaxContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = backgroundScreenStack = new BackgroundScreenStack { RelativeSizeAxes = Axes.Both },
            };

            ScreenPushed += screenPushed;
            ScreenExited += ScreenChanged;
        }

        public void PushSynchronously(OsuScreen screen)
        {
            LoadComponent(screen);

            Push(screen);
        }

        private void screenPushed(IScreen prev, IScreen next)
        {
            if (LoadState < LoadState.Ready)
            {
                // dependencies must be present to stay in a sane state.
                // this is generally only ever hit by test scenes.
                Schedule(() => screenPushed(prev, next));
                return;
            }

            // create dependencies synchronously to ensure leases are in a sane state.
            ((OsuScreen)next).CreateLeasedDependencies((prev as OsuScreen)?.Dependencies ?? Dependencies);

            ScreenChanged(prev, next);
        }

        protected virtual void ScreenChanged(IScreen prev, IScreen? next)
        {
            setParallax(next);
        }

        private void setParallax(IScreen? next) =>
            parallaxContainer.ParallaxAmount = ParallaxContainer.DEFAULT_PARALLAX_AMOUNT * ((next as IOsuScreen)?.BackgroundParallaxAmount ?? 1.0f);
    }
}
