// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Tests
{
    public partial class OsuTestBrowser : OsuGameBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            LoadComponentAsync(new ScreenStack(new BackgroundScreenDefault { Colour = OsuColour.Gray(0.5f) })
            {
                Depth = 10,
                RelativeSizeAxes = Axes.Both,
            }, Add);

            // Have to construct this here, rather than in the constructor, because
            // we depend on some dependencies to be loaded within OsuGameBase.load().
            Add(new TestBrowser());
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            host.Window.CursorState |= CursorState.Hidden;
        }
    }
}
