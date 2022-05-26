// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Spectator;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard
{
    internal class CurrentlyPlayingDisplay : CompositeDrawable
    {
        private const float search_bar_height = 40;
        private const float search_bar_width = 250;

        private readonly IBindableList<int> playingUsers = new BindableList<int>();

        private FillFlowContainer<PlayingUserPanel> userFlow;

        private List<int> currentUsers = new List<int>();
        private FocusedTextBox searchBar;
        private Container<FocusedTextBox> searchBarContainer;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            searchBarContainer = new Container<FocusedTextBox>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Padding = new MarginPadding(10),
                Child = searchBar = new FocusedTextBox
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Height = search_bar_height,
                    Width = search_bar_width,

                    Colour = OsuColour.Gray(0.8f),

                    PlaceholderText = "Search for User...",
                    HoldFocus = true,
                    ReleaseFocusOnCommit = true,
                },
            };

            userFlow = new FillFlowContainer<PlayingUserPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding {
                    Top = 10 + 10 + search_bar_height,
                    Bottom = 10,
                    Right = 10,
                    Left = 10,
                },
                Spacing = new Vector2(10),
            };

            InternalChildren = new Drawable[]
            {
                searchBarContainer,
                userFlow,
            };

            searchBar.Current.ValueChanged += onSearchBarValueChanged;
        }

        [Resolved]
        private UserLookupCache users { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playingUsers.BindTo(spectatorClient.PlayingUsers);
            playingUsers.BindCollectionChanged(onPlayingUsersChanged, true);
        }

        private void addCard(int userId)
        {
            if (currentUsers.Contains(userId))
                return;

            users.GetUserAsync(userId).ContinueWith(task =>
            {
                var user = task.GetResultSafely();

                if (user == null)
                    return;

                if (!user.Username.ToLower().Contains(searchBar.Text.ToLower()))
                    return;

                Schedule(() =>
                {
                    // user may no longer be playing.
                    if (!playingUsers.Contains(user.Id))
                        return;

                    currentUsers.Add(user.Id);

                    userFlow.Add(createUserPanel(user));
                });
            });
        }

        private void removeCard(PlayingUserPanel card)
        {
            if (card == null)
                return;

            currentUsers.Remove(card.User.Id);
            card.Expire();
        }

        private void onSearchBarValueChanged(ValueChangedEvent<string> change)
        {
            foreach (PlayingUserPanel card in userFlow)
            {
                if (!card.User.Username.ToLower().Contains(change.NewValue.ToLower()))
                    removeCard(card);
            }

            foreach (int userId in playingUsers)
            {
                addCard(userId);
            }
        }

        private void onPlayingUsersChanged(object sender, NotifyCollectionChangedEventArgs e) => Schedule(() =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    foreach (int userId in e.NewItems)
                        addCard(userId);

                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (int userId in e.OldItems)
                        removeCard(userFlow.FirstOrDefault(card => card.User.Id == userId));
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
            private IPerformFromScreenRunner performer { get; set; }

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
                                Text = "Spectate",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Action = () => performer?.PerformFromScreen(s => s.Push(new SoloSpectator(User))),
                                Enabled = { Value = User.Id != api.LocalUser.Value.Id }
                            }
                        }
                    },
                };
            }
        }
    }
}
