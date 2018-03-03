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
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Screens.Select.Filter;
using osu.Game.Tests.Platform;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCasePlaySongSelect : OsuTestCase
    {
        private BeatmapManager manager;

        private RulesetStore rulesets;

        private DependencyContainer dependencies;
        private WorkingBeatmap defaultBeatmap;

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

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) => dependencies = new DependencyContainer(parent);

        private class TestSongSelect : PlaySongSelect
        {
            public WorkingBeatmap CurrentBeatmap => Beatmap.Value;
            public WorkingBeatmap CurrentBeatmapDetailsBeatmap => BeatmapDetails.Beatmap;
            public new BeatmapCarousel Carousel => base.Carousel;

            public void SetRuleset(RulesetInfo ruleset) => Ruleset.Value = ruleset;

            public int? RulesetID => Ruleset.Value.ID;

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                // Necessary while running tests because gc is moody and uncollected object interferes with OnEntering test
                Beatmap.ValueChanged -= WorkingBeatmapChanged;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuConfigManager config)
        {
            TestSongSelect songSelect = null;

            var storage = new TestStorage(@"TestCasePlaySongSelect");

            // this is by no means clean. should be replacing inside of OsuGameBase somehow.
            IDatabaseContextFactory factory = new SingletonContextFactory(new OsuDbContext());

            dependencies.Cache(rulesets = new RulesetStore(factory));
            dependencies.Cache(manager = new BeatmapManager(storage, factory, rulesets, null)
            {
                DefaultBeatmap = defaultBeatmap = game.Beatmap.Default
            });

            void loadNewSongSelect(bool deleteMaps = false) => AddStep("reload song select", () =>
            {
                if (deleteMaps)
                {
                    // TODO: check why this alone doesn't allow import test to run twice in the same session, probably because the delete op is not saved?
                    manager.Delete(manager.GetAllUsableBeatmapSets());
                    game.Beatmap.SetDefault();
                }

                if (songSelect != null)
                {
                    Remove(songSelect);
                    songSelect.Dispose();
                }

                Add(songSelect = new TestSongSelect());

                songSelect?.SetRuleset(rulesets.AvailableRulesets.First());
            });

            loadNewSongSelect(true);

            AddWaitStep(3);

            AddAssert("dummy selected", () => songSelect.CurrentBeatmap == defaultBeatmap);

            AddAssert("dummy shown on wedge", () => songSelect.CurrentBeatmapDetailsBeatmap == defaultBeatmap);

            AddStep("import test maps", () =>
            {
                for (int i = 0; i < 100; i += 10)
                    manager.Import(createTestBeatmapSet(i));

                // also import a set which has a single non - osu ruleset beatmap
                manager.Import(new BeatmapSetInfo
                {
                    OnlineBeatmapSetID = 1993,
                    Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                    Metadata = new BeatmapMetadata
                    {
                        OnlineBeatmapSetID = 1993,
                        // Create random metadata, then we can check if sorting works based on these
                        Artist = "MONACA " + RNG.Next(0, 9),
                        Title = "Black Song " + RNG.Next(0, 9),
                        AuthorString = "Some Guy " + RNG.Next(0, 9),
                    },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            OnlineBeatmapID = 1994,
                            Ruleset = rulesets.AvailableRulesets.ElementAt(3),
                            RulesetID = 3,
                            Path = "normal.fruits",
                            Version = "Normal",
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                OverallDifficulty = 3.5f,
                            }
                        },
                    }
                });
            });

            AddWaitStep(3);
            AddAssert("random map selected", () => songSelect.CurrentBeatmap != defaultBeatmap);

            loadNewSongSelect();
            AddWaitStep(3);
            AddAssert("random map selected", () => songSelect.CurrentBeatmap != defaultBeatmap);

            AddStep(@"Sort by Artist", delegate { songSelect.FilterControl.Sort = SortMode.Artist; });
            AddStep(@"Sort by Title", delegate { songSelect.FilterControl.Sort = SortMode.Title; });
            AddStep(@"Sort by Author", delegate { songSelect.FilterControl.Sort = SortMode.Author; });
            AddStep(@"Sort by Difficulty", delegate { songSelect.FilterControl.Sort = SortMode.Difficulty; });

            // Test that song select sets a playable beatmap while entering
            AddStep(@"Remove song select", () =>
            {
                Remove(songSelect);
                songSelect.Dispose();
                songSelect = null;
            });
            AddStep(@"Set non-osu beatmap", () => game.Beatmap.Value = manager.GetWorkingBeatmap(manager.GetAllUsableBeatmapSets().First().Beatmaps.First(b => b.RulesetID != 0)));
            AddAssert(@"Non-osu beatmap set", () => game.Beatmap.Value.BeatmapInfo.RulesetID != 0);
            loadNewSongSelect();
            AddWaitStep(3);
            AddAssert(@"osu beatmap set", () => game.Beatmap.Value.BeatmapInfo.RulesetID == 0);

            // Test that song select changes WorkingBeatmap to be playable in current ruleset when updated externally
            AddStep(@"Try set non-osu beatmap", () =>
            {
                var testMap = manager.GetAllUsableBeatmapSets().First().Beatmaps.First(b => b.RulesetID != 0);
                songSelect.SetRuleset(rulesets.AvailableRulesets.First());
                game.Beatmap.Value = manager.GetWorkingBeatmap(testMap);
            });
            AddAssert(@"Beatmap changed to osu", () => songSelect.RulesetID == 0 && game.Beatmap.Value.BeatmapInfo.RulesetID == 0);

            // Test that song select updates WorkingBeatmap when ruleset conversion is disabled
            AddStep(@"Disable beatmap conversion", () => config.Set(OsuSetting.ShowConvertedBeatmaps, false));
            AddStep(@"Set osu beatmap taiko rs", () =>
            {
                game.Beatmap.Value = manager.GetWorkingBeatmap(manager.GetAllUsableBeatmapSets().First().Beatmaps.First(b => b.RulesetID == 0));
                songSelect.SetRuleset(rulesets.AvailableRulesets.First(r => r.ID == 1));
            });
            AddAssert(@"taiko beatmap set", () => songSelect.RulesetID == 1);

            // Test that song select changes the active ruleset when externally set beatmapset has no playable beatmaps
            AddStep(@"Set fruits only beatmapset", () =>
            {
                songSelect.SetRuleset(rulesets.AvailableRulesets.First());
                game.Beatmap.Value = manager.GetWorkingBeatmap(manager.QueryBeatmapSet(b => b.OnlineBeatmapSetID == 1993).Beatmaps.First());
            });
            AddAssert(@"Ruleset changed to fruits", () => songSelect.RulesetID == game.Beatmap.Value.BeatmapInfo.RulesetID);
        }

        private BeatmapSetInfo createTestBeatmapSet(int i)
        {
            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = 1234 + i,
                Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                Metadata = new BeatmapMetadata
                {
                    OnlineBeatmapSetID = 1234 + i,
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = "MONACA " + RNG.Next(0, 9),
                    Title = "Black Song " + RNG.Next(0, 9),
                    AuthorString = "Some Guy " + RNG.Next(0, 9),
                },
                Beatmaps = new List<BeatmapInfo>(new[]
                {
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1234 + i,
                        Ruleset = rulesets.AvailableRulesets.ElementAt(0),
                        RulesetID = 0,
                        Path = "normal.osu",
                        Version = "Normal",
                        BaseDifficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = 3.5f,
                        }
                    },
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1235 + i,
                        Ruleset = rulesets.AvailableRulesets.First(r => r.ID != 0),
                        RulesetID = 1,
                        Path = "hard.taiko",
                        Version = "Hard",
                        BaseDifficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = 5,
                        }
                    },
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1236 + i,
                        Ruleset = rulesets.AvailableRulesets.ElementAt(2),
                        RulesetID = 2,
                        Path = "insane.fruits",
                        Version = "Insane",
                        BaseDifficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = 7,
                        }
                    },
                }),
            };
        }
    }
}
