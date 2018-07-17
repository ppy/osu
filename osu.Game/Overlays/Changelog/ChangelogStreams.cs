// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays.Changelog.Streams;
using System;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogStreams : Container
    {
        private const float containerHeight = 106.5f;
        private const float containerTopBottomMargin = 20;
        private const float containerSideMargin = 85;

        public Bindable<ReleaseStreamInfo> SelectedRelease = new Bindable<ReleaseStreamInfo>();

        private readonly StreamColour streamColour;
        public readonly FillFlowContainer<StreamBadge> badgesContainer;
        
        public ChangelogStreams()
        {
            streamColour = new StreamColour();
            Height = containerHeight;
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new OpenTK.Vector2(1),
                    Colour = new Color4(32, 24, 35, 255),
                },
                badgesContainer = new FillFlowContainer<StreamBadge>
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Both,
                    Margin = new MarginPadding()
                    {
                        Top = containerTopBottomMargin,
                        Bottom = containerTopBottomMargin,
                        Left = containerSideMargin,
                        Right = containerSideMargin,
                    },
                    Children = new[]
                    {
                        new StreamBadge(streamColour.Stable, "Stable", "20180626.1", 16370, true),
                        new StreamBadge(streamColour.Beta, "Beta", "20180626", 186),
                        new StreamBadge(streamColour.Lazer, "Lazer", "2018.713.1"),
                    },
                },
            };
            badgesContainer.OnLoadComplete = d =>
            {
                foreach (StreamBadge streamBadge in badgesContainer.Children)
                {
                    streamBadge.OnActivation = () =>
                    {
                        SelectedRelease.Value = new ReleaseStreamInfo()
                        {
                            DisplayVersion = streamBadge.DisplayVersion,
                            IsFeatured = streamBadge.IsFeatured,
                            Name = streamBadge.Name,
                            Users = streamBadge.Users,
                        };
                        foreach (StreamBadge item in badgesContainer.Children)
                        {
                            if (item.Name != streamBadge.Name) item.Deactivate();
                        }
                    };
                }
            };
        }
    }
}
