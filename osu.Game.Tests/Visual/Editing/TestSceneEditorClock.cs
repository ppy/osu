// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneEditorClock : EditorClockTestScene
    {
        [Cached]
        private EditorBeatmap editorBeatmap = new EditorBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo));

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
            AddStep("reset clock", () => EditorClock.Seek(0));

            AddStep("start clock", () => EditorClock.Start());
            AddAssert("clock running", () => EditorClock.IsRunning);

            AddStep("seek near end", () => EditorClock.Seek(EditorClock.TrackLength - 250));
            AddUntilStep("clock stops", () => !EditorClock.IsRunning);

            AddUntilStep("clock stopped at end", () => EditorClock.CurrentTime, () => Is.EqualTo(EditorClock.TrackLength));

            AddStep("start clock again", () => EditorClock.Start());
            AddAssert("clock looped to start", () => EditorClock.IsRunning && EditorClock.CurrentTime < 500);
        }

        [Test]
        public void TestWrapWhenStoppedAtTrackEnd()
        {
            AddStep("reset clock", () => EditorClock.Seek(0));

            AddStep("stop clock", () => EditorClock.Stop());
            AddAssert("clock stopped", () => !EditorClock.IsRunning);

            AddStep("seek exactly to end", () => EditorClock.Seek(EditorClock.TrackLength));
            AddAssert("clock stopped at end", () => EditorClock.CurrentTime, () => Is.EqualTo(EditorClock.TrackLength));

            AddStep("start clock again", () => EditorClock.Start());
            AddAssert("clock looped to start", () => EditorClock.IsRunning && EditorClock.CurrentTime < 500);
        }

        [Test]
        public void TestClampWhenSeekOutsideBeatmapBounds()
        {
            AddStep("stop clock", () => EditorClock.Stop());

            AddStep("seek before start time", () => EditorClock.Seek(-1000));
            AddAssert("time is clamped to 0", () => EditorClock.CurrentTime, () => Is.EqualTo(0));

            AddStep("seek beyond track length", () => EditorClock.Seek(EditorClock.TrackLength + 1000));
            AddAssert("time is clamped to track length", () => EditorClock.CurrentTime, () => Is.EqualTo(EditorClock.TrackLength));

            AddStep("seek smoothly before start time", () => EditorClock.SeekSmoothlyTo(-1000));
            AddUntilStep("time is clamped to 0", () => EditorClock.CurrentTime, () => Is.EqualTo(0));

            AddStep("seek smoothly beyond track length", () => EditorClock.SeekSmoothlyTo(EditorClock.TrackLength + 1000));
            AddUntilStep("time is clamped to track length", () => EditorClock.CurrentTime, () => Is.EqualTo(EditorClock.TrackLength));
        }

        protected override void Dispose(bool isDisposing)
        {
            Beatmap.Disabled = false;
            base.Dispose(isDisposing);
        }
    }
}
