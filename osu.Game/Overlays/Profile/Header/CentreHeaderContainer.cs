// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class CentreHeaderContainer : CompositeDrawable
    {
        public readonly BindableBool DetailsVisible = new BindableBool(true);
        public readonly Bindable<User> User = new Bindable<User>();

        private OverlinedInfoContainer hiddenDetailGlobal;
        private OverlinedInfoContainer hiddenDetailCountry;

        public CentreHeaderContainer()
        {
            Height = 60;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, TextureStore textures)
        {
            Container<Drawable> hiddenDetailContainer;
            Container<Drawable> expandedDetailContainer;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding { Vertical = 10 },
                    Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN },
                    Spacing = new Vector2(10, 0),
                    Children = new Drawable[]
                    {
                        new FollowersButton
                        {
                            User = { BindTarget = User }
                        },
                        new MappingSubscribersButton
                        {
                            User = { BindTarget = User }
                        },
                        new MessageUserButton
                        {
                            User = { BindTarget = User }
                        },
                    }
                },
                new Container
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Vertical = 10 },
                    Width = UserProfileOverlay.CONTENT_X_MARGIN,
                    Child = new ExpandDetailsButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        DetailsVisible = { BindTarget = DetailsVisible }
                    },
                },
                new Container
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Right = UserProfileOverlay.CONTENT_X_MARGIN },
                    Children = new Drawable[]
                    {
                        new LevelBadge
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Size = new Vector2(40),
                            User = { BindTarget = User }
                        },
                        expandedDetailContainer = new Container
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Width = 200,
                            Height = 6,
                            Margin = new MarginPadding { Right = 50 },
                            Child = new LevelProgressBar
                            {
                                RelativeSizeAxes = Axes.Both,
                                User = { BindTarget = User }
                            }
                        },
                        hiddenDetailContainer = new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Width = 200,
                            AutoSizeAxes = Axes.Y,
                            Alpha = 0,
                            Spacing = new Vector2(10, 0),
                            Margin = new MarginPadding { Right = 50 },
                            Children = new[]
                            {
                                hiddenDetailGlobal = new OverlinedInfoContainer
                                {
                                    Title = UsersStrings.ShowRankGlobalSimple,
                                    LineColour = colourProvider.Highlight1
                                },
                                hiddenDetailCountry = new OverlinedInfoContainer
                                {
                                    Title = UsersStrings.ShowRankCountrySimple,
                                    LineColour = colourProvider.Highlight1
                                },
                            }
                        }
                    }
                }
            };

            DetailsVisible.BindValueChanged(visible =>
            {
                hiddenDetailContainer.FadeTo(visible.NewValue ? 0 : 1, 200, Easing.OutQuint);
                expandedDetailContainer.FadeTo(visible.NewValue ? 1 : 0, 200, Easing.OutQuint);
            });

            User.BindValueChanged(user => updateDisplay(user.NewValue));
        }

        private void updateDisplay(User user)
        {
            hiddenDetailGlobal.Content = user?.Statistics?.GlobalRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-";
            hiddenDetailCountry.Content = user?.Statistics?.CountryRank?.ToLocalisableString("\\##,##0") ?? (LocalisableString)"-";
        }
    }
}
