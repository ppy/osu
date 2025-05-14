// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Tests
{
    public partial class TestSceneReplayRecording : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            HitObjects =
            {
                new Fruit { StartTime = 0, },
                new Fruit { StartTime = 5000, },
                new Fruit { StartTime = 10000, },
                new Fruit { StartTime = 15000, }
            }
        };

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null) =>
            new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        [Test]
        public void TestRecording()
        {
            seekTo(0);
            AddStep("start moving left", () => InputManager.PressKey(Key.Left));
            seekTo(5000);
            AddStep("end moving left", () => InputManager.ReleaseKey(Key.Left));
            AddAssert("catcher max left", () => this.ChildrenOfType<Catcher>().Single().X, () => Is.EqualTo(0));
            AddAssert("movement to left recorded to replay", () => Player.Score.Replay.Frames.OfType<CatchReplayFrame>().Any(f => f.Actions.SequenceEqual([CatchAction.MoveLeft])));
            AddAssert("replay reached left edge", () => Player.Score.Replay.Frames.OfType<CatchReplayFrame>().Any(f => Precision.AlmostEquals(f.Position, 0)));

            AddStep("start dashing right", () =>
            {
                InputManager.PressKey(Key.LShift);
                InputManager.PressKey(Key.Right);
            });
            seekTo(10000);
            AddStep("end dashing right", () =>
            {
                InputManager.ReleaseKey(Key.LShift);
                InputManager.ReleaseKey(Key.Right);
            });
            AddAssert("catcher max right", () => this.ChildrenOfType<Catcher>().Single().X, () => Is.EqualTo(CatchPlayfield.WIDTH));
            AddAssert("dash to right recorded to replay", () => Player.Score.Replay.Frames.OfType<CatchReplayFrame>().Any(f => f.Actions.SequenceEqual([CatchAction.Dash, CatchAction.MoveRight])));
            AddAssert("replay reached right edge", () => Player.Score.Replay.Frames.OfType<CatchReplayFrame>().Any(f => Precision.AlmostEquals(f.Position, CatchPlayfield.WIDTH)));
        }

        private void seekTo(double time)
        {
            AddStep($"seek to {time}ms", () => Player.GameplayClockContainer.Seek(time));
            AddUntilStep("wait for seek to finish", () => Player.DrawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time).Within(500));
        }
    }
}
