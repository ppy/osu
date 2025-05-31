// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// The goal of this abstract test class is to exercise correct playback of replays sourced from previous osu! versions.
    /// Use <see cref="RunTest"/> to exercise that property.
    /// </summary>
    [HeadlessTest]
    [TestFixture]
    public abstract partial class LegacyReplayPlaybackTestScene : RateAdjustedBeatmapTestScene
    {
        private ReplayPlayer currentPlayer = null!;
        private readonly List<JudgementResult> results = new List<JudgementResult>();

        /// <summary>
        /// This is provided as a convenience for testing behaviour against osu!stable.
        /// Setting this field to a non-null path will cause beatmap files and replays used in all test cases
        /// to be exported to disk so that they can be cross-checked against stable.
        /// </summary>
        protected abstract string? ExportLocation { get; }

        /// <summary>
        /// Encodes the supplied <paramref name="originalScore"/>, decodes the result of encoding, runs the result of decoding against the supplied <paramref name="beatmap"/>,
        /// and checks that the judgement results recorded still match <paramref name="expectedResults"/>.
        /// If <see cref="ExportLocation"/> is set, exports both the beatmap and the replay to said location.
        /// </summary>
        protected void RunTest(string beatmapName, IBeatmap beatmap, string replayName, Score originalScore, IEnumerable<HitResult> expectedResults)
        {
            IBeatmap playableBeatmap = null!;
            MemoryStream beatmapStream = new MemoryStream();
            MemoryStream scoreStream = new MemoryStream();
            Score decodedScore = null!;

            AddStep(@"set up beatmap", () =>
            {
                beatmap.Metadata.Title = beatmapName;
                Beatmap.Value = CreateWorkingBeatmap(beatmap);
                Ruleset.Value = CreateRuleset()!.RulesetInfo;
                playableBeatmap = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);

                var beatmapEncoder = new LegacyBeatmapEncoder(beatmap, null);

                using (var writer = new StreamWriter(beatmapStream, Encoding.UTF8, leaveOpen: true))
                    beatmapEncoder.Encode(writer);

                beatmapStream.Seek(0, SeekOrigin.Begin);
                playableBeatmap.BeatmapInfo.MD5Hash = beatmapStream.ComputeMD5Hash();
            });

            AddStep(@"encode score", () =>
            {
                originalScore.ScoreInfo.BeatmapInfo = playableBeatmap.BeatmapInfo;
                var encoder = new LegacyScoreEncoder(originalScore, playableBeatmap);
                encoder.Encode(scoreStream, leaveOpen: true);

                // `LegacyScoreEncoder` hardcodes a replay version that belongs to lazer.
                // here we want to simulate a stable replay, which should have the classic mod attached etc.
                // to that end, we do a post-encode step to specify a stable-like replay version.
                scoreStream.Position = 1;

                using (var sw = new SerializationWriter(scoreStream, leaveOpen: true))
                {
                    const int version = 20250414;
                    sw.Write(version);
                }

                scoreStream.Position = 0;
            });

            if (ExportLocation != null)
            {
                AddStep("export beatmap", () =>
                {
                    using var stream = File.Open(Path.Combine(ExportLocation, $"{beatmapName}.osu"), FileMode.Create);
                    beatmapStream.CopyTo(stream);
                    beatmapStream.Position = 0;
                });

                AddStep("export score", () =>
                {
                    using var stream = File.Open(Path.Combine(ExportLocation, $@"{replayName}.osr"), FileMode.Create);
                    scoreStream.CopyTo(stream);
                    scoreStream.Position = 0;
                });
            }

            AddStep(@"decode score", () =>
            {
                using (scoreStream)
                {
                    scoreStream.Position = 0;
                    decodedScore = new TestScoreDecoder(Beatmap.Value, Ruleset.Value).Parse(scoreStream);
                }
            });

            AddAssert(@"classic mod present", () => decodedScore.ScoreInfo.Mods.Any(mod => mod is ModClassic));
            AddStep(@"push player", () => pushNewPlayer(decodedScore));

            AddUntilStep(@"Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddAssert(@"classic mod present", () => currentPlayer.GameplayState.Mods.Any(mod => mod is ModClassic));
            AddUntilStep(@"Wait for completion", () => currentPlayer.GameplayState.HasCompleted);
            AddAssert(@"judgement results after encode are correct", () => results.Select(r => r.Type), () => Is.EquivalentTo(expectedResults));
        }

        private void pushNewPlayer(Score score)
        {
            var player = new ReplayPlayer(score);
            SelectedMods.Value = score.ScoreInfo.Mods;
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
            private readonly Ruleset ruleset;

            public TestScoreDecoder(WorkingBeatmap beatmap, RulesetInfo ruleset)
            {
                this.beatmap = beatmap;
                this.ruleset = ruleset.CreateInstance();
            }

            protected override Ruleset GetRuleset(int rulesetId) => ruleset;
            protected override WorkingBeatmap GetBeatmap(string md5Hash) => beatmap;
        }
    }
}
