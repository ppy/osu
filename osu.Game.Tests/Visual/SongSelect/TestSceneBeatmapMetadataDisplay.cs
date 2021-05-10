// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    public class TestSceneBeatmapMetadataDisplay : OsuTestScene
    {
        private BeatmapMetadataDisplay display;

        [Resolved]
        private BeatmapManager manager { get; set; }

        [Test]
        public void TestLocal([Values("Beatmap", "Some long title and stuff")]
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
                    Version = version,
                    StarDifficulty = RNG.NextDouble(0, 10),
                }
            }));
        }

        [Test]
        public void TestRandomFromDatabase()
        {
            showMetadataForBeatmap(() =>
            {
                var allBeatmapSets = manager.GetAllUsableBeatmapSets(IncludedDetails.Minimal);
                if (allBeatmapSets.Count == 0)
                    return manager.DefaultBeatmap;

                var randomBeatmapSet = allBeatmapSets[RNG.Next(0, allBeatmapSets.Count - 1)];
                var randomBeatmap = randomBeatmapSet.Beatmaps[RNG.Next(0, randomBeatmapSet.Beatmaps.Count - 1)];

                return manager.GetWorkingBeatmap(randomBeatmap);
            });
        }

        private void showMetadataForBeatmap(Func<WorkingBeatmap> getBeatmap)
        {
            AddStep("setup display", () =>
            {
                var randomMods = Ruleset.Value.CreateInstance().GetAllMods().OrderBy(_ => RNG.Next()).Take(5).ToList();

                Child = display = new BeatmapMetadataDisplay(getBeatmap(), new Bindable<IReadOnlyList<Mod>>(randomMods), Empty())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1.5f),
                    Alpha = 0f,
                };
            });

            AddStep("fade in", () => display.FadeIn(400, Easing.OutQuint));
            AddToggleStep("trigger loading", v => display.Loading = v);
        }
    }
}
