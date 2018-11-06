// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Tournament.Tests
{
    public class TournamentTestBrowser : TournamentGame
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            LoadComponentAsync(new BackgroundScreenDefault
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
