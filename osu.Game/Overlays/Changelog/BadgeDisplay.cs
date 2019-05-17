// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class BadgeDisplay : CompositeDrawable
    {
        private const float vertical_padding = 20;
        private const float horizontal_padding = 85;

        public readonly Bindable<APIUpdateStream> Current = new Bindable<APIUpdateStream>();

        private readonly FillFlowContainer<StreamBadge> badgesContainer;

        public BadgeDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(32, 24, 35, 255),
                },
                badgesContainer = new FillFlowContainer<StreamBadge>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Vertical = vertical_padding, Horizontal = horizontal_padding },
                },
            };

            Current.ValueChanged += e =>
            {
                foreach (StreamBadge streamBadge in badgesContainer)
                {
                    if (!IsHovered || e.NewValue.Id == streamBadge.Stream.Id)
                        streamBadge.Activate();
                    else
                        streamBadge.Deactivate();
                }
            };
        }

        public void Populate(List<APIUpdateStream> streams)
        {
            Current.Value = null;

            foreach (APIUpdateStream updateStream in streams)
            {
                var streamBadge = new StreamBadge(updateStream);
                streamBadge.Selected += () => Current.Value = updateStream;
                badgesContainer.Add(streamBadge);
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            foreach (StreamBadge streamBadge in badgesContainer.Children)
                streamBadge.EnableDim();

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            foreach (StreamBadge streamBadge in badgesContainer.Children)
                streamBadge.DisableDim();

            base.OnHoverLost(e);
        }
    }
}
