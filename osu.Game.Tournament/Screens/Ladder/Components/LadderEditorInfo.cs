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
