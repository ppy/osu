// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneSongProgress : SkinnableHUDComponentTestScene
    {
        private DefaultSongProgress progress => this.ChildrenOfType<DefaultSongProgress>().Single();
        private GameplayClockContainer gameplayClockContainer;
        private const double gameplay_start_time = -2000;

        [BackgroundDependencyLoader]
        private void load()
        {
            var working = CreateWorkingBeatmap(Ruleset.Value);
            working.LoadTrack();
            Add(gameplayClockContainer = new MasterGameplayClockContainer(working, gameplay_start_time));
            Dependencies.CacheAs(gameplayClockContainer);
            Dependencies.CacheAs(gameplayClockContainer.GameplayClock);
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("reset clock", () => gameplayClockContainer.Reset(false));
            AddStep("set hit objects", setHitObjects);
        }

        [Test]
        public void TestDisplay()
        {
            AddStep("seek to intro", () => gameplayClockContainer.Seek(gameplay_start_time));
            AddStep("start", gameplayClockContainer.Start);
            AddStep("stop", gameplayClockContainer.Stop);
        }

        [Test]
        public void TestToggleSeeking()
        {
            AddStep("allow seeking", () => progress.AllowSeeking.Value = true);
            AddStep("hide graph", () => progress.ShowGraph.Value = false);
            AddStep("disallow seeking", () => progress.AllowSeeking.Value = false);
            AddStep("allow seeking", () => progress.AllowSeeking.Value = true);
            AddStep("show graph", () => progress.ShowGraph.Value = true);
        }

        private void setHitObjects()
        {
            var objects = new List<HitObject>();
            for (double i = 0; i < 5000; i++)
                objects.Add(new HitObject { StartTime = i });

            this.ChildrenOfType<SongProgress>().ForEach(progress => progress.Objects = objects);
        }

        protected override Drawable CreateDefaultImplementation() => new DefaultSongProgress
        {
            RelativeSizeAxes = Axes.X,
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
        };

        protected override Drawable CreateLegacyImplementation() => new LegacySongProgress
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };
    }
}
