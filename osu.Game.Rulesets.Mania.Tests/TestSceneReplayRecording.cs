// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneReplayRecording : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new ManiaBeatmap(new StageDefinition(1))
        {
            HitObjects =
            {
                new Note { StartTime = 0, },
                new Note { StartTime = 5000, },
                new Note { StartTime = 10000, },
                new Note { StartTime = 15000, }
            },
            Difficulty = { CircleSize = 1 },
            BeatmapInfo =
            {
                Ruleset = ruleset,
            }
        };

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null) =>
            new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        [Test]
        public void TestRecording()
        {
            seekTo(0);
            AddStep("press space", () => InputManager.PressKey(Key.Space));
            seekTo(15);
            AddStep("release space", () => InputManager.ReleaseKey(Key.Space));
            AddUntilStep("button press recorded to replay", () => Player.Score.Replay.Frames.OfType<ManiaReplayFrame>().Any(f => f.Actions.SequenceEqual([ManiaAction.Key1])));
        }

        private void seekTo(double time)
        {
            AddStep($"seek to {time}ms", () => Player.GameplayClockContainer.Seek(time));
            AddUntilStep("wait for seek to finish", () => Player.DrawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time).Within(500));
        }
    }
}
