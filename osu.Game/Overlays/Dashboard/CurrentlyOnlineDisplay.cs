// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Online.Spectator;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard
{
    internal partial class CurrentlyOnlineDisplay : CompositeDrawable
    {
        private const float search_textbox_height = 40;
        private const float padding = 10;

        private readonly IBindableList<int> playingUsers = new BindableList<int>();
        private readonly IBindableDictionary<int, UserPresence> onlineUsers = new BindableDictionary<int, UserPresence>();
        private readonly Dictionary<int, OnlineUserPanel> userPanels = new Dictionary<int, OnlineUserPanel>();

        private SearchContainer<OnlineUserPanel> userFlow;
        private BasicSearchTextBox searchTextBox;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        [Resolved]
        private MetadataClient metadataClient { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = padding * 2 + search_textbox_height,
                    Colour = colourProvider.Background4,
                },
                new Container<BasicSearchTextBox>
                {
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding { Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING, Vertical = padding },
                    Child = searchTextBox = new BasicSearchTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Height = search_textbox_height,
                        ReleaseFocusOnCommit = false,
                        HoldFocus = true,
                        PlaceholderText = HomeStrings.SearchPlaceholder,
                    },
                },
                userFlow = new SearchContainer<OnlineUserPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding
                    {
                        Top = padding * 3 + search_textbox_height,
                        Bottom = padding,
                        Right = padding,
                        Left = padding,
                    },
                },
            };

            searchTextBox.Current.ValueChanged += text => userFlow.SearchTerm = text.NewValue;
        }

        [Resolved]
        private UserLookupCache users { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            onlineUsers.BindTo(metadataClient.UserStates);
            onlineUsers.BindCollectionChanged(onUserUpdated, true);

            playingUsers.BindTo(spectatorClient.PlayingUsers);
            playingUsers.BindCollectionChanged(onPlayingUsersChanged, true);
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);

            searchTextBox.TakeFocus();
        }

        private void onUserUpdated(object sender, NotifyDictionaryChangedEventArgs<int, UserPresence> e) => Schedule(() =>
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    foreach (var kvp in e.NewItems)
                    {
                        int userId = kvp.Key;

                        users.GetUserAsync(userId).ContinueWith(task =>
                        {
                            APIUser user = task.GetResultSafely();

                            if (user == null)
                                return;

                            Schedule(() =>
                            {
                                // explicitly refetch the user's status.
                                // things may have changed in between the time of scheduling and the time of actual execution.
                                if (onlineUsers.TryGetValue(userId, out var updatedStatus))
                                {
                                    user.Activity.Value = updatedStatus.Activity;
                                    user.Status.Value = updatedStatus.Status;
                                }

                                userFlow.Add(userPanels[userId] = createUserPanel(user));
                            });
                        });
                    }

                    break;

                case NotifyDictionaryChangedAction.Replace:
                    Debug.Assert(e.NewItems != null);

                    foreach (var kvp in e.NewItems)
                    {
                        if (userPanels.TryGetValue(kvp.Key, out var panel))
                        {
                            panel.User.Activity.Value = kvp.Value.Activity;
                            panel.User.Status.Value = kvp.Value.Status;
                        }
                    }

                    break;

                case NotifyDictionaryChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (var kvp in e.OldItems)
                    {
                        int userId = kvp.Key;
                        if (userPanels.Remove(userId, out var userPanel))
                            userPanel.Expire();
                    }

                    break;
            }
        });

        private void onPlayingUsersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    foreach (int userId in e.NewItems)
                    {
                        if (userPanels.TryGetValue(userId, out var panel))
                            panel.CanSpectate.Value = userId != api.LocalUser.Value.Id;
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (int userId in e.OldItems)
                    {
                        if (userPanels.TryGetValue(userId, out var panel))
                            panel.CanSpectate.Value = false;
                    }

                    break;
            }
        }

        private OnlineUserPanel createUserPanel(APIUser user) =>
            new OnlineUserPanel(user).With(panel =>
            {
                panel.Anchor = Anchor.TopCentre;
                panel.Origin = Anchor.TopCentre;
                panel.CanSpectate.Value = playingUsers.Contains(user.Id);
            });

        public partial class OnlineUserPanel : CompositeDrawable, IFilterable
        {
            public readonly APIUser User;

            public BindableBool CanSpectate { get; } = new BindableBool();

            public IEnumerable<LocalisableString> FilterTerms { get; }

            [Resolved(canBeNull: true)]
            private IPerformFromScreenRunner performer { get; set; }

            public bool FilteringActive { set; get; }

            public bool MatchingFilter
            {
                set
                {
                    if (value)
                        Show();
                    else
                        Hide();
                }
            }

            public OnlineUserPanel(APIUser user)
            {
                User = user;

                FilterTerms = new LocalisableString[] { User.Username };

                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load()
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
                                // this is SHOCKING
                                Activity = { BindTarget = User.Activity },
                                Status = { BindTarget = User.Status },
                            },
                            new PurpleRoundedButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Text = "Spectate",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Action = () => performer?.PerformFromScreen(s => s.Push(new SoloSpectatorScreen(User))),
                                Enabled = { BindTarget = CanSpectate }
                            }
                        }
                    },
                };
            }
        }
    }
}
