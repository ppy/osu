// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;

namespace osu.Game.Screens.Ranking
{
    public interface IResultPageInfo
    {
        FontAwesome Icon { get; }

        string Name { get; }

        ResultsPage CreatePage();
    }
}
