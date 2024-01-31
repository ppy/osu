// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Tests.Visual
{
    public partial class TestSceneLegacyHitWindowEdges : RateAdjustedBeatmapTestScene
    {
        private static readonly List<double> input_edge_deltas = new List<double> { 1.0, 0.7, 0.50001, 0.5, 0.49999, 0.2, 0.0 };

        // ground-truth osu!stable judgement results (except for 0.5, which is intentionally different for simplicity and consistency)
        private readonly List<HitResult> correctResults = new List<HitResult> { HitResult.Great, HitResult.Great, HitResult.Great, HitResult.Great, HitResult.Ok, HitResult.Ok, HitResult.Ok };

        private readonly List<HitResult> correctResultsMania = new List<HitResult>
            { HitResult.Great, HitResult.Great, HitResult.Great, HitResult.Great, HitResult.Good, HitResult.Good, HitResult.Good };

        private static readonly int hit_objects_count = input_edge_deltas.Count * 2;

        private static readonly Vector2 circle_position = Vector2.Zero;
        private const double step = 100.0;

        private readonly List<JudgementResult> judgementResults = new List<JudgementResult>();

        private ScoreAccessibleReplayPlayer currentPlayer = null!;

        [Test]
        public void TestGameplayOsu()
        {
            performOsuTest(10.0f);
        }

        [Test]
        public void TestGameplayOsuFractionalOd()
        {
            performOsuTest(9.7f);
        }

        [Test]
        public void TestGameplayTaiko()
        {
            performTaikoTest(10.0f);
        }

        [Test]
        public void TestGameplayTaikoFractionalOd()
        {
            performTaikoTest(9.7f);
        }

        [Test]
        public void TestGameplayMania()
        {
            performManiaTest(10.0f);
        }

        [Test]
        public void TestGameplayManiaFractionalOd()
        {
            performManiaTest(9.7f);
        }

        private static double getStableIntegral300HitWindow(HitWindows hitWindows, float overallDifficulty)
        {
            hitWindows.SetDifficulty(overallDifficulty);
            double hitWindow300 = hitWindows.WindowFor(HitResult.Great);
            hitWindow300 = Math.Floor(hitWindow300); // osu!stable floored hit windows, therefore in order to get ground-truth results for this test, this hit window must also be floored; this is particularly relevant for fractional ODs

            return hitWindow300;
        }

        private static List<ReplayFrame> generateOsuFrames(float overallDifficulty)
        {
            double hitWindow300 = getStableIntegral300HitWindow(new OsuStableHitWindows(), overallDifficulty);

            List<ReplayFrame> frames = new List<ReplayFrame>();

            // late hits
            for (int i = 0; i < hit_objects_count / 2; i++)
            {
                frames.Add(new OsuReplayFrame((i + 1) * step + hitWindow300 - input_edge_deltas[i], circle_position, OsuAction.LeftButton));
                frames.Add(new OsuReplayFrame(frames[^1].Time, ((OsuReplayFrame)frames[^1]).Position));
            }

            // early hits
            for (int i = hit_objects_count / 2; i < hit_objects_count; i++)
            {
                frames.Add(new OsuReplayFrame((i + 1) * step - hitWindow300 + input_edge_deltas[i - hit_objects_count / 2], circle_position, OsuAction.LeftButton));
                frames.Add(new OsuReplayFrame(frames[^1].Time, ((OsuReplayFrame)frames[^1]).Position));
            }

            return frames;
        }

        private static List<ReplayFrame> generateTaikoFrames(float overallDifficulty)
        {
            double hitWindow300 = getStableIntegral300HitWindow(new TaikoStableHitWindows(), overallDifficulty);

            List<ReplayFrame> frames = new List<ReplayFrame>();

            // late hits
            for (int i = 0; i < hit_objects_count / 2; i++)
            {
                frames.Add(new TaikoReplayFrame((i + 1) * step + hitWindow300 - input_edge_deltas[i], TaikoAction.LeftCentre));
                frames.Add(new TaikoReplayFrame(frames[^1].Time));
            }

            // early hits
            for (int i = hit_objects_count / 2; i < hit_objects_count; i++)
            {
                frames.Add(new TaikoReplayFrame((i + 1) * step - hitWindow300 + input_edge_deltas[i - hit_objects_count / 2], TaikoAction.LeftCentre));
                frames.Add(new TaikoReplayFrame(frames[^1].Time));
            }

            return frames;
        }

        private static List<ReplayFrame> generateManiaFrames(float overallDifficulty)
        {
            double
                hitWindow300 = getStableIntegral300HitWindow(new ManiaStableHitWindows(), overallDifficulty)
                               + 1; // in osu!stable, mania, unlike standard and taiko, used a <= comparison instead of a < comparison to make a judgement; since both sides of the comparison were integers, this is the same as a < comparison with 1 added to the right hand side; this is equivalent to using a 1ms wider hit window

            List<ReplayFrame> frames = new List<ReplayFrame>();

            // late hits
            for (int i = 0; i < hit_objects_count / 2; i++)
            {
                frames.Add(new ManiaReplayFrame((i + 1) * step + hitWindow300 - input_edge_deltas[i], ManiaAction.Key1));
                frames.Add(new ManiaReplayFrame(frames[^1].Time));
            }

            // early hits
            for (int i = hit_objects_count / 2; i < hit_objects_count; i++)
            {
                frames.Add(new ManiaReplayFrame((i + 1) * step - hitWindow300 + input_edge_deltas[i - hit_objects_count / 2], ManiaAction.Key1));
                frames.Add(new ManiaReplayFrame(frames[^1].Time));
            }

            return frames;
        }

        private static List<OsuHitObject> generateOsuHitObjects()
        {
            List<OsuHitObject> hitObjects = new List<OsuHitObject>();

            for (int i = 1; i <= hit_objects_count; i++)
            {
                hitObjects.Add(new HitCircle
                {
                    StartTime = i * step,
                    Position = circle_position
                });
            }

            return hitObjects;
        }

        private static List<TaikoHitObject> generateTaikoHitObjects()
        {
            List<TaikoHitObject> hitObjects = new List<TaikoHitObject>();

            for (int i = 1; i <= hit_objects_count; i++)
            {
                hitObjects.Add(new Hit
                {
                    StartTime = i * step
                });
            }

            return hitObjects;
        }

        private static List<ManiaHitObject> generateManiaHitObjects()
        {
            List<ManiaHitObject> hitObjects = new List<ManiaHitObject>();

            for (int i = 1; i <= hit_objects_count; i++)
            {
                hitObjects.Add(new Note
                {
                    StartTime = i * step
                });
            }

            return hitObjects;
        }

        private void performOsuTest(float overallDifficulty)
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap<OsuHitObject>
                {
                    HitObjects = generateOsuHitObjects(),
                    BeatmapInfo =
                    {
                        Difficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = overallDifficulty
                        },
                        Ruleset = new OsuRuleset().RulesetInfo,
                        StackLeniency = 0.0001f
                    }
                });

                var p = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = generateOsuFrames(overallDifficulty) } });
                SelectedMods.Value = new[] { new OsuModClassic() };

                p.OnLoadComplete += _ =>
                {
                    p.ScoreProcessor.NewJudgement += result =>
                    {
                        if (currentPlayer == p) judgementResults.Add(result);
                    };
                };

                LoadScreen(currentPlayer = p);
                judgementResults.Clear();
            });
            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep("Wait for completion", () => currentPlayer.ScoreProcessor.HasCompleted.Value);

            for (int i = 0; i < hit_objects_count; i++) assertJudgement(i);
        }

        private void performTaikoTest(float overallDifficulty)
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap<TaikoHitObject>
                {
                    HitObjects = generateTaikoHitObjects(),
                    BeatmapInfo =
                    {
                        Difficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = overallDifficulty
                        },
                        Ruleset = new TaikoRuleset().RulesetInfo,
                        StackLeniency = 0.0001f
                    }
                });

                var p = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = generateTaikoFrames(overallDifficulty) } });
                SelectedMods.Value = new[] { new TaikoModClassic() };

                p.OnLoadComplete += _ =>
                {
                    p.ScoreProcessor.NewJudgement += result =>
                    {
                        if (currentPlayer == p) judgementResults.Add(result);
                    };
                };

                LoadScreen(currentPlayer = p);
                judgementResults.Clear();
            });
            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep("Wait for completion", () => currentPlayer.ScoreProcessor.HasCompleted.Value);

            for (int i = 0; i < hit_objects_count; i++) assertJudgement(i);
        }

        private void performManiaTest(float overallDifficulty)
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap<ManiaHitObject>
                {
                    HitObjects = generateManiaHitObjects(),
                    BeatmapInfo =
                    {
                        Difficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = overallDifficulty
                        },
                        Ruleset = new ManiaRuleset().RulesetInfo,
                        StackLeniency = 0.0001f
                    }
                });

                var p = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = generateManiaFrames(overallDifficulty) } });
                SelectedMods.Value = new[] { new ManiaModClassic() };

                p.OnLoadComplete += _ =>
                {
                    p.ScoreProcessor.NewJudgement += result =>
                    {
                        if (currentPlayer == p) judgementResults.Add(result);
                    };
                };

                LoadScreen(currentPlayer = p);
                judgementResults.Clear();
            });
            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep("Wait for completion", () => currentPlayer.ScoreProcessor.HasCompleted.Value);

            for (int i = 0; i < hit_objects_count; i++) assertJudgementMania(i);
        }

        private void assertJudgement(int i)
        {
            AddAssert(
                $"check judgement no. {i}",
                () => judgementResults[i].Type,
                () => Is.EqualTo(correctResults[i % (hit_objects_count / 2)]));
        }

        private void assertJudgementMania(int i)
        {
            AddAssert(
                $"check judgement no. {i}",
                () => judgementResults[i].Type,
                () => Is.EqualTo(correctResultsMania[i % (hit_objects_count / 2)]));
        }

        private class OsuStableHitWindows : HitWindows
        {
            internal static readonly HitWindowRange[] OSU_STABLE_RANGES =
            {
                new HitWindowRange(HitResult.Great, 80, 50, 20),
                new HitWindowRange(HitResult.Ok, 140, 100, 60),
                new HitWindowRange(HitResult.Meh, 200, 150, 100),
                new HitWindowRange(HitResult.Miss, 400, 400, 400)
            };

            protected override HitWindowRange[] GetRanges()
            {
                return OSU_STABLE_RANGES;
            }
        }

        private class TaikoStableHitWindows : HitWindows
        {
            internal static readonly HitWindowRange[] TAIKO_STABLE_RANGES =
            {
                new HitWindowRange(HitResult.Great, 50, 35, 20),
                new HitWindowRange(HitResult.Ok, 120, 80, 50),
                new HitWindowRange(HitResult.Miss, 135, 95, 70)
            };

            protected override HitWindowRange[] GetRanges()
            {
                return TAIKO_STABLE_RANGES;
            }
        }

        private class ManiaStableHitWindows : HitWindows
        {
            internal static readonly HitWindowRange[]
                MANIA_STABLE_NOMOD_RANGES = // will not work as in-game on non-100% speed rate, since in-game, osu!mania keeps OD hit windows constant with speed rate
                {
                    new HitWindowRange(HitResult.Perfect, 16, 16, 16),
                    new HitWindowRange(HitResult.Great, 64, 49, 34),
                    new HitWindowRange(HitResult.Good, 97, 82, 67),
                    new HitWindowRange(HitResult.Ok, 127, 112, 97),
                    new HitWindowRange(HitResult.Meh, 151, 136, 121),
                    new HitWindowRange(HitResult.Miss, 188, 173, 158)
                };

            protected override HitWindowRange[] GetRanges()
            {
                return MANIA_STABLE_NOMOD_RANGES;
            }
        }

        private partial class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            protected override bool PauseOnFocusLost => false;

            public ScoreAccessibleReplayPlayer(Score score)
                : base(score, new PlayerConfiguration
                {
                    AllowPause = false,
                    ShowResults = false
                })
            {
            }
        }
    }
}
