// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Configuration;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class TournamentGrouping
    {
        public readonly Bindable<string> Name = new Bindable<string>();
        public readonly Bindable<string> Description = new Bindable<string>();

        public int BestOf;

        public List<int> Pairings = new List<int>();
    }
}
