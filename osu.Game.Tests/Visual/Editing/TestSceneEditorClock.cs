// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public class TestSceneEditorClock : EditorClockTestScene
    {
        public TestSceneEditorClock()
        {
            Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new TimeInfoContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(200, 100)
                    },
                    new PlaybackControl
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(200, 100)
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
            // ensure that music controller does not change this beatmap due to it
            // completing naturally as part of the test.
            Beatmap.Disabled = true;
        }

        [Test]
        public void TestStopAtTrackEnd()
        {
            AddStep("reset clock", () => Clock.Seek(0));

            AddStep("start clock", Clock.Start);
            AddAssert("clock running", () => Clock.IsRunning);

            AddStep("seek near end", () => Clock.Seek(Clock.TrackLength - 250));
            AddUntilStep("clock stops", () => !Clock.IsRunning);

            AddAssert("clock stopped at end", () => Clock.CurrentTime == Clock.TrackLength);

            AddStep("start clock again", Clock.Start);
            AddAssert("clock looped to start", () => Clock.IsRunning && Clock.CurrentTime < 500);
        }

        [Test]
        public void TestWrapWhenStoppedAtTrackEnd()
        {
            AddStep("reset clock", () => Clock.Seek(0));

            AddStep("stop clock", Clock.Stop);
            AddAssert("clock stopped", () => !Clock.IsRunning);

            AddStep("seek exactly to end", () => Clock.Seek(Clock.TrackLength));
            AddAssert("clock stopped at end", () => Clock.CurrentTime == Clock.TrackLength);

            AddStep("start clock again", Clock.Start);
            AddAssert("clock looped to start", () => Clock.IsRunning && Clock.CurrentTime < 500);
        }

        [Test]
        public void TestClampWhenSeekOutsideBeatmapBounds()
        {
            AddStep("stop clock", Clock.Stop);

            AddStep("seek before start time", () => Clock.Seek(-1000));
            AddAssert("time is clamped to 0", () => Clock.CurrentTime == 0);

            AddStep("seek beyond track length", () => Clock.Seek(Clock.TrackLength + 1000));
            AddAssert("time is clamped to track length", () => Clock.CurrentTime == Clock.TrackLength);

            AddStep("seek smoothly before start time", () => Clock.SeekSmoothlyTo(-1000));
            AddAssert("time is clamped to 0", () => Clock.CurrentTime == 0);

            AddStep("seek smoothly beyond track length", () => Clock.SeekSmoothlyTo(Clock.TrackLength + 1000));
            AddAssert("time is clamped to track length", () => Clock.CurrentTime == Clock.TrackLength);
        }

        protected override void Dispose(bool isDisposing)
        {
            Beatmap.Disabled = false;
            base.Dispose(isDisposing);
        }
    }
}
