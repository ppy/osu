// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens
{
    public class BackgroundScreenStack : CompositeDrawable
    {
        public BackgroundScreen Current => (BackgroundScreen)stack.CurrentScreen;

        private readonly ParallaxContainer parallax;
        private readonly ScreenStack stack;

        public BackgroundScreenStack()
        {
            InternalChild = parallax = new ParallaxContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = stack = new ScreenStack
                {
                    RelativeSizeAxes = Axes.Both,
                    Scale = new Vector2(1.06f)
                }
            };
        }

        public float ParallaxAmount { set => parallax.ParallaxAmount = ParallaxContainer.DEFAULT_PARALLAX_AMOUNT * value; }

        public void Push(BackgroundScreen screen)
        {
            if (screen == null)
                return;

            if (EqualityComparer<BackgroundScreen>.Default.Equals(Current, screen))
                return;

            stack.Push(screen);
        }

        public void Exit(BackgroundScreen screen)
        {
            if (stack.CurrentScreen == screen)
                stack.Exit();
        }
    }
}