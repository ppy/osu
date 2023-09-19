// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// A mapping between two <see cref="TournamentMatch"/>es.
    /// Used for serialisation exclusively.
    /// </summary>
    [Serializable]
    public class TournamentProgression
    {
        public int SourceID;
        public int TargetID;

        public bool Losers;

        public TournamentProgression(int sourceID, int targetID, bool losers = false)
        {
            SourceID = sourceID;
            TargetID = targetID;
            Losers = losers;
        }
    }
}
