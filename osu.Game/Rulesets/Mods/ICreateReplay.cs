// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public interface ICreateReplay
    {
        public Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods);
    }
}
