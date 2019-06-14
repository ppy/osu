// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament
{
    public class LadderInfo
    {
        public List<MatchPairing> Pairings = new List<MatchPairing>();
        public List<TournamentProgression> Progressions = new List<TournamentProgression>();
        public BindableList<TournamentGrouping> Groupings = new BindableList<TournamentGrouping>();
        public List<TournamentTeam> Teams = new List<TournamentTeam>();

        [JsonIgnore]
        public Bindable<MatchPairing> CurrentMatch = new Bindable<MatchPairing>();
    }
}
