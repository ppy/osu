// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneGameplayRewinding : PlayerTestScene
    {
        private RulesetExposingPlayer player => (RulesetExposingPlayer)Player;

        [Resolved]
        private AudioManager audioManager { get; set; }

        public TestSceneGameplayRewinding()
            : base(new OsuRuleset())
        {
        }

        private Track track;

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap)
        {
            var working = new ClockBackedTestWorkingBeatmap(beatmap, new FramedClock(new ManualClock { Rate = 1 }), audioManager);
            track = working.Track;
            return working;
        }

        [Test]
        public void TestNoJudgementsOnRewind()
        {
            AddUntilStep("wait for track to start running", () => track.IsRunning);
            addSeekStep(3000);
            AddAssert("all judged", () => player.DrawableRuleset.Playfield.AllHitObjects.All(h => h.Judged));
            AddUntilStep("key counter counted keys", () => player.HUDOverlay.KeyCounter.Children.All(kc => kc.CountPresses >= 7));
            AddStep("clear results", () => player.AppliedResults.Clear());
            addSeekStep(0);
            AddAssert("none judged", () => player.DrawableRuleset.Playfield.AllHitObjects.All(h => !h.Judged));
            AddUntilStep("key counters reset", () => player.HUDOverlay.KeyCounter.Children.All(kc => kc.CountPresses == 0));
            AddAssert("no results triggered", () => player.AppliedResults.Count == 0);
        }

        private void addSeekStep(double time)
        {
            AddStep($"seek to {time}", () => track.Seek(time));

            // Allow a few frames of lenience
            AddUntilStep("wait for seek to finish", () => Precision.AlmostEquals(time, player.DrawableRuleset.FrameStableClock.CurrentTime, 100));
        }

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            Mods.Value = Mods.Value.Concat(new[] { ruleset.GetAutoplayMod() }).ToArray();
            return new RulesetExposingPlayer();
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = { BaseDifficulty = { ApproachRate = 9 } },
            };

            for (int i = 0; i < 15; i++)
            {
                beatmap.HitObjects.Add(new HitCircle
                {
                    Position = new Vector2(256, 192),
                    StartTime = 1000 + 30 * i
                });
            }

            return beatmap;
        }

        private class RulesetExposingPlayer : Player
        {
            public readonly List<JudgementResult> AppliedResults = new List<JudgementResult>();

            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public new HUDOverlay HUDOverlay => base.HUDOverlay;

            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

            public new DrawableRuleset DrawableRuleset => base.DrawableRuleset;

            public RulesetExposingPlayer()
                : base(false, false)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                ScoreProcessor.NewJudgement += r => AppliedResults.Add(r);
            }
        }
    }
}
