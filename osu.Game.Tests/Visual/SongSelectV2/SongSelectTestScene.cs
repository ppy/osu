// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
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
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Overlays.Toolbar;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
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
        protected ScoreManager ScoreManager { get; private set; } = null!;

        private RealmDetachedBeatmapStore beatmapStore = null!;

        protected Screens.SelectV2.SongSelect SongSelect { get; private set; } = null!;
        protected BeatmapCarousel Carousel => SongSelect.ChildrenOfType<BeatmapCarousel>().Single();

        [Cached]
        private readonly OsuLogo logo;

        [Cached]
        private readonly VolumeOverlay volume;

        [Cached(typeof(INotificationOverlay))]
        private readonly INotificationOverlay notificationOverlay = new NotificationOverlay();

        [Cached]
        protected readonly LeaderboardManager LeaderboardManager = new LeaderboardManager();

        protected SongSelectTestScene()
        {
            Children = new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        LeaderboardManager,
                        new Toolbar
                        {
                            State = { Value = Visibility.Visible },
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
            dependencies.Cache(ScoreManager = new ScoreManager(Rulesets, () => Beatmaps, LocalStorage, Realm, API, Config));

            dependencies.CacheAs<BeatmapStore>(beatmapStore = new RealmDetachedBeatmapStore());

            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(beatmapStore);
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
                Config.SetValue(OsuSetting.SongSelectGroupMode, GroupMode.None);

                SongSelect = null!;
            });

            AddStep("delete all beatmaps", () => Beatmaps.Delete());
        }

        protected virtual void LoadSongSelect()
        {
            AddStep("load screen", () => Stack.Push(SongSelect = new SoloSongSelect()));
            AddUntilStep("wait for load", () => Stack.CurrentScreen == SongSelect && SongSelect.IsLoaded);
            AddUntilStep("wait for filtering", () => !Carousel.IsFiltering);
        }

        protected void SortBy(SortMode mode) => AddStep($"sort by {mode.GetDescription().ToLowerInvariant()}", () => Config.SetValue(OsuSetting.SongSelectSortingMode, mode));

        protected void GroupBy(GroupMode mode) => AddStep($"group by {mode.GetDescription().ToLowerInvariant()}", () => Config.SetValue(OsuSetting.SongSelectGroupMode, mode));

        protected void SortAndGroupBy(SortMode sort, GroupMode group)
        {
            AddStep($"sort by {sort.GetDescription().ToLowerInvariant()} & group by {group.GetDescription().ToLowerInvariant()}", () =>
            {
                Config.SetValue(OsuSetting.SongSelectSortingMode, sort);
                Config.SetValue(OsuSetting.SongSelectGroupMode, group);
            });
        }

        protected void WaitForFiltering() => AddUntilStep("wait for filtering", () => !SongSelect.IsFiltering);

        protected void ImportBeatmapForRuleset(params int[] rulesetIds) => ImportBeatmapForRuleset(_ => { }, 3, rulesetIds);

        protected void ImportBeatmapForRuleset(Action<BeatmapSetInfo> applyToBeatmap, int difficultyCount, params int[] rulesetIds)
        {
            int beatmapsCount = 0;

            AddStep($"import test map for ruleset {rulesetIds}", () =>
            {
                beatmapsCount = SongSelect.IsNull() ? 0 : Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single().SetItems.Count;

                var beatmapSet = TestResources.CreateTestBeatmapSetInfo(difficultyCount, Rulesets.AvailableRulesets.Where(r => rulesetIds.Contains(r.OnlineID)).ToArray());
                applyToBeatmap(beatmapSet);
                Beatmaps.Import(beatmapSet);
            });

            // This is specifically for cases where the add is happening post song select load.
            // For cases where song select is null, the assertions are provided by the load checks.
            AddUntilStep("wait for imported to arrive in carousel", () => SongSelect.IsNull() || Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single().SetItems.Count > beatmapsCount);
        }

        protected void ChangeMods(params Mod[] mods) => AddStep($"change mods to {string.Join(", ", mods.Select(m => m.Acronym))}", () => SelectedMods.Value = mods);

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

        protected void WaitForSuspension() => AddUntilStep("wait for not current", () => !SongSelect.AsNonNull().IsCurrentScreen());
    }
}
