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
        private const float container_margin_y = 20;
        private const float container_margin_x = 85;
        public Action OnSelection;

        public APIChangelog SelectedRelease;

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
                },
            };
            // ok, so this is probably not the best.
            // will need to reflect on this.
            // do we need the changelog to be updateable?
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
