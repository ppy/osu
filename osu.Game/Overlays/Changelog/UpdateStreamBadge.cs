// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Overlays.Changelog
{
    public class UpdateStreamBadge : TabItem<APIUpdateStream>
    {
        private const float badge_width = 100;
        private const float transition_duration = 100;

        public readonly Bindable<APIUpdateStream> SelectedTab = new Bindable<APIUpdateStream>();

        private readonly APIUpdateStream stream;

        private FillFlowContainer<SpriteText> text;
        private ExpandingBar expandingBar;

        public UpdateStreamBadge(APIUpdateStream stream)
            : base(stream)
        {
            this.stream = stream;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Size = new Vector2(stream.IsFeatured ? badge_width * 2 : badge_width, 60);
            Padding = new MarginPadding(5);

            AddRange(new Drawable[]
            {
                text = new FillFlowContainer<SpriteText>
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding { Top = 6 },
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Text = stream.DisplayName,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Black),
                        },
                        new OsuSpriteText
                        {
                            Text = stream.LatestBuild.DisplayVersion,
                            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Regular),
                        },
                        new OsuSpriteText
                        {
                            Text = stream.LatestBuild.Users > 0 ? $"{"user".ToQuantity(stream.LatestBuild.Users, "N0")} online" : null,
                            Font = OsuFont.GetFont(size: 10),
                            Colour = colourProvider.Foreground1
                        },
                    }
                },
                expandingBar = new ExpandingBar
                {
                    Anchor = Anchor.TopCentre,
                    Colour = stream.Colour,
                    ExpandedSize = 4,
                    CollapsedSize = 2,
                    Expanded = true
                },
                new HoverClickSounds()
            });

            SelectedTab.BindValueChanged(_ => updateState(), true);
        }

        protected override void OnActivated() => updateState();

        protected override void OnDeactivated() => updateState();

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private void updateState()
        {
            // highlighted regardless if we are hovered
            bool textHighlighted = IsHovered;
            bool barExpanded = IsHovered;

            if (SelectedTab.Value == null)
            {
                // at listing, all badges are highlighted when user is not hovering any badge.
                textHighlighted |= !userHoveringArea;
                barExpanded |= !userHoveringArea;
            }
            else
            {
                // bar is always expanded when active
                barExpanded |= Active.Value;

                // text is highlighted only when hovered or active (but not if in selection mode)
                textHighlighted |= Active.Value && !userHoveringArea;
            }

            expandingBar.Expanded = barExpanded;
            text.FadeTo(textHighlighted ? 1 : 0.5f, transition_duration, Easing.OutQuint);
        }

        private bool userHoveringArea;

        public bool UserHoveringArea
        {
            set
            {
                if (value == userHoveringArea)
                    return;

                userHoveringArea = value;
                updateState();
            }
        }
    }
}
