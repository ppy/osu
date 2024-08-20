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
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Play;
using osu.Game.Storyboards;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneUnorderedBreaks : OsuPlayerTestScene
    {
        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new OsuBeatmap();
            beatmap.HitObjects.Add(new HitCircle { StartTime = 0 });
            beatmap.HitObjects.Add(new HitCircle { StartTime = 5000 });
            beatmap.HitObjects.Add(new HitCircle { StartTime = 10000 });
            beatmap.Breaks.Add(new BreakPeriod(6000, 9000));
            beatmap.Breaks.Add(new BreakPeriod(1000, 4000));
            return beatmap;
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null) =>
            new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
        }

        [Test]
        public void TestBreakOverlayVisibility()
        {
            AddAssert("break overlay hidden", () => !this.ChildrenOfType<BreakOverlay>().Single().Child.IsPresent);
            addSeekStep(2000);
            AddUntilStep("break overlay visible", () => this.ChildrenOfType<BreakOverlay>().Single().Child.IsPresent);
            addSeekStep(5000);
            AddAssert("break overlay hidden", () => !this.ChildrenOfType<BreakOverlay>().Single().Child.IsPresent);
            addSeekStep(7000);
            AddUntilStep("break overlay visible", () => this.ChildrenOfType<BreakOverlay>().Single().Child.IsPresent);
            addSeekStep(10000);
            AddAssert("break overlay hidden", () => !this.ChildrenOfType<BreakOverlay>().Single().Child.IsPresent);
        }

        private void addSeekStep(double time)
        {
            AddStep($"seek to {time}", () => Beatmap.Value.Track.Seek(time));

            // Allow a few frames of lenience
            AddUntilStep("wait for seek to finish", () => Precision.AlmostEquals(time, Player.DrawableRuleset.FrameStableClock.CurrentTime, 100));
        }
    }
}
