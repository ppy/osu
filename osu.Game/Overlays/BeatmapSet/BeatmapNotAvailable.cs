// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
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

                removeLinks();

                if (beatmapSet?.OnlineInfo.Availability != null)
                    Show();
                else
                    Hide();
            }
        }

        private readonly OsuTextFlowContainer text;
        private readonly LinkFlowContainer link;

        public BeatmapNotAvailable()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Top = 10, Right = 20 };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.6f),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding { Top = 10, Left = 5, Right = 20 },

                    Children = new Drawable[]
                    {
                        text = new OsuTextFlowContainer(t => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.Medium))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Bottom = 10, Horizontal = 5 },
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

        private void removeLinks() => link?.RemoveAll(x => true);
    }
}
