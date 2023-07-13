// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Drawings.Components
{
    public interface ITeamList
    {
        IEnumerable<TournamentTeam> Teams { get; }
    }
}
