// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneReplayRecording : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            HitObjects =
            {
                new HitCircle
                {
                    Position = OsuPlayfield.BASE_SIZE / 2,
                    StartTime = 0,
                },
                new HitCircle
                {
                    Position = OsuPlayfield.BASE_SIZE / 2,
                    StartTime = 5000,
                },
                new HitCircle
                {
                    Position = OsuPlayfield.BASE_SIZE / 2,
                    StartTime = 10000,
                },
                new HitCircle
                {
                    Position = OsuPlayfield.BASE_SIZE / 2,
                    StartTime = 15000,
                }
            }
        };

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null) =>
            new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        [Test]
        public void TestRecording()
        {
            seekTo(0);
            AddStep("move cursor to circle", () => InputManager.MoveMouseTo(Player.DrawableRuleset.Playfield.HitObjectContainer.AliveObjects.Single()));
            AddStep("press X", () => InputManager.Key(Key.X));
            AddAssert("right button press recorded to replay", () => Player.Score.Replay.Frames.OfType<OsuReplayFrame>().Any(f => f.Actions.SequenceEqual([OsuAction.RightButton])));

            seekTo(5000);
            AddStep("move cursor to circle", () => InputManager.MoveMouseTo(Player.DrawableRuleset.Playfield.HitObjectContainer.AliveObjects.Single()));
            AddStep("press Z", () => InputManager.Key(Key.Z));
            AddAssert("left button press recorded to replay", () => Player.Score.Replay.Frames.OfType<OsuReplayFrame>().Any(f => f.Actions.SequenceEqual([OsuAction.LeftButton])));

            seekTo(10000);
            AddStep("move cursor to circle", () => InputManager.MoveMouseTo(Player.DrawableRuleset.Playfield.HitObjectContainer.AliveObjects.Single()));
            AddStep("press C", () => InputManager.Key(Key.C));
            AddAssert("smoke button press recorded to replay", () => Player.Score.Replay.Frames.OfType<OsuReplayFrame>().Any(f => f.Actions.SequenceEqual([OsuAction.Smoke])));
        }

        private void seekTo(double time)
        {
            AddStep($"seek to {time}ms", () => Player.GameplayClockContainer.Seek(time));
            AddUntilStep("wait for seek to finish", () => Player.DrawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time).Within(500));
        }
    }
}
