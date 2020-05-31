// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Utils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Storyboards;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneGameplayRewinding : PlayerTestScene
    {
        [Resolved]
        private AudioManager audioManager { get; set; }

        public TestSceneGameplayRewinding()
            : base(new OsuRuleset())
        {
        }

        private Track track;

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
        {
            var working = new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);
            track = working.Track;
            return working;
        }

        [Test]
        public void TestNoJudgementsOnRewind()
        {
            AddUntilStep("wait for track to start running", () => track.IsRunning);
            addSeekStep(3000);
            AddAssert("all judged", () => Player.DrawableRuleset.Playfield.AllHitObjects.All(h => h.Judged));
            AddUntilStep("key counter counted keys", () => Player.HUDOverlay.KeyCounter.Children.All(kc => kc.CountPresses >= 7));
            AddStep("clear results", () => Player.Results.Clear());
            addSeekStep(0);
            AddAssert("none judged", () => Player.DrawableRuleset.Playfield.AllHitObjects.All(h => !h.Judged));
            AddUntilStep("key counters reset", () => Player.HUDOverlay.KeyCounter.Children.All(kc => kc.CountPresses == 0));
            AddAssert("no results triggered", () => Player.Results.Count == 0);
        }

        private void addSeekStep(double time)
        {
            AddStep($"seek to {time}", () => track.Seek(time));

            // Allow a few frames of lenience
            AddUntilStep("wait for seek to finish", () => Precision.AlmostEquals(time, Player.DrawableRuleset.FrameStableClock.CurrentTime, 100));
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Concat(new[] { ruleset.GetAutoplayMod() }).ToArray();
            return base.CreatePlayer(ruleset);
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
    }
}
