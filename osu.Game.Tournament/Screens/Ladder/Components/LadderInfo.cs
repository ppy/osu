using System.Collections.Generic;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderInfo
    {
        public List<MatchPairing> Pairings = new List<MatchPairing>();
        public List<(int, int)> Progressions = new List<(int, int)>();
    }
}
