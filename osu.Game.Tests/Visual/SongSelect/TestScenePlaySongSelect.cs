// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.MathUtils;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public class TestScenePlaySongSelect : ScreenTestScene
    {
        private BeatmapManager manager;

        private RulesetStore rulesets;

        private WorkingBeatmap defaultBeatmap;
        private DatabaseContextFactory factory;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Screens.Select.SongSelect),
            typeof(BeatmapCarousel),

            typeof(CarouselItem),
            typeof(CarouselGroup),
            typeof(CarouselGroupEagerSelect),
            typeof(CarouselBeatmap),
            typeof(CarouselBeatmapSet),

            typeof(DrawableCarouselItem),
            typeof(CarouselItemState),

            typeof(DrawableCarouselBeatmap),
            typeof(DrawableCarouselBeatmapSet),
        };

        private class TestSongSelect : PlaySongSelect
        {
            public Action StartRequested;

            public new Bindable<RulesetInfo> Ruleset => base.Ruleset;

            public WorkingBeatmap CurrentBeatmap => Beatmap.Value;
            public WorkingBeatmap CurrentBeatmapDetailsBeatmap => BeatmapDetails.Beatmap;
            public new BeatmapCarousel Carousel => base.Carousel;

            protected override bool OnStart()
            {
                StartRequested?.Invoke();
                return base.OnStart();
            }
        }

        private TestSongSelect songSelect;

        protected override void Dispose(bool isDisposing)
        {
            factory.ResetDatabase();
            base.Dispose(isDisposing);
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            factory = new DatabaseContextFactory(LocalStorage);
            factory.ResetDatabase();

            using (var usage = factory.Get())
                usage.Migrate();

            factory.ResetDatabase();

            using (var usage = factory.Get())
                usage.Migrate();

            Dependencies.Cache(rulesets = new RulesetStore(factory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, factory, rulesets, null, audio, host, defaultBeatmap = Beatmap.Default));

            Beatmap.SetDefault();
        }

        [SetUp]
        public virtual void SetUp() => Schedule(() =>
        {
            Ruleset.Value = new OsuRuleset().RulesetInfo;
            manager?.Delete(manager.GetAllUsableBeatmapSets());
        });

        [Test]
        public void TestDummy()
        {
            createSongSelect();
            AddAssert("dummy selected", () => songSelect.CurrentBeatmap == defaultBeatmap);

            AddUntilStep("dummy shown on wedge", () => songSelect.CurrentBeatmapDetailsBeatmap == defaultBeatmap);

            addManyTestMaps();
            AddWaitStep("wait for select", 3);

            AddAssert("random map selected", () => songSelect.CurrentBeatmap != defaultBeatmap);
        }

        [Test]
        public void TestSorting()
        {
            createSongSelect();
            addManyTestMaps();
            AddWaitStep("wait for add", 3);

            AddAssert("random map selected", () => songSelect.CurrentBeatmap != defaultBeatmap);

            AddStep(@"Sort by Artist", delegate { songSelect.FilterControl.Sort = SortMode.Artist; });
            AddStep(@"Sort by Title", delegate { songSelect.FilterControl.Sort = SortMode.Title; });
            AddStep(@"Sort by Author", delegate { songSelect.FilterControl.Sort = SortMode.Author; });
            AddStep(@"Sort by Difficulty", delegate { songSelect.FilterControl.Sort = SortMode.Difficulty; });
        }

        [Test]
        [Ignore("needs fixing")]
        public void TestImportUnderDifferentRuleset()
        {
            createSongSelect();
            changeRuleset(2);
            addRulesetImportStep(0);
            AddUntilStep("no selection", () => songSelect.Carousel.SelectedBeatmap == null);
        }

        [Test]
        public void TestImportUnderCurrentRuleset()
        {
            createSongSelect();
            changeRuleset(2);
            addRulesetImportStep(2);
            addRulesetImportStep(1);
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmap.RulesetID == 2);

            changeRuleset(1);
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmap.RulesetID == 1);

            changeRuleset(0);
            AddUntilStep("no selection", () => songSelect.Carousel.SelectedBeatmap == null);
        }

        [Test]
        public void TestRulesetChangeResetsMods()
        {
            createSongSelect();
            changeRuleset(0);

            changeMods(new OsuModHardRock());

            int actionIndex = 0;
            int modChangeIndex = 0;
            int rulesetChangeIndex = 0;

            AddStep("change ruleset", () =>
            {
                Mods.ValueChanged += onModChange;
                songSelect.Ruleset.ValueChanged += onRulesetChange;

                Ruleset.Value = new TaikoRuleset().RulesetInfo;

                Mods.ValueChanged -= onModChange;
                songSelect.Ruleset.ValueChanged -= onRulesetChange;
            });

            AddAssert("mods changed before ruleset", () => modChangeIndex < rulesetChangeIndex);
            AddAssert("empty mods", () => !Mods.Value.Any());

            void onModChange(ValueChangedEvent<IReadOnlyList<Mod>> e) => modChangeIndex = actionIndex++;
            void onRulesetChange(ValueChangedEvent<RulesetInfo> e) => rulesetChangeIndex = actionIndex++;
        }

        [Test]
        public void TestStartAfterUnMatchingFilterDoesNotStart()
        {
            createSongSelect();
            addManyTestMaps();
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmap != null);

            bool startRequested = false;

            AddStep("set filter and finalize", () =>
            {
                songSelect.StartRequested = () => startRequested = true;

                songSelect.Carousel.Filter(new FilterCriteria { SearchText = "somestringthatshouldn'tbematchable" });
                songSelect.FinaliseSelection();

                songSelect.StartRequested = null;
            });

            AddAssert("start not requested", () => !startRequested);
        }

        [Test]
        public void TestHideSetSelectsCorrectBeatmap()
        {
            int? previousID = null;
            createSongSelect();
            addRulesetImportStep(0);
            AddStep("Move to last difficulty", () => songSelect.Carousel.SelectBeatmap(songSelect.Carousel.BeatmapSets.First().Beatmaps.Last()));
            AddStep("Store current ID", () => previousID = songSelect.Carousel.SelectedBeatmap.ID);
            AddStep("Hide first beatmap", () => manager.Hide(songSelect.Carousel.SelectedBeatmapSet.Beatmaps.First()));
            AddAssert("Selected beatmap has not changed", () => songSelect.Carousel.SelectedBeatmap.ID == previousID);
        }

        private void addRulesetImportStep(int id) => AddStep($"import test map for ruleset {id}", () => importForRuleset(id));

        private void importForRuleset(int id) => manager.Import(createTestBeatmapSet(getImportId(), rulesets.AvailableRulesets.Where(r => r.ID == id).ToArray())).Wait();

        private static int importId;
        private int getImportId() => ++importId;

        private void changeMods(params Mod[] mods) => AddStep($"change mods to {string.Join(", ", mods.Select(m => m.Acronym))}", () => Mods.Value = mods);

        private void changeRuleset(int id) => AddStep($"change ruleset to {id}", () => Ruleset.Value = rulesets.AvailableRulesets.First(r => r.ID == id));

        private void createSongSelect()
        {
            AddStep("create song select", () => LoadScreen(songSelect = new TestSongSelect()));
            AddUntilStep("wait for present", () => songSelect.IsCurrentScreen());
        }

        private void addManyTestMaps()
        {
            AddStep("import test maps", () =>
            {
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.ID != 2).ToArray();

                for (int i = 0; i < 100; i += 10)
                    manager.Import(createTestBeatmapSet(i, usableRulesets)).Wait();
            });
        }

        private BeatmapSetInfo createTestBeatmapSet(int setId, RulesetInfo[] rulesets)
        {
            int j = 0;
            RulesetInfo getRuleset() => rulesets[j++ % rulesets.Length];

            var beatmaps = new List<BeatmapInfo>();

            for (int i = 0; i < 6; i++)
            {
                int beatmapId = setId * 10 + i;

                beatmaps.Add(new BeatmapInfo
                {
                    Ruleset = getRuleset(),
                    OnlineBeatmapID = beatmapId,
                    Path = "normal.osu",
                    Version = $"{beatmapId}",
                    BaseDifficulty = new BeatmapDifficulty
                    {
                        OverallDifficulty = 3.5f,
                    }
                });
            }

            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = setId,
                Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                Metadata = new BeatmapMetadata
                {
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = "Some Artist " + RNG.Next(0, 9),
                    Title = $"Some Song (set id {setId})",
                    AuthorString = "Some Guy " + RNG.Next(0, 9),
                },
                Beatmaps = beatmaps
            };
        }
    }
}
