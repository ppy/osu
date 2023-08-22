// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Judgements
{
    public partial class JudgementTest : RateAdjustedBeatmapTestScene
    {
        private ScoreAccessibleReplayPlayer currentPlayer = null!;
        protected List<JudgementResult> JudgementResults { get; private set; } = null!;

        protected void AssertJudgementCount(int count)
        {
            AddAssert($"{count} judgement{(count > 0 ? "s" : "")}", () => JudgementResults, () => Has.Count.EqualTo(count));
        }

        protected void AssertResult<T>(int index, HitResult expectedResult)
        {
            AddAssert($"{typeof(T).ReadableName()} ({index}) judged as {expectedResult}",
                () => JudgementResults.Where(j => j.HitObject is T).OrderBy(j => j.HitObject.StartTime).ElementAt(index).Type,
                () => Is.EqualTo(expectedResult));
        }

        protected void PerformTest(List<ReplayFrame> frames, Beatmap<TaikoHitObject>? beatmap = null, Mod[]? mods = null)
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(beatmap);
                SelectedMods.Value = mods ?? Array.Empty<Mod>();

                var p = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = frames } });

                p.OnLoadComplete += _ =>
                {
                    p.ScoreProcessor.NewJudgement += result =>
                    {
                        if (currentPlayer == p) JudgementResults.Add(result);
                    };
                };

                LoadScreen(currentPlayer = p);
                JudgementResults = new List<JudgementResult>();
            });

            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep("Wait for completion", () => currentPlayer.ScoreProcessor.HasCompleted.Value);
        }

        protected Beatmap<TaikoHitObject> CreateBeatmap(params TaikoHitObject[] hitObjects)
        {
            var beatmap = new Beatmap<TaikoHitObject>
            {
                HitObjects = hitObjects.ToList(),
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty { SliderTickRate = 4 },
                    Ruleset = new TaikoRuleset().RulesetInfo
                },
            };

            beatmap.ControlPointInfo.Add(0, new EffectControlPoint { ScrollSpeed = 0.1f });
            return beatmap;
        }

        private partial class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            protected override bool PauseOnFocusLost => false;

            public ScoreAccessibleReplayPlayer(Score score)
                : base(score, new PlayerConfiguration
                {
                    AllowPause = false,
                    ShowResults = false,
                })
            {
            }
        }
    }
}
