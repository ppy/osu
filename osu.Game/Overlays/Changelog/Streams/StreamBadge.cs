// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using System;

namespace osu.Game.Overlays.Changelog.Streams
{
    public class StreamBadge : ClickableContainer
    {
        private const float badgeHeight = 56.5f;
        private const float badgeWidth = 100;
        private const float badgeTopBottomMargin = 5;
        private const float transition_duration = 100;

        public Action OnActivation;

        private bool isActive;

        private Header.LineBadge lineBadge;

        public string Name { get; private set; }
        public string DisplayVersion { get; private set; }
        public bool IsFeatured { get; private set; }
        public float Users { get; private set; }

        public StreamBadge(ColourInfo colour, string streamName, string streamBuild, float onlineUsers = 0, bool isFeatured = false)
        {
            Name = streamName;
            DisplayVersion = streamBuild;
            IsFeatured = isFeatured;
            Users = onlineUsers;
            Height = badgeHeight;
            Width = isFeatured ? badgeWidth * 2 : badgeWidth;
            Margin = new MarginPadding(5);
            isActive = true;
            Children = new Drawable[]
            {
                new FillFlowContainer<SpriteText>
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new[]
                    {
                        new SpriteText
                        {
                            Text = streamName,
                            Font = @"Exo2.0-Bold",
                            TextSize = 16,
                            Margin = new MarginPadding
                            {
                                Top = 7,
                            }
                        },
                        new SpriteText
                        {
                            Text = streamBuild,
                            Font = @"Exo2.0-Light",
                            TextSize = 21,
                        },
                        new SpriteText
                        {
                            Text = onlineUsers > 0 ?
                                string.Join(" ", onlineUsers.ToString("N0"), "users online"):
                                null,
                            TextSize = 12,
                            Font = @"Exo2.0-Regular",
                            Colour = new Color4(203, 164, 218, 255),
                        },
                    }
                },
                lineBadge = new Header.LineBadge(false, 2, 4)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 1,
                    Colour = colour,
                    RelativeSizeAxes = Axes.X,
                },
            };
        }

        public void Activate(bool withoutHeaderUpdate = false)
        {
            isActive = true;
            this.FadeIn(transition_duration);
            lineBadge.IsCollapsed = false;
            if (!withoutHeaderUpdate) OnActivation?.Invoke();
        }

        public void Deactivate()
        {
            isActive = false;
            this.FadeTo(0.5f, transition_duration);
            lineBadge.IsCollapsed = true;
        }

        protected override bool OnClick(InputState state)
        {
            Activate();
            return base.OnClick(state);
        }

        protected override bool OnHover(InputState state)
        {
            this.FadeIn(transition_duration);
            lineBadge.IsCollapsed = false;
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (!isActive)
            {
                this.FadeTo(0.5f, transition_duration);
                lineBadge.IsCollapsed = true;
            }
            base.OnHoverLost(state);
        }
    }
}
