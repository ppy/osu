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
    public class BeatmapAvailability : Container
    {
        private BeatmapSetInfo beatmapSet;

        private bool downloadDisabled => BeatmapSet?.OnlineInfo.Availability?.DownloadDisabled ?? false;
        private bool hasExternalLink => !string.IsNullOrEmpty(BeatmapSet?.OnlineInfo.Availability?.ExternalLink);

        public BeatmapSetInfo BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet)
                    return;

                beatmapSet = value;

                linkContainer.Clear();

                if (downloadDisabled || hasExternalLink)
                {
                    Show();
                    updateText();
                }
                else Hide();
            }
        }

        private readonly TextFlowContainer textContainer;
        private readonly LinkFlowContainer linkContainer;

        public BeatmapAvailability()
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
                    Padding = new MarginPadding(10),
                    Children = new Drawable[]
                    {
                        textContainer = new TextFlowContainer(t => t.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Medium))
                        {
                            Direction = FillDirection.Full,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Colour = Color4.Orange,
                        },
                        linkContainer = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: 10))
                        {
                            Direction = FillDirection.Full,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Top = 10 },
                        },
                    },
                },
            };
        }

        private void updateText()
        {
            textContainer.Text = downloadDisabled
                ? "This beatmap is currently not available for download."
                : "Portions of this beatmap have been removed at the request of the creator or a third-party rights holder.";

            if (hasExternalLink)
                linkContainer.AddLink("Check here for more information.", BeatmapSet.OnlineInfo.Availability.ExternalLink);
        }
    }
}
