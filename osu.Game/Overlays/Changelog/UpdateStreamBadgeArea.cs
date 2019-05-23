// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class UpdateStreamBadgeArea : TabControl<APIUpdateStream>
    {
        public UpdateStreamBadgeArea()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddInternal(new Box
            {
                Colour = Color4.Black,
                Alpha = 0.12f,
                RelativeSizeAxes = Axes.Both,
            });
        }

        public void Populate(List<APIUpdateStream> streams)
        {
            Current.Value = null;

            foreach (APIUpdateStream updateStream in streams)
                AddItem(updateStream);
        }

        protected override bool OnHover(HoverEvent e)
        {
            foreach (UpdateStreamBadge streamBadge in TabContainer.Children.OfType<UpdateStreamBadge>())
                streamBadge.EnableDim();

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            foreach (UpdateStreamBadge streamBadge in TabContainer.Children.OfType<UpdateStreamBadge>())
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
