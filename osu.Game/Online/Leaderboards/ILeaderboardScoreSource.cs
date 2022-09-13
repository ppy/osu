// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Scoring;

namespace osu.Game.Online.Leaderboards
{
    [Cached]
    public interface ILeaderboardScoreSource
    {
        IBindableList<ScoreInfo> Scores { get; }
    }
}
