// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// The goal of this abstract test class is to ensure that the process of exporting of a replay does not affect its playback.
    /// Use <see cref="RunTest"/> to exercise that property.
    /// </summary>
    [HeadlessTest]
    [TestFixture]
    public abstract partial class ReplayStabilityTestScene : RateAdjustedBeatmapTestScene
    {
        private ReplayPlayer currentPlayer = null!;
        private readonly List<JudgementResult> results = new List<JudgementResult>();

        /// <summary>
        /// Runs <paramref name="replay"/> against the supplied <paramref name="beatmap"/>
        /// and checks that the judgement results recorded match <paramref name="expectedResults"/>.
        /// Then, encodes the <paramref name="replay"/>, decodes the result of encoding, runs the result of decoding against the supplied <paramref name="beatmap"/>,
        /// and checks that the judgement results recorded still match <paramref name="expectedResults"/>.
        /// </summary>
        protected void RunTest(IBeatmap beatmap, Replay replay, IEnumerable<HitResult> expectedResults)
        {
            Score originalScore = null!;
            Score decodedScore = null!;

            AddStep(@"create replay", () => originalScore = new Score
            {
                Replay = replay,
                ScoreInfo = new ScoreInfo()
            });

            AddStep(@"set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(beatmap));
            AddStep(@"set ruleset", () => Ruleset.Value = beatmap.BeatmapInfo.Ruleset);
            AddStep(@"push player", () => pushNewPlayer(originalScore));

            AddUntilStep(@"wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep(@"wait for completion", () => currentPlayer.GameplayState.HasCompleted);
            AddAssert(@"judgement results before encode are correct", () => results.Select(r => r.Type), () => Is.EquivalentTo(expectedResults));

            AddStep(@"exit player", () => currentPlayer.Exit());

            AddStep(@"encode and decode score", () =>
            {
                var encoder = new LegacyScoreEncoder(originalScore, beatmap);

                using (var stream = new MemoryStream())
                {
                    encoder.Encode(stream, leaveOpen: true);
                    stream.Position = 0;
                    decodedScore = new TestScoreDecoder(Beatmap.Value).Parse(stream);
                }
            });

            AddStep(@"push player", () => pushNewPlayer(decodedScore));

            AddUntilStep(@"Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep(@"Wait for completion", () => currentPlayer.GameplayState.HasCompleted);
            AddAssert(@"judgement results after encode are correct", () => results.Select(r => r.Type), () => Is.EquivalentTo(expectedResults));
        }

        private void pushNewPlayer(Score score)
        {
            var player = new ReplayPlayer(score);
            player.OnLoadComplete += _ =>
            {
                player.GameplayState.ScoreProcessor.NewJudgement += result =>
                {
                    if (currentPlayer == player)
                        results.Add(result);
                };
            };
            LoadScreen(currentPlayer = player);
            results.Clear();
        }

        private class TestScoreDecoder : LegacyScoreDecoder
        {
            private readonly WorkingBeatmap beatmap;

            public TestScoreDecoder(WorkingBeatmap beatmap)
            {
                this.beatmap = beatmap;
            }

            protected override Ruleset GetRuleset(int rulesetId) => beatmap.BeatmapInfo.Ruleset.CreateInstance();
            protected override WorkingBeatmap GetBeatmap(string md5Hash) => beatmap;
        }
    }
}
