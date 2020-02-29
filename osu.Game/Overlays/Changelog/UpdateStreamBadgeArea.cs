﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Changelog
{
    public class UpdateStreamBadgeArea : TabControl<APIUpdateStream>
    {
        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddInternal(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Background5,
            });
        }

        public void Populate(List<APIUpdateStream> streams)
        {
            foreach (var updateStream in streams)
                AddItem(updateStream);
        }

        protected override bool OnHover(HoverEvent e)
        {
            foreach (var streamBadge in TabContainer.Children.OfType<UpdateStreamBadge>())
                streamBadge.EnableDim();

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            foreach (var streamBadge in TabContainer.Children.OfType<UpdateStreamBadge>())
                streamBadge.DisableDim();

            base.OnHoverLost(e);
        }

        protected override TabFillFlowContainer CreateTabFlow()
        {
            var flow = base.CreateTabFlow();

            flow.RelativeSizeAxes = Axes.X;
            flow.AutoSizeAxes = Axes.Y;
            flow.AllowMultiline = true;
            flow.Padding = new MarginPadding
            {
                Vertical = 20,
                Horizontal = 85,
            };

            return flow;
        }

        protected override Dropdown<APIUpdateStream> CreateDropdown() => null;

        protected override TabItem<APIUpdateStream> CreateTabItem(APIUpdateStream value) =>
            new UpdateStreamBadge(value) { SelectedTab = { BindTarget = Current } };
    }
}
