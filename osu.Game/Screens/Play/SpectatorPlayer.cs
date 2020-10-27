// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    public class SpectatorPlayer : ReplayPlayer
    {
        public SpectatorPlayer(Score score)
            : base(score)
        {
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
        {
            // if we already have frames, start gameplay at the point in time they exist, should they be too far into the beatmap.
            double? firstFrameTime = Score.Replay.Frames.FirstOrDefault()?.Time;

            if (firstFrameTime == null || firstFrameTime <= gameplayStart + 5000)
                return base.CreateGameplayClockContainer(beatmap, gameplayStart);

            return new GameplayClockContainer(beatmap, firstFrameTime.Value, true);
        }
    }
}
