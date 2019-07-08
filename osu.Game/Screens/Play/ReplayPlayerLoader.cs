// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    public class ReplayPlayerLoader : PlayerLoader
    {
        private readonly ScoreInfo scoreInfo;

        public ReplayPlayerLoader(Score score)
            : base(() => new ReplayPlayer(score))
        {
            scoreInfo = score.ScoreInfo;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = base.CreateChildDependencies(parent);

            Mods.Value = scoreInfo.Mods;
            Ruleset.Value = scoreInfo.Ruleset;

            return dependencies;
        }
    }
}
