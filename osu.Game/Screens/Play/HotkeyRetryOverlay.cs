// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using OpenTK.Input;
using osu.Framework.Allocation;
using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class HotkeyRetryOverlay : Container
    {
        public Action Action;

        private Box overlay;

        private const int activate_delay = 400;
        private const int fadeout_delay = 200;

        private bool fired;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;

            Children = new Drawable[]
            {
                overlay = new Box
                {
                    Alpha = 0,
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            if (args.Key == Key.Tilde)
            {
                overlay.FadeIn(activate_delay, Easing.Out);
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (args.Key == Key.Tilde && !fired)
            {
                overlay.FadeOut(fadeout_delay, Easing.Out);
                return true;
            }

            return base.OnKeyUp(state, args);
        }

        protected override void Update()
        {
            base.Update();
            if (!fired && overlay.Alpha == 1)
            {
                fired = true;
                Action?.Invoke();
            }
        }
    }
}
