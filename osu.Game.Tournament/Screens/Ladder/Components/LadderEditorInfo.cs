// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderEditorInfo
    {
        public readonly BindableBool EditingEnabled = new BindableBool();
        public List<TournamentTeam> Teams = new List<TournamentTeam>();
        public List<TournamentGrouping> Groupings = new List<TournamentGrouping>();
        public readonly Bindable<MatchPairing> Selected = new Bindable<MatchPairing>();
    }
}
