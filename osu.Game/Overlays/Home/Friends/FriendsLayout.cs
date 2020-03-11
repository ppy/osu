// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Social;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Home.Friends
{
    public class FriendsLayout : CompositeDrawable
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly Bindable<User> localUser = new Bindable<User>();

        private GetFriendsRequest request;
        private CancellationTokenSource cancellationToken;

        private readonly Box background;
        private readonly Box controlBackground;
        private readonly FriendsOnlineStatusControl onlineStatusControl;
        private readonly Container itemsPlaceholder;

        public FriendsLayout()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            controlBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding
                                {
                                    Top = 20,
                                    Horizontal = 45
                                },
                                Child = onlineStatusControl = new FriendsOnlineStatusControl(),
                            }
                        }
                    },
                    new Container
                    {
                        Name = "User List",
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            background = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Margin = new MarginPadding { Bottom = 20 },
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding
                                        {
                                            Horizontal = 40,
                                            Vertical = 20
                                        },
                                        Child = new UserListToolbar
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                        }
                                    },
                                    itemsPlaceholder = new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Horizontal = 50 }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background4;
            controlBackground.Colour = colourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(_ => refetch(), true);
        }

        private void refetch()
        {
            request?.Cancel();
            request = new GetFriendsRequest();
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void onSuccess(List<User> users)
        {
            onlineStatusControl.Clear();
            onlineStatusControl.Populate(users);

            recreatePanels(users);
        }

        private void recreatePanels(List<User> users)
        {
            cancellationToken?.Cancel();

            var table = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10),
                Children = users.Select(u => new SocialGridPanel(u).With(panel =>
                {
                    panel.Anchor = Anchor.TopCentre;
                    panel.Origin = Anchor.TopCentre;
                })).ToList()
            };

            LoadComponentAsync(table, loaded =>
            {
                itemsPlaceholder.Clear();
                itemsPlaceholder.Add(loaded);
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }
    }
}
