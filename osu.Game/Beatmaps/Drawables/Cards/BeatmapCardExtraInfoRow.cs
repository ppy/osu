// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCardExtraInfoRow : CompositeDrawable
    {
        [Resolved(CanBeNull = true)]
        private BeatmapCardContent? content { get; set; }

        public BeatmapCardExtraInfoRow(APIBeatmapSet beatmapSet)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(4, 0),
                Children = new Drawable[]
                {
                    new BeatmapSetOnlineStatusPill
                    {
                        AutoSizeAxes = Axes.Both,
                        Status = beatmapSet.Status,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft
                    },
                    new DifficultySpectrumDisplay(beatmapSet)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        DotSize = new Vector2(6, 12)
                    }
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            content?.ExpandAfterDelay();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (content?.Expanded.Value == false)
                content.CancelExpand();

            base.OnHoverLost(e);
        }
    }
}
