// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Game.Collections;
using osu.Game.Graphics.Sprites;
using osu.Game.Input;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens;
using osu.Game.Screens.Mvis;
using osu.Game.Screens.Mvis.Plugins;

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

        private MvisScreen mvis;

        [Test]
        public void CreateMvisScreen()
        {
            AddStep("Create screen", () =>
            {
                if (Stack.CurrentScreen != null)
                    Stack?.Exit();

                LoadScreen(mvis = new MvisScreen());
            });
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(Storage storage, OsuGameBase gameBase)
        {
            CollectionManager collectionManager;
            MvisPluginManager mvisPluginManager;
            CustomStore customStore = dependencies.Get<CustomStore>() ?? new CustomStore(storage, gameBase);
            dependencies.Cache(customStore);

            dependencies.Cache(collectionManager = new CollectionManager(LocalStorage));
            dependencies.Cache(mvisPluginManager = new MvisPluginManager());
            dependencies.Cache(GetContainingInputManager() ?? new LocalInputManager());
            mvisPluginManager.AddPlugin(new MvisTestsPlugin());

            Add(mvisPluginManager);
            Add(idle);
            Add(musicController);
            Add(dialog);

            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            AddStep("Add Collection", () =>
            {
                collectionManager.Collections.Add(new BeatmapCollection
                {
                    Name = { Value = "Collection" },
                });
            });
        }

        private class MvisTestsPlugin : MvisPlugin
        {
            private OsuSpriteText songTitle;
            private OsuSpriteText songArtist;

            protected override Drawable CreateContent() => new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    songTitle = new OsuSpriteText(),
                    songArtist = new OsuSpriteText()
                }
            };

            protected override bool OnContentLoaded(Drawable content) => true;

            protected override bool PostInit() => true;

            public override int Version => 0;

            protected override void LoadComplete()
            {
                MvisScreen.OnBeatmapChanged += b =>
                {
                    songTitle.Text = b.Metadata.TitleUnicode ?? b.Metadata.Title;
                    songArtist.Text = b.Metadata.ArtistUnicode ?? b.Metadata.Artist;
                };

                base.LoadComplete();
            }
        }

        private class LocalInputManager : UserInputManager
        {
        }
    }
}
