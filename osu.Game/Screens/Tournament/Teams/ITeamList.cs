// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Users;

namespace osu.Game.Screens.Tournament.Teams
{
    public interface ITeamList
    {
        IEnumerable<Country> Teams { get; }
    }
}
