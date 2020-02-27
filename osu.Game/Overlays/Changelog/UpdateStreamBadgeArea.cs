// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Changelog
{
    public class UpdateStreamBadgeArea : TabControl<APIUpdateStream>
    {
        public UpdateStreamBadgeArea()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
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

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            AllowMultiline = true,
        };

        protected override Dropdown<APIUpdateStream> CreateDropdown() => null;

        protected override TabItem<APIUpdateStream> CreateTabItem(APIUpdateStream value) =>
            new UpdateStreamBadge(value) { SelectedTab = { BindTarget = Current } };
    }
}
