// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.SearchableList;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi
{
    public class Header : Container
    {
        public const float HEIGHT = 121;

        private readonly HeaderBreadcrumbControl breadcrumbs;

        public Header(ScreenStack stack)
        {
            MultiHeaderTitle title;
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex(@"2f2043"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING + OsuScreen.HORIZONTAL_OVERFLOW_PADDING },
                    Children = new Drawable[]
                    {
                        title = new MultiHeaderTitle
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.BottomLeft,
                            X = -ScreenTitle.ICON_WIDTH,
                        },
                        breadcrumbs = new HeaderBreadcrumbControl(stack)
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
            };

            breadcrumbs.Current.ValueChanged += screen =>
            {
                if (screen.NewValue is IMultiplayerSubScreen multiScreen)
                    title.Screen = multiScreen;
            };

            breadcrumbs.Current.TriggerChange();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            breadcrumbs.StripColour = colours.Green;
        }

        private class MultiHeaderTitle : ScreenTitle
        {
            public IMultiplayerSubScreen Screen
            {
                set => Section = value.ShortTitle.ToLowerInvariant();
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Title = "multi";
                Icon = OsuIcon.Multi;
                AccentColour = colours.Yellow;
            }
        }

        private class HeaderBreadcrumbControl : ScreenBreadcrumbControl
        {
            public HeaderBreadcrumbControl(ScreenStack stack)
                : base(stack)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                AccentColour = Color4.White;
            }
        }
    }
}
