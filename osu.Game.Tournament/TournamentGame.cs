// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Screens;

namespace osu.Game.Tournament
{
    public class TournamentGame : TournamentGameBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new TournamentSceneManager()
            });

            MenuCursorContainer.Cursor.Alpha = 0;
        }
    }
}
