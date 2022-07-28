// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneSongProgress : SkinnableHUDComponentTestScene
    {
        private DefaultSongProgress defaultProgress;

        private readonly List<SongProgress> progresses = new List<SongProgress>();

        private readonly StopwatchClock clock;
        private readonly FramedClock framedClock;

        [Cached]
        private readonly GameplayClock gameplayClock;

        public TestSceneSongProgress()
        {
            clock = new StopwatchClock();
            gameplayClock = new GameplayClock(framedClock = new FramedClock(clock));
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("reset clock", clock.Reset);
        }

        [Test]
        public void TestDisplay()
        {
            AddStep("display max values", displayMaxValues);
            AddStep("start", clock.Start);
            AddStep("stop", clock.Stop);
        }

        [Test]
        public void TestToggleSeeking()
        {
            AddStep("allow seeking", () => defaultProgress.AllowSeeking.Value = true);
            AddStep("hide graph", () => defaultProgress.ShowGraph.Value = false);
            AddStep("disallow seeking", () => defaultProgress.AllowSeeking.Value = false);
            AddStep("allow seeking", () => defaultProgress.AllowSeeking.Value = true);
            AddStep("show graph", () => defaultProgress.ShowGraph.Value = true);
        }

        private void displayMaxValues()
        {
            var objects = new List<HitObject>();
            for (double i = 0; i < 5000; i++)
                objects.Add(new HitObject { StartTime = i });

            replaceObjects(objects);
        }

        private void replaceObjects(List<HitObject> objects)
        {
            defaultProgress.RequestSeek = pos => clock.Seek(pos);

            foreach (var p in progresses)
            {
                p.Objects = objects;
            }
        }

        protected override void Update()
        {
            base.Update();
            framedClock.ProcessFrame();
        }

        protected override Drawable CreateDefaultImplementation()
        {
            defaultProgress = new DefaultSongProgress
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
            };

            progresses.Add(defaultProgress);
            return defaultProgress;
        }

        protected override Drawable CreateLegacyImplementation()
        {
            var progress = new LegacySongProgress
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            progresses.Add(progress);
            return progress;
        }
    }
}
