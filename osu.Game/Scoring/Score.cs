// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Replays;

namespace osu.Game.Scoring
{
    public class Score
    {
        public ScoreInfo ScoreInfo = new ScoreInfo();
        public Replay Replay = new Replay();
    }
}
