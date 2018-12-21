// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Game.Screens.Ranking.Pages;

namespace osu.Game.Screens.Ranking.Types
{
    public interface IResultType
    {
        FontAwesome Icon { get; }

        ResultsPage CreatePage();
    }
}
