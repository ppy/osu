// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Overlays.Toolbar;
using osu.Game.Rulesets;
using osu.Game.Screens;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public abstract partial class SongSelectTestScene : ScreenTestScene
    {
        protected BeatmapManager Beatmaps { get; private set; } = null!;
        protected RealmRulesetStore Rulesets { get; private set; } = null!;
        protected OsuConfigManager Config { get; private set; } = null!;

        private RealmDetachedBeatmapStore beatmapStore = null!;

        protected Screens.SelectV2.SongSelect Screen { get; private set; } = null!;
        protected BeatmapCarousel Carousel => Screen.ChildrenOfType<BeatmapCarousel>().Single();

        [Cached]
        private readonly ScreenFooter screenFooter;

        [Cached]
        private readonly OsuLogo logo;

        [Cached]
        private readonly VolumeOverlay volume;

        [Cached(typeof(INotificationOverlay))]
        private readonly INotificationOverlay notificationOverlay = new NotificationOverlay();

        protected SongSelectTestScene()
        {
            Children = new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Toolbar
                        {
                            State = { Value = Visibility.Visible },
                        },
                        screenFooter = new ScreenFooter
                        {
                            OnBack = () => Stack.CurrentScreen.Exit(),
                        },
                        logo = new OsuLogo
                        {
                            Alpha = 0f,
                        },
                        volume = new VolumeOverlay(),
                    },
                },
            };

            Stack.Padding = new MarginPadding { Top = Toolbar.HEIGHT };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            // These DI caches are required to ensure for interactive runs this test scene doesn't nuke all user beatmaps in the local install.
            // At a point we have isolated interactive test runs enough, this can likely be removed.
            dependencies.Cache(Rulesets = new RealmRulesetStore(Realm));
            dependencies.Cache(Realm);
            dependencies.Cache(Beatmaps = new BeatmapManager(LocalStorage, Realm, null, Dependencies.Get<AudioManager>(), Resources, Dependencies.Get<GameHost>(), Beatmap.Default));
            dependencies.Cache(Config = new OsuConfigManager(LocalStorage));

            dependencies.CacheAs<BeatmapStore>(beatmapStore = new RealmDetachedBeatmapStore());

            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(beatmapStore);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Stack.ScreenPushed += updateFooter;
            Stack.ScreenExited += updateFooter;
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset defaults", () =>
            {
                Ruleset.Value = Rulesets.AvailableRulesets.First();

                Beatmap.SetDefault();
                SelectedMods.SetDefault();

                Config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.Title);
                Config.SetValue(OsuSetting.SongSelectGroupingMode, GroupMode.All);

                Screen = null!;
            });

            AddStep("delete all beatmaps", () => Beatmaps.Delete());
        }

        protected virtual void LoadSongSelect()
        {
            AddStep("load screen", () => Stack.Push(Screen = new SoloSongSelect()));
            AddUntilStep("wait for load", () => Stack.CurrentScreen == Screen && Screen.IsLoaded);
        }

        protected void ImportBeatmapForRuleset(int rulesetId)
        {
            int beatmapsCount = 0;

            AddStep($"import test map for ruleset {rulesetId}", () =>
            {
                beatmapsCount = Screen.IsNull() ? 0 : Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single().SetItems.Count;
                Beatmaps.Import(TestResources.CreateTestBeatmapSetInfo(3, Rulesets.AvailableRulesets.Where(r => r.OnlineID == rulesetId).ToArray()));
            });

            // This is specifically for cases where the add is happening post song select load.
            // For cases where song select is null, the assertions are provided by the load checks.
            AddUntilStep("wait for imported to arrive in carousel", () => Screen.IsNull() || Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single().SetItems.Count > beatmapsCount);
        }

        protected void ChangeRuleset(int rulesetId)
        {
            AddStep($"change ruleset to {rulesetId}", () => Ruleset.Value = Rulesets.AvailableRulesets.First(r => r.OnlineID == rulesetId));
        }

        /// <summary>
        /// Imports test beatmap sets to show in the carousel.
        /// </summary>
        /// <param name="difficultyCountPerSet">
        /// The exact count of difficulties to create for each beatmap set.
        /// A <see langword="null"/> value causes the count of difficulties to be selected randomly.
        /// </param>
        protected void AddManyTestMaps(int? difficultyCountPerSet = null)
        {
            AddStep("import test maps", () =>
            {
                var usableRulesets = Rulesets.AvailableRulesets.Where(r => r.OnlineID != 2).ToArray();

                for (int i = 0; i < 10; i++)
                    Beatmaps.Import(TestResources.CreateTestBeatmapSetInfo(difficultyCountPerSet, usableRulesets));
            });
        }

        protected void WaitForSuspension() => AddUntilStep("wait for not current", () => !Screen.AsNonNull().IsCurrentScreen());

        private void updateFooter(IScreen? _, IScreen? newScreen)
        {
            if (newScreen is IOsuScreen osuScreen && osuScreen.ShowFooter)
            {
                screenFooter.Show();
                screenFooter.SetButtons(osuScreen.CreateFooterButtons());
            }
            else
            {
                screenFooter.Hide();
                screenFooter.SetButtons(Array.Empty<ScreenFooterButton>());
            }
        }
    }
}
