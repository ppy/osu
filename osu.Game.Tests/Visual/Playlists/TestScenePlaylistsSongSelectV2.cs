// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistsSongSelectV2 : OnlinePlayTestScene
    {
        private BeatmapManager beatmaps = null!;
        private RealmRulesetStore rulesets = null!;
        private OsuConfigManager config = null!;
        private ScoreManager scoreManager = null!;
        private RealmDetachedBeatmapStore beatmapStore = null!;

        private PlaylistsSongSelectV2 songSelect = null!;

        private BeatmapCarousel Carousel => songSelect.ChildrenOfType<BeatmapCarousel>().Single();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            // These DI caches are required to ensure for interactive runs this test scene doesn't nuke all user beatmaps in the local install.
            // At a point we have isolated interactive test runs enough, this can likely be removed.
            dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            dependencies.Cache(Realm);
            dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, Dependencies.Get<AudioManager>(), Resources, Dependencies.Get<GameHost>(), Beatmap.Default));
            dependencies.Cache(config = new OsuConfigManager(LocalStorage));
            dependencies.Cache(scoreManager = new ScoreManager(rulesets, () => beatmaps, LocalStorage, Realm, API, config));

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

            ImportBeatmapForRuleset(0);

            AddStep("load screen", () => LoadScreen(songSelect = new PlaylistsSongSelectV2()));
            AddUntilStep("wait for load", () => Stack.CurrentScreen == songSelect && songSelect.IsLoaded);
            AddUntilStep("wait for filtering", () => !Carousel.IsFiltering);
        }

        protected void ImportBeatmapForRuleset(params int[] rulesetIds) => ImportBeatmapForRuleset(_ => { }, 3, rulesetIds);

        protected void ImportBeatmapForRuleset(Action<BeatmapSetInfo> applyToBeatmap, int difficultyCount, params int[] rulesetIds)
        {
            int beatmapsCount = 0;

            AddStep($"import test map for ruleset {rulesetIds}", () =>
            {
                beatmapsCount = songSelect.IsNull() ? 0 : Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single().SetItems.Count;

                var beatmapSet = TestResources.CreateTestBeatmapSetInfo(difficultyCount, rulesets.AvailableRulesets.Where(r => rulesetIds.Contains(r.OnlineID)).ToArray());
                applyToBeatmap(beatmapSet);
                beatmaps.Import(beatmapSet);
            });

            // This is specifically for cases where the add is happening post song select load.
            // For cases where song select is null, the assertions are provided by the load checks.
            AddUntilStep("wait for imported to arrive in carousel", () => songSelect.IsNull() || Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single().SetItems.Count > beatmapsCount);
        }
    }
}
