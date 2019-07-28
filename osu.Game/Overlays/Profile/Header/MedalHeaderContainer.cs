// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header
{
    public class MedalHeaderContainer : CompositeDrawable
    {
        private FillFlowContainer badgeFlowContainer;

        public readonly Bindable<User> User = new Bindable<User>();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Alpha = 0;
            AutoSizeAxes = Axes.Y;
            User.ValueChanged += e => updateDisplay(e.NewValue);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.GreySeafoamDarker,
                },
                new Container //artificial shadow
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new ColourInfo
                        {
                            TopLeft = Color4.Black.Opacity(0.2f),
                            TopRight = Color4.Black.Opacity(0.2f),
                            BottomLeft = Color4.Black.Opacity(0),
                            BottomRight = Color4.Black.Opacity(0)
                        }
                    },
                },
                badgeFlowContainer = new FillFlowContainer
                {
                    Direction = FillDirection.Full,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Top = 5 },
                    Spacing = new Vector2(10, 10),
                    Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN, Vertical = 10 },
                }
            };
        }

        private void updateDisplay(User user)
        {
            var badges = user.Badges;
            badgeFlowContainer.Clear();

            if (badges?.Length > 0)
            {
                Show();

                for (var index = 0; index < badges.Length; index++)
                {
                    int displayIndex = index;
                    LoadComponentAsync(new DrawableBadge(badges[index]), asyncBadge =>
                    {
                        // load in stable order regardless of async load order.
                        badgeFlowContainer.Insert(displayIndex, asyncBadge);
                    });
                }
            }
            else
            {
                Hide();
            }
        }
    }
}
