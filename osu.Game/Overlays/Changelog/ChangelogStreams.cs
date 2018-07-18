// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Overlays.Changelog.Streams;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogStreams : Container
    {
        private const float container_height = 106.5f;
        private const float container_margin_y = 20;
        private const float container_margin_x = 85;

        public Bindable<ReleaseStreamInfo> SelectedRelease = new Bindable<ReleaseStreamInfo>();

        public readonly FillFlowContainer<StreamBadge> BadgesContainer;

        public ChangelogStreams()
        {
            Height = container_height;
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new OpenTK.Vector2(1),
                    Colour = new Color4(32, 24, 35, 255),
                },
                BadgesContainer = new FillFlowContainer<StreamBadge>
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Both,
                    Margin = new MarginPadding
                    {
                        Top = container_margin_y,
                        Bottom = container_margin_y,
                        Left = container_margin_x,
                        Right = container_margin_x,
                    },
                    Children = new[]
                    {
                        new StreamBadge(StreamColour.STABLE, "Stable", "20180626.1", 16370, true),
                        new StreamBadge(StreamColour.BETA, "Beta", "20180626", 186),
                        new StreamBadge(StreamColour.LAZER, "Lazer", "2018.713.1"),
                    },
                },
            };
            BadgesContainer.OnLoadComplete = d =>
            {
                foreach (StreamBadge streamBadge in BadgesContainer.Children)
                {
                    streamBadge.OnActivation = () =>
                    {
                        SelectedRelease.Value = new ReleaseStreamInfo
                        {
                            DisplayVersion = streamBadge.DisplayVersion,
                            IsFeatured = streamBadge.IsFeatured,
                            Name = streamBadge.Name,
                            Users = streamBadge.Users,
                        };
                        foreach (StreamBadge item in BadgesContainer.Children)
                        {
                            if (item.Name != streamBadge.Name) item.Deactivate();
                        }
                    };
                }
            };
        }

        protected override bool OnHover(InputState state)
        {
            // is this nullreference-safe for badgesContainer?
            foreach (StreamBadge streamBadge in BadgesContainer.Children)
            {
                if (SelectedRelease.Value != null)
                {
                    if (SelectedRelease.Value.Name != streamBadge.Name)
                    {
                        streamBadge.Deactivate();
                    }
                }
                else streamBadge.Deactivate();
            }
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (SelectedRelease.Value == null)
            {
                foreach (StreamBadge streamBadge in BadgesContainer.Children) streamBadge.Activate(true);
            }
            base.OnHoverLost(state);
        }
    }
}
