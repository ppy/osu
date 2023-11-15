// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Game.Replays;
using osu.Game.Utils;

namespace osu.Game.Scoring
{
    public class Score : IDeepCloneable<Score>
    {
        public ScoreInfo ScoreInfo = new ScoreInfo();
        public Replay Replay = new Replay();

        public Score DeepClone()
        {
            return new Score
            {
                ScoreInfo = ScoreInfo.DeepClone(),
                Replay = Replay.DeepClone(),
            };
        }

        Score IDeepCloneable<Score>.DeepClone(IDictionary<object, object> referenceLookup)
        {
            return DeepClone();
        }
    }
}
