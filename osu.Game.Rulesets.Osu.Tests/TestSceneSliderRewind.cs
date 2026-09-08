// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public partial class TestSceneSliderRewind : RateAdjustedBeatmapTestScene
    {
        private TestReplayPlayer player = null!;

        private bool autoplay;

        [Test]
        public void TestAuto()
        {
            AddStep("enable autoplay", () => autoplay = true);
            createPlayer();
            AddStep("pause", () => player.GameplayClockContainer.Stop());
            addSeekStep("seek after slider", 20000);
            addSeekStep("seek after repeat", 5000);
            AddUntilStep("follow circle is visible", () => player.ChildrenOfType<FollowCircle>().FirstOrDefault()?.IsPresent, () => Is.True);
            addSeekStep("seek before repeat, after tick", 3000);
            AddUntilStep("follow circle is visible", () => player.ChildrenOfType<FollowCircle>().FirstOrDefault()?.IsPresent, () => Is.True);
            addSeekStep("seek before repeat, before tick", 2000);
            AddUntilStep("follow circle is visible", () => player.ChildrenOfType<FollowCircle>().FirstOrDefault()?.IsPresent, () => Is.True);
        }

        [Test]
        public void TestSliderBreakOnTick()
        {
            AddStep("disable autoplay", () => autoplay = false);
            createPlayer([
                new OsuReplayFrame(1500, new Vector2(100, 100), OsuAction.LeftButton),
                new OsuReplayFrame(2000, new Vector2(150, 150)),
                new OsuReplayFrame(3000, new Vector2(250, 250), OsuAction.LeftButton),
                new OsuReplayFrame(3500, new Vector2(300, 300), OsuAction.LeftButton),
                new OsuReplayFrame(5500, new Vector2(100, 100), OsuAction.LeftButton),
            ]);
            AddStep("pause", () => player.GameplayClockContainer.Stop());
            addSeekStep("seek after slider", 20000);
            addSeekStep("seek after sliderbreak", 3200);
            AddUntilStep("follow circle is visible", () => player.ChildrenOfType<FollowCircle>().FirstOrDefault()?.IsPresent, () => Is.True);
            addSeekStep("seek close to sliderbreak", 2500);
            AddUntilStep("follow circle is not visible", () => player.ChildrenOfType<FollowCircle>().FirstOrDefault()?.IsPresent, () => Is.False);
            addSeekStep("seek before sliderbreak", 1800);
            AddUntilStep("follow circle is visible", () => player.ChildrenOfType<FollowCircle>().FirstOrDefault()?.IsPresent, () => Is.True);
        }

        [Test]
        public void TestSliderBreakAfterRepeat()
        {
            AddStep("disable autoplay", () => autoplay = false);
            createPlayer([
                new OsuReplayFrame(1500, new Vector2(100, 100), OsuAction.LeftButton),
                new OsuReplayFrame(3500, new Vector2(300, 300), OsuAction.LeftButton),
                new OsuReplayFrame(4000, new Vector2(250, 250)),
                new OsuReplayFrame(5000, new Vector2(150, 150), OsuAction.LeftButton),
                new OsuReplayFrame(5500, new Vector2(100, 100), OsuAction.LeftButton),
            ]);
            AddStep("pause", () => player.GameplayClockContainer.Stop());
            addSeekStep("seek after slider", 20000);
            addSeekStep("seek after sliderbreak", 5200);
            AddUntilStep("follow circle is visible", () => player.ChildrenOfType<FollowCircle>().FirstOrDefault()?.IsPresent, () => Is.True);
            addSeekStep("seek close to sliderbreak", 4500);
            AddUntilStep("follow circle is not visible", () => player.ChildrenOfType<FollowCircle>().FirstOrDefault()?.IsPresent, () => Is.False);
            addSeekStep("seek before sliderbreak", 3200);
            AddUntilStep("follow circle is visible", () => player.ChildrenOfType<FollowCircle>().FirstOrDefault()?.IsPresent, () => Is.True);
        }

        private void addSeekStep(string description, double time)
        {
            AddStep(description, () => player.GameplayClockContainer.Seek(time));
            AddUntilStep("wait for seek to finish", () => player.DrawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time).Within(100));
        }

        private void createPlayer(List<ReplayFrame>? replayFrames = null)
        {
            AddStep("load player", () =>
            {
                var ruleset = new OsuRuleset();
                Ruleset.Value = ruleset.RulesetInfo;

                var beatmap = CreateBeatmap(ruleset.RulesetInfo);
                Beatmap.Value = CreateWorkingBeatmap(beatmap);

                SelectedMods.Value = Array.Empty<Mod>();

                var noFailMod = ruleset.CreateMod<ModNoFail>();
                if (noFailMod != null)
                    SelectedMods.Value = SelectedMods.Value.Append(noFailMod).ToArray();

                if (autoplay)
                {
                    var mod = ruleset.GetAutoplayMod();
                    if (mod != null)
                        SelectedMods.Value = SelectedMods.Value.Append(mod).ToArray();
                }

                player = replayFrames != null
                    ? new TestReplayPlayer(new Score { Replay = new Replay { Frames = replayFrames } })
                    : new TestReplayPlayer();
                LoadScreen(player);
            });
            AddUntilStep("wait until player is loaded", () => player.IsLoaded && player.Alpha == 1);
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap { HitObjects = createHitObjects() };

        private static List<HitObject> createHitObjects() => new List<HitObject>
        {
            new Slider
            {
                StartTime = 1500,
                Position = new Vector2(100, 100),
                Path = new SliderPath(PathType.PERFECT_CURVE, new[]
                {
                    Vector2.Zero,
                    new Vector2(200, 200),
                }),
                RepeatCount = 1,
            },
            new HitCircle
            {
                StartTime = 30000,
                Position = new Vector2(100, 100),
            },
        };
    }
}
