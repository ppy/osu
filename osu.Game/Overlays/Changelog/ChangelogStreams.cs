// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Online.API.Requests.Responses;
using System;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogStreams : Container
    {
        private const float container_height = 106.5f;
        private const float padding_y = 20;
        private const float padding_x = 85;
        public Action OnSelection;

        public APIChangelog SelectedRelease;
        // not using SelectedRelease as a Bindable and then using .OnValueChange instead of OnSelection
        // because it doesn't "refresh" the selection if the same stream is chosen

        public readonly FillFlowContainer<StreamBadge> BadgesContainer;

        public ChangelogStreams()
        {
            // this should actually be resizeable (https://streamable.com/yw2ug)
            // if not, with small width:height ratio it cuts off right-most content
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(32, 24, 35, 255),
                },
                BadgesContainer = new FillFlowContainer<StreamBadge>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding
                    {
                        Top = padding_y,
                        Bottom = padding_y,
                        Left = padding_x,
                        Right = padding_x,
                    },
                },
            };
            // ok, so this is probably not the best.
            // how else can this be done?
            BadgesContainer.OnUpdate = d =>
            {
                foreach (StreamBadge streamBadge in BadgesContainer.Children)
                {
                    streamBadge.OnActivation = () =>
                    {
                        SelectedRelease = streamBadge.ChangelogEntry;
                        foreach (StreamBadge item in BadgesContainer.Children)
                        {
                            if (item.ChangelogEntry.Id != streamBadge.ChangelogEntry.Id) item.Deactivate();
                        }
                        OnSelection?.Invoke();
                    };
                }
            };
        }

        protected override bool OnHover(InputState state)
        {
            // is this nullreference-safe for badgesContainer?
            foreach (StreamBadge streamBadge in BadgesContainer.Children)
            {
                if (SelectedRelease != null)
                {
                    if (SelectedRelease.UpdateStream.Id != streamBadge.ChangelogEntry.Id)
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
            if (SelectedRelease == null)
            {
                foreach (StreamBadge streamBadge in BadgesContainer.Children) streamBadge.Activate(true);
            }
            base.OnHoverLost(state);
        }
    }
}
