// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards
{
    public partial class RankedPlayCardContent
    {
        private partial class CardCover(APIBeatmap beatmap) : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(CardColours colours)
            {
                BufferedContainer coverContainer;

                InternalChildren =
                [
                    coverContainer = new BufferedContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        GrayscaleStrength = 0.25f,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(colours.Background.Opacity(0.2f), colours.Background.Opacity(0.65f))
                    }
                ];

                var cover = new OnlineBeatmapSetCover(beatmap.BeatmapSet)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill,
                    EdgeSmoothness = new Vector2(2),
                };

                LoadComponentAsync(cover, _ =>
                {
                    coverContainer.Add(cover);
                    cover.FadeInFromZero(200);
                });
            }
        }
    }
}
