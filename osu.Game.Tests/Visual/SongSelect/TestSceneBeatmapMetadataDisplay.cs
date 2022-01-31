// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    public class TestSceneBeatmapMetadataDisplay : OsuTestScene
    {
        private BeatmapMetadataDisplay display;

        [Resolved]
        private BeatmapManager manager { get; set; }

        [Cached(typeof(BeatmapDifficultyCache))]
        private readonly TestBeatmapDifficultyCache testDifficultyCache = new TestBeatmapDifficultyCache();

        [Test]
        public void TestLocal(
            [Values("Beatmap", "Some long title and stuff")]
            string title,
            [Values("Trial", "Some1's very hardest difficulty")]
            string version)
        {
            showMetadataForBeatmap(() => CreateWorkingBeatmap(new Beatmap
            {
                BeatmapInfo =
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = title,
                    },
                    DifficultyName = version,
                    StarRating = RNG.NextDouble(0, 10),
                }
            }));
        }

        [Test]
        public void TestDelayedStarRating()
        {
            AddStep("block calculation", () => testDifficultyCache.BlockCalculation = true);

            showMetadataForBeatmap(() => CreateWorkingBeatmap(new Beatmap
            {
                BeatmapInfo =
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = "Heavy beatmap",
                    },
                    DifficultyName = "10k objects",
                    StarRating = 99.99f,
                }
            }));

            AddStep("allow calculation", () => testDifficultyCache.BlockCalculation = false);
        }

        [Test]
        public void TestRandomFromDatabase()
        {
            showMetadataForBeatmap(() =>
            {
                var allBeatmapSets = manager.GetAllUsableBeatmapSets();
                if (allBeatmapSets.Count == 0)
                    return manager.DefaultBeatmap;

                var randomBeatmapSet = allBeatmapSets[RNG.Next(0, allBeatmapSets.Count - 1)];
                var randomBeatmap = randomBeatmapSet.Beatmaps[RNG.Next(0, randomBeatmapSet.Beatmaps.Count - 1)];

                return manager.GetWorkingBeatmap(randomBeatmap);
            });
        }

        private void showMetadataForBeatmap(Func<IWorkingBeatmap> getBeatmap)
        {
            AddStep("setup display", () =>
            {
                var randomMods = Ruleset.Value.CreateInstance().CreateAllMods().OrderBy(_ => RNG.Next()).Take(5).ToList();

                OsuLogo logo = new OsuLogo { Scale = new Vector2(0.15f) };

                Remove(testDifficultyCache);

                Children = new Drawable[]
                {
                    testDifficultyCache,
                    display = new BeatmapMetadataDisplay(getBeatmap(), new Bindable<IReadOnlyList<Mod>>(randomMods), logo)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0f,
                    }
                };

                display.FadeIn(400, Easing.OutQuint);
            });

            AddWaitStep("wait a bit", 5);

            AddStep("finish loading", () => display.Loading = false);
        }

        private class TestBeatmapDifficultyCache : BeatmapDifficultyCache
        {
            private TaskCompletionSource<bool> calculationBlocker;

            private bool blockCalculation;

            public bool BlockCalculation
            {
                get => blockCalculation;
                set
                {
                    if (value == blockCalculation)
                        return;

                    blockCalculation = value;

                    if (value)
                        calculationBlocker = new TaskCompletionSource<bool>();
                    else
                        calculationBlocker?.SetResult(false);
                }
            }

            public override async Task<StarDifficulty?> GetDifficultyAsync(IBeatmapInfo beatmapInfo, IRulesetInfo rulesetInfo = null, IEnumerable<Mod> mods = null, CancellationToken cancellationToken = default)
            {
                if (blockCalculation)
                    await calculationBlocker.Task.ConfigureAwait(false);

                return await base.GetDifficultyAsync(beatmapInfo, rulesetInfo, mods, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
