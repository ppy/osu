// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public partial class TestSceneBeatmapCardDifficultyList : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [BackgroundDependencyLoader]
        private void load()
        {
            var beatmapSet = new APIBeatmapSet
            {
                Beatmaps = new[]
                {
                    new APIBeatmap { RulesetID = 1, StarRating = 5.76, DifficultyName = "Oni" },
                    new APIBeatmap { RulesetID = 1, StarRating = 3.20, DifficultyName = "Muzukashii" },
                    new APIBeatmap { RulesetID = 1, StarRating = 2.45, DifficultyName = "Futsuu" },

                    new APIBeatmap { RulesetID = 0, StarRating = 2.04, DifficultyName = "Normal" },
                    new APIBeatmap { RulesetID = 0, StarRating = 3.51, DifficultyName = "Hard" },
                    new APIBeatmap { RulesetID = 0, StarRating = 5.25, DifficultyName = "Insane" },

                    new APIBeatmap { RulesetID = 2, StarRating = 2.64, DifficultyName = "Salad" },
                    new APIBeatmap { RulesetID = 2, StarRating = 3.56, DifficultyName = "Platter" },
                    new APIBeatmap { RulesetID = 2, StarRating = 4.65, DifficultyName = "Rain" },

                    new APIBeatmap { RulesetID = 3, StarRating = 1.93, DifficultyName = "[7K] Normal" },
                    new APIBeatmap { RulesetID = 3, StarRating = 3.18, DifficultyName = "[7K] Hyper" },
                    new APIBeatmap { RulesetID = 3, StarRating = 4.82, DifficultyName = "[7K] Another" },

                    new APIBeatmap { RulesetID = 4, StarRating = 9.99, DifficultyName = "Unknown?!" },
                }
            };

            Child = new Container
            {
                Width = 300,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background2
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(10),
                        Child = new BeatmapCardDifficultyList(beatmapSet)
                    }
                }
            };
        }
    }
}
