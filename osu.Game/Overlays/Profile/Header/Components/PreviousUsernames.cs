// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class PreviousUsernames : CompositeDrawable
    {
        private const int duration = 200;
        private const int margin = 10;
        private const int width = 310;
        private const int move_offset = 15;

        public readonly Bindable<APIUser> User = new Bindable<APIUser>();

        private readonly TextFlowContainer text;
        private readonly Box background;
        private readonly SpriteText header;

        public PreviousUsernames()
        {
            HoverIconContainer hoverIcon;

            AutoSizeAxes = Axes.Y;
            Width = width;
            Masking = true;
            CornerRadius = 5;

            AddRangeInternal(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new GridContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            hoverIcon = new HoverIconContainer(),
                            header = new OsuSpriteText
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Text = UsersStrings.ShowPreviousUsernames,
                                Font = OsuFont.GetFont(size: 10, italics: true)
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            text = new TextFlowContainer(s => s.Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold, italics: true))
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Full,
                                Margin = new MarginPadding { Bottom = margin, Top = margin / 2f }
                            }
                        }
                    }
                }
            });

            hoverIcon.ActivateHover += showContent;
            hideContent();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.GreySeafoamDarker;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(onUserChanged, true);
        }

        private void onUserChanged(ValueChangedEvent<APIUser> user)
        {
            text.Text = string.Empty;

            string[] usernames = user.NewValue?.PreviousUsernames;

            if (usernames?.Any() ?? false)
            {
                text.Text = string.Join(", ", usernames);
                Show();
                return;
            }

            Hide();
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            hideContent();
        }

        private void showContent()
        {
            text.FadeIn(duration, Easing.OutQuint);
            header.FadeIn(duration, Easing.OutQuint);
            background.FadeIn(duration, Easing.OutQuint);
            this.MoveToY(-move_offset, duration, Easing.OutQuint);
        }

        private void hideContent()
        {
            text.FadeOut(duration, Easing.OutQuint);
            header.FadeOut(duration, Easing.OutQuint);
            background.FadeOut(duration, Easing.OutQuint);
            this.MoveToY(0, duration, Easing.OutQuint);
        }

        private class HoverIconContainer : Container
        {
            public Action ActivateHover;

            public HoverIconContainer()
            {
                AutoSizeAxes = Axes.Both;
                Child = new SpriteIcon
                {
                    Margin = new MarginPadding { Top = 6, Left = margin, Right = margin * 2 },
                    Size = new Vector2(15),
                    Icon = FontAwesome.Solid.IdCard,
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                ActivateHover?.Invoke();
                return base.OnHover(e);
            }
        }
    }
}
