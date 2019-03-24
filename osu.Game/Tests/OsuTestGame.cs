// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Tests
{
    public abstract class OsuTestGame : OsuGameBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            LoadComponentAsync(new ScreenStack(new BackgroundScreenDefault { Colour = OsuColour.Gray(0.5f) })
            {
                Depth = 10,
                RelativeSizeAxes = Axes.Both,
            }, AddInternal);
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            host.Window.CursorState |= CursorState.Hidden;
        }
    }
}