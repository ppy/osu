// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Collections;
using osu.Game.Input;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Mvis;

namespace osu.Game.Tests.Visual.Mvis
{
    [TestFixture]
    public class TestSceneMvisScreen : ScreenTestScene
    {
        [Cached]
        private MusicController musicController = new MusicController();

        [Cached]
        private IdleTracker idle = new IdleTracker(6000);

        [Cached]
        private DialogOverlay dialog = new DialogOverlay();

        private DependencyContainer dependencies;

        [Test]
        public void CreateMvisScreen()
        {
            AddStep("Create screen", () =>
            {
                if (Stack.CurrentScreen != null)
                    Stack?.Exit();

                LoadScreen(new MvisScreen());
            });
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(idle);
            Add(musicController);
            Add(dialog);

            CollectionManager collectionManager;
            dependencies.Cache(collectionManager = new CollectionManager(LocalStorage));
            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            AddStep("Add Collection", () =>
            {
                collectionManager.Collections.Add(new BeatmapCollection
                {
                    Name = { Value = "Collection" },
                });
            });
        }
    }
}
