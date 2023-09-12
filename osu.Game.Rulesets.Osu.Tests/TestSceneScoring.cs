// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual.Gameplay;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public partial class TestSceneScoring : ScoringTestScene
    {
        protected override ScoreProcessor CreateScoreProcessor() => new OsuScoreProcessor();

        protected override IBeatmap CreateBeatmap(int maxCombo)
        {
            var beatmap = new OsuBeatmap();
            for (int i = 0; i < maxCombo; i++)
                beatmap.HitObjects.Add(new HitCircle());
            return beatmap;
        }

        protected override JudgementResult CreatePerfectJudgementResult() => new OsuJudgementResult(new HitCircle(), new OsuJudgement()) { Type = HitResult.Great };
        protected override JudgementResult CreateNonPerfectJudgementResult() => new OsuJudgementResult(new HitCircle(), new OsuJudgement()) { Type = HitResult.Ok };
        protected override JudgementResult CreateMissJudgementResult() => new OsuJudgementResult(new HitCircle(), new OsuJudgement()) { Type = HitResult.Miss };
    }
}
