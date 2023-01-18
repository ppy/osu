// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public partial class TestSceneSongProgress : SkinnableHUDComponentTestScene
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
            AddStep("hook seeking", () =>
            {
                applyToDefaultProgress(d => d.ChildrenOfType<SongProgressBar>().Single().OnSeek += t => gameplayClockContainer.Seek(t));
                applyToArgonProgress(d => d.ChildrenOfType<ArgonSongProgressBar>().Single().OnSeek += t => gameplayClockContainer.Seek(t));
            });
            AddStep("seek to intro", () => gameplayClockContainer.Seek(skip_target_time));
            AddStep("start", gameplayClockContainer.Start);
        }

        [Test]
        public void TestBasic()
        {
            AddToggleStep("toggle seeking", b =>
            {
                applyToDefaultProgress(s => s.Interactive.Value = b);
                applyToArgonProgress(s => s.Interactive.Value = b);
            });

            AddToggleStep("toggle graph", b =>
            {
                applyToDefaultProgress(s => s.ShowGraph.Value = b);
                applyToArgonProgress(s => s.ShowGraph.Value = b);
            });

            AddStep("stop", gameplayClockContainer.Stop);
        }

        private void applyToArgonProgress(Action<ArgonSongProgress> action) =>
            this.ChildrenOfType<ArgonSongProgress>().ForEach(action);

        private void applyToDefaultProgress(Action<DefaultSongProgress> action) =>
            this.ChildrenOfType<DefaultSongProgress>().ForEach(action);

        private void setHitObjects()
        {
            var objects = new List<HitObject>();
            for (double i = 0; i < 5000; i++)
                objects.Add(new HitObject { StartTime = i });

            this.ChildrenOfType<SongProgress>().ForEach(progress => progress.Objects = objects);
        }

        protected override Drawable CreateDefaultImplementation() => new DefaultSongProgress();

        protected override Drawable CreateArgonImplementation() => new ArgonSongProgress();

        protected override Drawable CreateLegacyImplementation() => new LegacySongProgress();
    }
}
