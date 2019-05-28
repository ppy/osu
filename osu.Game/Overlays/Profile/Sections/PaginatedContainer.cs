// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Framework.Input.Events;
using System;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class PaginatedContainer : FillFlowContainer
    {
        protected readonly FillFlowContainer ItemsContainer;
        protected readonly ShowMoreButton MoreButton;
        protected readonly OsuSpriteText MissingText;

        protected int VisiblePages;
        protected int ItemsPerPage;

        protected readonly Bindable<User> User = new Bindable<User>();

        protected IAPIProvider Api;
        protected APIRequest RetrievalRequest;
        protected RulesetStore Rulesets;

        protected PaginatedContainer(Bindable<User> user, string header, string missing)
        {
            User.BindTo(user);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;

            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = header,
                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                    Margin = new MarginPadding { Top = 10, Bottom = 10 },
                },
                ItemsContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 2),
                },
                MoreButton = new ShowMoreButton
                {
                    Alpha = 0,
                    Margin = new MarginPadding { Top = 10 },
                    Action = ShowMore,
                },
                MissingText = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 15),
                    Text = missing,
                    Alpha = 0,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, RulesetStore rulesets)
        {
            Api = api;
            Rulesets = rulesets;

            User.ValueChanged += onUserChanged;
            User.TriggerChange();
        }

        private void onUserChanged(ValueChangedEvent<User> e)
        {
            VisiblePages = 0;
            ItemsContainer.Clear();

            if (e.NewValue != null)
                ShowMore();
        }

        protected abstract void ShowMore();

        protected class ShowMoreButton : CircularContainer
        {
            private const int duration = 300;
            private Color4 idleColour;
            private Color4 hoveredColour;

            public Action Action;
            private readonly Box background;
            private readonly LoadingAnimation loading;
            private readonly FillFlowContainer content;

            private bool isLoading;

            public bool IsLoading
            {
                get => isLoading;
                set
                {
                    if (isLoading == value)
                        return;

                    isLoading = value;

                    if (value)
                    {
                        loading.FadeIn(duration, Easing.OutQuint);
                        content.FadeOut(duration, Easing.OutQuint);
                    }
                    else
                    {
                        loading.FadeOut(duration, Easing.OutQuint);
                        content.FadeIn(duration, Easing.OutQuint);
                    }
                }
            }

            public ShowMoreButton()
            {
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                Masking = true;
                Size = new Vector2(140, 30);
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    content = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(7),
                        Children = new Drawable[]
                        {
                            new ChevronIcon(),
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                                Text = "show more".ToUpper(),
                            },
                            new ChevronIcon(),
                        }
                    },
                    loading = new LoadingAnimation
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(20)
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colors)
            {
                background.Colour = idleColour = colors.GreySeafoam;
                hoveredColour = colors.GreySeafoamLight;
            }

            protected override bool OnHover(HoverEvent e)
            {
                background.FadeColour(hoveredColour, duration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                background.FadeColour(idleColour, duration, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                IsLoading = true;
                Action.Invoke();
                return base.OnClick(e);
            }

            private class ChevronIcon : SpriteIcon
            {
                private const int bottom_margin = 2;
                private const int icon_size = 8;

                public ChevronIcon()
                {
                    Anchor = Anchor.Centre;
                    Origin = Anchor.Centre;
                    Margin = new MarginPadding { Bottom = bottom_margin };
                    Size = new Vector2(icon_size);
                    Icon = FontAwesome.Solid.ChevronDown;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colors)
                {
                    Colour = colors.Yellow;
                }
            }
        }
    }
}
