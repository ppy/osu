// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneSongProgress : SkinnableHUDComponentTestScene
    {
        private GameplayClockContainer gameplayClockContainer = null!;

        private const double skip_target_time = -2000;

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            Add(gameplayClockContainer = new MasterGameplayClockContainer(Beatmap.Value, skip_target_time));

            Dependencies.CacheAs<IGameplayClock>(gameplayClockContainer);
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("reset clock", () => gameplayClockContainer.Reset());
            AddStep("set hit objects", setHitObjects);
        }

        [Test]
        public void TestDisplay()
        {
            AddStep("seek to intro", () => gameplayClockContainer.Seek(skip_target_time));
            AddStep("start", gameplayClockContainer.Start);
            AddStep("stop", gameplayClockContainer.Stop);
        }

        [Test]
        public void TestToggleSeeking()
        {
            DefaultSongProgress getDefaultProgress() => this.ChildrenOfType<DefaultSongProgress>().Single();

            AddStep("allow seeking", () => getDefaultProgress().AllowSeeking.Value = true);
            AddStep("hide graph", () => getDefaultProgress().ShowGraph.Value = false);
            AddStep("disallow seeking", () => getDefaultProgress().AllowSeeking.Value = false);
            AddStep("allow seeking", () => getDefaultProgress().AllowSeeking.Value = true);
            AddStep("show graph", () => getDefaultProgress().ShowGraph.Value = true);
        }

        private void setHitObjects()
        {
            var objects = new List<HitObject>();
            for (double i = 0; i < 5000; i++)
                objects.Add(new HitObject { StartTime = i });

            this.ChildrenOfType<SongProgress>().ForEach(progress => progress.Objects = objects);
        }

        protected override Drawable CreateDefaultImplementation() => new DefaultSongProgress();

        protected override Drawable CreateLegacyImplementation() => new LegacySongProgress();
    }
}
