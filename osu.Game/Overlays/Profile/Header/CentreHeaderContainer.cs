// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile.Header.Components;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public partial class CentreHeaderContainer : CompositeDrawable
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        private LevelBadge levelBadge = null!;

        public CentreHeaderContainer()
        {
            Height = 60;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding { Vertical = 10 },
                    Margin = new MarginPadding { Left = WaveOverlayContainer.HORIZONTAL_PADDING },
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
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Right = WaveOverlayContainer.HORIZONTAL_PADDING },
                    Children = new Drawable[]
                    {
                        levelBadge = new LevelBadge
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Size = new Vector2(40)
                        },
                        new Container
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Width = 200,
                            Height = 6,
                            Margin = new MarginPadding { Right = WaveOverlayContainer.HORIZONTAL_PADDING },
                            Child = new LevelProgressBar
                            {
                                RelativeSizeAxes = Axes.Both,
                                User = { BindTarget = User }
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(user => updateDisplay(user.NewValue?.User), true);
        }

        private void updateDisplay(APIUser? user)
        {
            levelBadge.LevelInfo.Value = user?.Statistics?.Level;
        }
    }
}
