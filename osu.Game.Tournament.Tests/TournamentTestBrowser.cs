// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Tournament.Tests
{
    public class TournamentTestBrowser : TournamentGameBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            LoadComponentAsync(new Background("Menu/menu-background-0")
            {
                Colour = OsuColour.Gray(0.5f),
                Depth = 10
            }, AddInternal);

            // Have to construct this here, rather than in the constructor, because
            // we depend on some dependencies to be loaded within OsuGameBase.load().
            Add(new TestBrowser());
        }
    }
}
