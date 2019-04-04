// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestCaseDirect : OsuTestCase
    {
        private DirectOverlay direct;
        private RulesetStore rulesets;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(direct = new DirectOverlay());
            newBeatmaps();

            AddStep(@"toggle", direct.ToggleVisibility);
            AddStep(@"result counts", () => direct.ResultAmounts = new DirectOverlay.ResultCounts(1, 4, 13));
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        private void newBeatmaps()
        {
            var ruleset = rulesets.GetRuleset(0);

            direct.BeatmapSets = new[]
            {
                new BeatmapSetInfo
                {
                    OnlineBeatmapSetID = 578332,
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"OrVid",
                        Artist = @"An",
                        AuthorString = @"RLC",
                        Source = @"",
                        Tags = @"acuticnotes an-fillnote revid tear tearvid encrpted encryption axi axivid quad her hervid recoll",
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Card = @"https://assets.ppy.sh/beatmaps/578332/covers/card.jpg?1494591390",
                            Cover = @"https://assets.ppy.sh/beatmaps/578332/covers/cover.jpg?1494591390",
                        },
                        Preview = @"https://b.ppy.sh/preview/578332.mp3",
                        PlayCount = 97,
                        FavouriteCount = 72,
                    },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 5.35f,
                            Metadata = new BeatmapMetadata(),
                        },
                    },
                },
                new BeatmapSetInfo
                {
                    OnlineBeatmapSetID = 599627,
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"tiny lamp",
                        Artist = @"fhana",
                        AuthorString = @"Sotarks",
                        Source = @"ぎんぎつね",
                        Tags = @"lantis junichi sato yuxuki waga kevin mitsunaga towana gingitsune opening op full ver version kalibe collab collaboration",
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Card = @"https://assets.ppy.sh/beatmaps/599627/covers/card.jpg?1494539318",
                            Cover = @"https://assets.ppy.sh/beatmaps/599627/covers/cover.jpg?1494539318",
                        },
                        Preview = @"https//b.ppy.sh/preview/599627.mp3",
                        PlayCount = 3082,
                        FavouriteCount = 14,
                    },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 5.81f,
                            Metadata = new BeatmapMetadata(),
                        },
                    },
                },
                new BeatmapSetInfo
                {
                    OnlineBeatmapSetID = 513268,
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"At Gwanghwamun",
                        Artist = @"KYUHYUN",
                        AuthorString = @"Cerulean Veyron",
                        Source = @"",
                        Tags = @"soul ballad kh super junior sj suju 슈퍼주니어 kt뮤직 sm엔터테인먼트 s.m.entertainment kt music 1st mini album ep",
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Card = @"https://assets.ppy.sh/beatmaps/513268/covers/card.jpg?1494502863",
                            Cover = @"https://assets.ppy.sh/beatmaps/513268/covers/cover.jpg?1494502863",
                        },
                        Preview = @"https//b.ppy.sh/preview/513268.mp3",
                        PlayCount = 2762,
                        FavouriteCount = 15,
                    },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 0.9f,
                            Metadata = new BeatmapMetadata(),
                        },
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 1.1f,
                        },
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 2.02f,
                        },
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 3.49f,
                        },
                    },
                },
                new BeatmapSetInfo
                {
                    OnlineBeatmapSetID = 586841,
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"RHAPSODY OF BLUE SKY",
                        Artist = @"fhana",
                        AuthorString = @"[Kamiya]",
                        Source = @"小林さんちのメイドラゴン",
                        Tags = @"kobayashi san chi no maidragon aozora no opening anime maid dragon oblivion karen dynamix imoutosan pata-mon gxytcgxytc",
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Card = @"https://assets.ppy.sh/beatmaps/586841/covers/card.jpg?1494052741",
                            Cover = @"https://assets.ppy.sh/beatmaps/586841/covers/cover.jpg?1494052741",
                        },
                        Preview = @"https//b.ppy.sh/preview/586841.mp3",
                        PlayCount = 62317,
                        FavouriteCount = 161,
                    },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 1.26f,
                            Metadata = new BeatmapMetadata(),
                        },
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 2.01f,
                        },
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 2.87f,
                        },
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 3.76f,
                        },
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 3.93f,
                        },
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 4.37f,
                        },
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 5.13f,
                        },
                        new BeatmapInfo
                        {
                            Ruleset = ruleset,
                            StarDifficulty = 5.42f,
                        },
                    },
                },
            };
        }
    }
}
