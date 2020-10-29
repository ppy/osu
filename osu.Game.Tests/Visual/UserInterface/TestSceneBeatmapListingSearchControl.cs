// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
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
    public class TestSceneBeatmapListingSearchControl : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly BeatmapListingSearchControl control;

        public TestSceneBeatmapListingSearchControl()
        {
            OsuSpriteText query;
            OsuSpriteText ruleset;
            OsuSpriteText category;
            OsuSpriteText genre;
            OsuSpriteText language;
            OsuSpriteText extra;
            OsuSpriteText ranks;
            OsuSpriteText played;

            Add(control = new BeatmapListingSearchControl
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
                    extra = new OsuSpriteText(),
                    ranks = new OsuSpriteText(),
                    played = new OsuSpriteText()
                }
            });

            control.Query.BindValueChanged(q => query.Text = $"Query: {q.NewValue}", true);
            control.Ruleset.BindValueChanged(r => ruleset.Text = $"Ruleset: {r.NewValue}", true);
            control.Category.BindValueChanged(c => category.Text = $"Category: {c.NewValue}", true);
            control.Genre.BindValueChanged(g => genre.Text = $"Genre: {g.NewValue}", true);
            control.Language.BindValueChanged(l => language.Text = $"Language: {l.NewValue}", true);
            control.Extra.BindCollectionChanged((u, v) => extra.Text = $"Extra: {(control.Extra.Any() ? string.Join('.', control.Extra.Select(i => i.ToString().ToLowerInvariant())) : "")}", true);
            control.Ranks.BindCollectionChanged((u, v) => ranks.Text = $"Ranks: {(control.Ranks.Any() ? string.Join('.', control.Ranks.Select(i => i.ToString())) : "")}", true);
            control.Played.BindValueChanged(p => played.Text = $"Played: {p.NewValue}", true);
        }

        [Test]
        public void TestCovers()
        {
            AddStep("Set beatmap", () => control.BeatmapSet = beatmap_set);
            AddStep("Set beatmap (no cover)", () => control.BeatmapSet = no_cover_beatmap_set);
            AddStep("Set null beatmap", () => control.BeatmapSet = null);
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
