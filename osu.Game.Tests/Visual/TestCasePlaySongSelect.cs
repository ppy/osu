// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCasePlaySongSelect : OsuTestCase
    {
        private BeatmapManager manager;

        private RulesetStore rulesets;

        private WorkingBeatmap defaultBeatmap;
        private DatabaseContextFactory factory;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SongSelect),
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
            public WorkingBeatmap CurrentBeatmap => Beatmap.Value;
            public WorkingBeatmap CurrentBeatmapDetailsBeatmap => BeatmapDetails.Beatmap;
            public new BeatmapCarousel Carousel => base.Carousel;
        }

        private TestSongSelect songSelect;

        protected override void Dispose(bool isDisposing)
        {
            factory.ResetDatabase();
            base.Dispose(isDisposing);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            factory = new DatabaseContextFactory(LocalStorage);
            factory.ResetDatabase();

            using (var usage = factory.Get())
                usage.Migrate();

            factory.ResetDatabase();

            using (var usage = factory.Get())
                usage.Migrate();

            Dependencies.Cache(rulesets = new RulesetStore(factory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, factory, rulesets, null, null)
            {
                DefaultBeatmap = defaultBeatmap = Beatmap.Default
            });

            Beatmap.SetDefault();
        }

        [SetUp]
        public virtual void SetUp()
        {
            manager?.Delete(manager.GetAllUsableBeatmapSets());
            Child = songSelect = new TestSongSelect();
        }

        [Test]
        public void TestDummy()
        {
            AddAssert("dummy selected", () => songSelect.CurrentBeatmap == defaultBeatmap);

            AddAssert("dummy shown on wedge", () => songSelect.CurrentBeatmapDetailsBeatmap == defaultBeatmap);

            addManyTestMaps();
            AddWaitStep(3);

            AddAssert("random map selected", () => songSelect.CurrentBeatmap != defaultBeatmap);
        }

        [Test]
        public void TestSorting()
        {
            addManyTestMaps();
            AddWaitStep(3);

            AddAssert("random map selected", () => songSelect.CurrentBeatmap != defaultBeatmap);

            AddStep(@"Sort by Artist", delegate { songSelect.FilterControl.Sort = SortMode.Artist; });
            AddStep(@"Sort by Title", delegate { songSelect.FilterControl.Sort = SortMode.Title; });
            AddStep(@"Sort by Author", delegate { songSelect.FilterControl.Sort = SortMode.Author; });
            AddStep(@"Sort by Difficulty", delegate { songSelect.FilterControl.Sort = SortMode.Difficulty; });
        }

        [Test]
        [Ignore("needs fixing")]
        public void ImportUnderDifferentRuleset()
        {
            changeRuleset(2);
            importForRuleset(0);
            AddUntilStep(() => songSelect.Carousel.SelectedBeatmap == null, "no selection");
        }

        [Test]
        public void ImportUnderCurrentRuleset()
        {
            changeRuleset(2);
            importForRuleset(2);
            importForRuleset(1);
            AddUntilStep(() => songSelect.Carousel.SelectedBeatmap.RulesetID == 2, "has selection");

            changeRuleset(1);
            AddUntilStep(() => songSelect.Carousel.SelectedBeatmap.RulesetID == 1, "has selection");

            changeRuleset(0);
            AddUntilStep(() => songSelect.Carousel.SelectedBeatmap == null, "no selection");
        }

        private void importForRuleset(int id) => AddStep($"import test map for ruleset {id}", () => manager.Import(createTestBeatmapSet(getImportId(), rulesets.AvailableRulesets.Where(r => r.ID == id).ToArray())));

        private static int importId;
        private int getImportId() => ++importId;

        private void changeRuleset(int id) => AddStep($"change ruleset to {id}", () => Ruleset.Value = rulesets.AvailableRulesets.First(r => r.ID == id));

        private void addManyTestMaps()
        {
            AddStep("import test maps", () =>
            {
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.ID != 2).ToArray();

                for (int i = 0; i < 100; i += 10)
                    manager.Import(createTestBeatmapSet(i, usableRulesets));
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
