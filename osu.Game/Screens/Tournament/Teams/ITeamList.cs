// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Screens.Tournament.Teams
{
    public interface ITeamList
    {
        IEnumerable<DrawingsTeam> Teams { get; }
    }
}
