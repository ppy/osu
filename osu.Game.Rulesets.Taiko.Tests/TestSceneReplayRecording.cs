// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public partial class TestSceneReplayRecording : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new TaikoRuleset();

        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            HitObjects =
            {
                new Hit { StartTime = 0, },
                new Hit { StartTime = 5000, },
                new Hit { StartTime = 10000, },
                new Hit { StartTime = 15000, }
            }
        };

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null) =>
            new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        [Test]
        public void TestRecording()
        {
            seekTo(0);
            AddStep("press D", () => InputManager.PressKey(Key.D));
            seekTo(15);
            AddStep("release D", () => InputManager.ReleaseKey(Key.D));
            AddAssert("left rim press recorded to replay", () => Player.Score.Replay.Frames.OfType<TaikoReplayFrame>().Any(f => f.Actions.SequenceEqual([TaikoAction.LeftRim])));

            seekTo(5000);
            AddStep("press F", () => InputManager.PressKey(Key.F));
            seekTo(5015);
            AddStep("release F", () => InputManager.ReleaseKey(Key.F));
            AddAssert("left centre press recorded to replay", () => Player.Score.Replay.Frames.OfType<TaikoReplayFrame>().Any(f => f.Actions.SequenceEqual([TaikoAction.LeftCentre])));

            seekTo(10000);
            AddStep("press J", () => InputManager.PressKey(Key.J));
            seekTo(10015);
            AddStep("release J", () => InputManager.ReleaseKey(Key.J));
            AddAssert("right centre press recorded to replay", () => Player.Score.Replay.Frames.OfType<TaikoReplayFrame>().Any(f => f.Actions.SequenceEqual([TaikoAction.RightCentre])));

            seekTo(15000);
            AddStep("press K", () => InputManager.PressKey(Key.K));
            seekTo(15015);
            AddStep("release K", () => InputManager.ReleaseKey(Key.K));
            AddAssert("right rim press recorded to replay", () => Player.Score.Replay.Frames.OfType<TaikoReplayFrame>().Any(f => f.Actions.SequenceEqual([TaikoAction.RightRim])));
        }

        private void seekTo(double time)
        {
            AddStep($"seek to {time}ms", () => Player.GameplayClockContainer.Seek(time));
            AddUntilStep("wait for seek to finish", () => Player.DrawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time).Within(500));
        }
    }
}
