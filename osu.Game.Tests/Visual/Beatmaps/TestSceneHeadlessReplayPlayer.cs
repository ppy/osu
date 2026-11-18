// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public partial class TestSceneHeadlessReplayPlayer : OsuTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("clear", Clear);
            AddStep("import beatmaps", () => beatmaps.Import(TestResources.GetTestBeatmapForImport()).WaitSafely());
        }

        [Test]
        public void TestBasic()
        {
            Score score = null!;
            bool completed = false;

            AddStep("import replay", () =>
            {
                using (var replayStream = TestResources.OpenResource("Replays/osu-renatus-replay.osr"))
                    score = new DatabasedLegacyScoreDecoder(rulesets, beatmaps).Parse(replayStream);
            });
            AddStep("create player", () =>
            {
                completed = false;

                // Component isn't hidden in tests to allow for visually checking if everything works correctly.
                LoadComponentAsync(new HeadlessReplayPlayer(score, beatmaps.GetWorkingBeatmap(score.ScoreInfo.BeatmapInfo))
                {
                    Width = 1024,
                    Height = 768,
                    PlaybackCompleted = _ => completed = true,
                }, Add);
            });
            AddUntilStep("wait until completed", () => completed, () => Is.True);
            AddAssert("hit events exist", () => score.ScoreInfo.HitEvents.Any());
        }

        [Test]
        public void TestFailed()
        {
            Score score = null!;
            bool completed = false;

            AddStep("import replay", () =>
            {
                using (var replayStream = TestResources.OpenResource("Replays/osu-renatus-replay-failed.osr"))
                    score = new DatabasedLegacyScoreDecoder(rulesets, beatmaps).Parse(replayStream);
            });
            AddStep("create player", () =>
            {
                completed = false;

                LoadComponentAsync(new HeadlessReplayPlayer(score, beatmaps.GetWorkingBeatmap(score.ScoreInfo.BeatmapInfo))
                {
                    Width = 1024,
                    Height = 768,
                    PlaybackCompleted = _ => completed = true,
                }, Add);
            });
            AddUntilStep("wait until completed", () => completed, () => Is.True);
            AddAssert("hit events exist", () => score.ScoreInfo.HitEvents.Any());
        }
    }
}
