// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class TournamentProgression
    {
        public int SourceID;
        public int TargetID;

        // migration
        public int Item1
        {
            set => SourceID = value;
        }

        public int Item2
        {
            set => TargetID = value;
        }

        public bool Losers;

        public TournamentProgression(int sourceID, int targetID, bool losers = false)
        {
            SourceID = sourceID;
            TargetID = targetID;
            Losers = losers;
        }
    }
}
