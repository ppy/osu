// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Game.Online.API.Requests;
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
            OsuSpriteText genre;
            OsuSpriteText language;

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
                    genre = new OsuSpriteText(),
                    language = new OsuSpriteText(),
                }
            });

            section.SearchParameters.BindValueChanged(parameters =>
            {
                query.Text = $"Query: {parameters.NewValue.Query}";
                ruleset.Text = $"Ruleset: {parameters.NewValue.Ruleset}";
                category.Text = $"Category: {parameters.NewValue.Category}";
                genre.Text = $"Genre: {parameters.NewValue.Genre}";
                language.Text = $"Language: {parameters.NewValue.Language}";
            }, true);
        }

        [Test]
        public void TestCovers()
        {
            AddStep("Set beatmap", () => section.BeatmapSet = beatmap_set);
            AddStep("Set beatmap (no cover)", () => section.BeatmapSet = no_cover_beatmap_set);
            AddStep("Set null beatmap", () => section.BeatmapSet = null);
        }

        [Test]
        public void TestParametersSet()
        {
            AddStep("Set big black tag", () => section.SetTag("big black"));
            AddAssert("Check query is big black", () => section.SearchParameters.Value.Query == "big black");
            AddStep("Set anime genre", () => section.SetGenre(BeatmapSearchGenre.Anime));
            AddAssert("Check genre is anime", () => section.SearchParameters.Value.Genre == BeatmapSearchGenre.Anime);
            AddStep("Set japanese language", () => section.SetLanguage(BeatmapSearchLanguage.Japanese));
            AddAssert("Check language is japanese", () => section.SearchParameters.Value.Language == BeatmapSearchLanguage.Japanese);
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
