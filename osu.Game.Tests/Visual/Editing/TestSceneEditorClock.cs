// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
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

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
            // ensure that music controller does not change this beatmap due to it
            // completing naturally as part of the test.
            Beatmap.Disabled = true;
        }

        [Test]
        public void TestStopAtTrackEnd()
        {
            AddStep("Reset clock", () => Clock.Seek(0));
            AddStep("Start clock", Clock.Start);
            AddAssert("Clock running", () => Clock.IsRunning);
            AddStep("Seek near end", () => Clock.Seek(Clock.TrackLength - 250));
            AddUntilStep("Clock stops", () => !Clock.IsRunning);
            AddAssert("Clock stopped at end", () => Clock.CurrentTime == Clock.TrackLength);
            AddStep("Start clock again", Clock.Start);
            AddAssert("Clock looped to start", () => Clock.IsRunning && Clock.CurrentTime < 500);
        }
    }
}
