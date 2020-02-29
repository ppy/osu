﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneBeatmapListingSearchSection : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BeatmapListingSearchSection),
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly BeatmapListingSearchSection section;

        public TestSceneBeatmapListingSearchSection()
        {
            OsuSpriteText query;
            OsuSpriteText ruleset;
            OsuSpriteText category;

            Add(section = new BeatmapListingSearchSection
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    query = new OsuSpriteText(),
                    ruleset = new OsuSpriteText(),
                    category = new OsuSpriteText(),
                }
            });

            section.Query.BindValueChanged(q => query.Text = $"Query: {q.NewValue}", true);
            section.Ruleset.BindValueChanged(r => ruleset.Text = $"Ruleset: {r.NewValue}", true);
            section.Category.BindValueChanged(c => category.Text = $"Category: {c.NewValue}", true);
        }

        [Test]
        public void TestCovers()
        {
            AddStep("Set beatmap", () => section.BeatmapSet = beatmap_set);
            AddStep("Set beatmap (no cover)", () => section.BeatmapSet = no_cover_beatmap_set);
            AddStep("Set null beatmap", () => section.BeatmapSet = null);
        }

        private static readonly BeatmapSetInfo beatmap_set = new BeatmapSetInfo
        {
            OnlineInfo = new BeatmapSetOnlineInfo
            {
                Covers = new BeatmapSetOnlineCovers
                {
                    Cover = "https://assets.ppy.sh/beatmaps/1094296/covers/cover@2x.jpg?1581416305"
                }
            }
        };

        private static readonly BeatmapSetInfo no_cover_beatmap_set = new BeatmapSetInfo
        {
            OnlineInfo = new BeatmapSetOnlineInfo
            {
                Covers = new BeatmapSetOnlineCovers
                {
                    Cover = string.Empty
                }
            }
        };
    }
}
