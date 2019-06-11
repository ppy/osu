// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapNotAvailable : Container
    {
        private BeatmapSetInfo beatmapSet;

        public BeatmapSetInfo BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet) return;

                beatmapSet = value;

                if (beatmapSet?.OnlineInfo.Availability != null)
                {
                    Header?.ResizeHeightTo(450, 500);
                    Show();
                }
                else
                {
                    Header?.ResizeHeightTo(400, 500);
                    Hide();
                }
            }
        }

        public Header Header;

        private readonly OsuSpriteText text;
        private readonly LinkFlowContainer link;

        public BeatmapNotAvailable()
        {
            AutoSizeAxes = Axes.Both;
            Margin = new MarginPadding { Top = 10 };

            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black.Opacity(0.6f),
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding { Top = 10, Left = 5, Right = 20 },

                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Margin = new MarginPadding { Bottom = 10, Horizontal = 5 },
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Medium),
                            Colour = Color4.Orange,
                        },
                        link = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: 14))
                        {
                            Direction = FillDirection.Full,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Bottom = 10, Horizontal = 5 },
                        },
                    },
                },
            };

            Hide();
        }

        public override void Show()
        {
            text.Text = BeatmapSet.OnlineInfo.Availability.DownloadDisabled
                ? "This beatmap is currently not available for download."
                : "Portions of this beatmap have been removed at the request of the creator or a third-party rights holder.";

            link.AddLink("Check here for more information.", BeatmapSet.OnlineInfo.Availability.ExternalLink);

            base.Show();
        }

        public override void Hide()
        {
            link.RemoveAll(x => true);

            base.Hide();
        }
    }
}
