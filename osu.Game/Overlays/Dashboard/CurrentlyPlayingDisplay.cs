// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Spectator;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard
{
    internal class CurrentlyPlayingDisplay : CompositeDrawable
    {
        private readonly IBindableList<int> playingUsers = new BindableList<int>();

        private FillFlowContainer<PlayingUserPanel> userFlow;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = userFlow = new FillFlowContainer<PlayingUserPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(10),
                Spacing = new Vector2(10),
            };
        }

        [Resolved]
        private UserLookupCache users { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playingUsers.BindTo(spectatorClient.PlayingUsers);
            playingUsers.BindCollectionChanged(onUsersChanged, true);
        }

        private void onUsersChanged(object sender, NotifyCollectionChangedEventArgs e) => Schedule(() =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (int id in e.NewItems.OfType<int>().ToArray())
                    {
                        users.GetUserAsync(id).ContinueWith(u =>
                        {
                            if (u.Result == null) return;

                            Schedule(() =>
                            {
                                // user may no longer be playing.
                                if (!playingUsers.Contains(u.Result.Id))
                                    return;

                                userFlow.Add(createUserPanel(u.Result));
                            });
                        });
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (int u in e.OldItems.OfType<int>())
                        userFlow.FirstOrDefault(card => card.User.Id == u)?.Expire();
                    break;

                case NotifyCollectionChangedAction.Reset:
                    userFlow.Clear();
                    break;
            }
        });

        private PlayingUserPanel createUserPanel(APIUser user) =>
            new PlayingUserPanel(user).With(panel =>
            {
                panel.Anchor = Anchor.TopCentre;
                panel.Origin = Anchor.TopCentre;
            });

        private class PlayingUserPanel : CompositeDrawable
        {
            public readonly APIUser User;

            [Resolved(canBeNull: true)]
            private OsuGame game { get; set; }

            public PlayingUserPanel(APIUser user)
            {
                User = user;

                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(IAPIProvider api)
            {
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(2),
                        Width = 290,
                        Children = new Drawable[]
                        {
                            new UserGridPanel(User)
                            {
                                RelativeSizeAxes = Axes.X,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            new PurpleTriangleButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Text = "Watch",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Action = () => game?.PerformFromScreen(s => s.Push(new SoloSpectator(User))),
                                Enabled = { Value = User.Id != api.LocalUser.Value.Id }
                            }
                        }
                    },
                };
            }
        }
    }
}
