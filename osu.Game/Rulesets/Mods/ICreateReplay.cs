// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    [Obsolete("Use ICreateReplayData instead")] // Can be removed 20220929
    public interface ICreateReplay : ICreateReplayData
    {
        public Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods);

        ModReplayData ICreateReplayData.CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods)
        {
            var replayScore = CreateReplayScore(beatmap, mods);
            return new ModReplayData(replayScore.Replay, new ModCreatedUser { Username = replayScore.ScoreInfo.User.Username });
        }
    }
}
