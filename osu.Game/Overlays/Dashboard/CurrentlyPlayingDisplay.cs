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
using osu.Game.Online.Spectator;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard
{
    internal partial class CurrentlyPlayingDisplay : CompositeDrawable
    {
        private const float search_textbox_height = 40;
        private const float padding = 10;

        private readonly IBindableList<int> playingUsers = new BindableList<int>();

        private SearchContainer<PlayingUserPanel> userFlow;
        private BasicSearchTextBox searchTextBox;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

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
                userFlow = new SearchContainer<PlayingUserPanel>
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

            playingUsers.BindTo(spectatorClient.PlayingUsers);
            playingUsers.BindCollectionChanged(onPlayingUsersChanged, true);
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);

            searchTextBox.TakeFocus();
        }

        private void onPlayingUsersChanged(object sender, NotifyCollectionChangedEventArgs e) => Schedule(() =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    foreach (int userId in e.NewItems)
                    {
                        users.GetUserAsync(userId).ContinueWith(task =>
                        {
                            APIUser user = task.GetResultSafely();

                            if (user == null)
                                return;

                            Schedule(() =>
                            {
                                // user may no longer be playing.
                                if (!playingUsers.Contains(user.Id))
                                    return;

                                // TODO: remove this once online state is being updated more correctly.
                                user.IsOnline = true;

                                userFlow.Add(createUserPanel(user));
                            });
                        });
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (int userId in e.OldItems)
                        userFlow.FirstOrDefault(card => card.User.Id == userId)?.Expire();
                    break;
            }
        });

        private PlayingUserPanel createUserPanel(APIUser user) =>
            new PlayingUserPanel(user).With(panel =>
            {
                panel.Anchor = Anchor.TopCentre;
                panel.Origin = Anchor.TopCentre;
            });

        public partial class PlayingUserPanel : CompositeDrawable, IFilterable
        {
            public readonly APIUser User;

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

            public PlayingUserPanel(APIUser user)
            {
                User = user;

                FilterTerms = new LocalisableString[] { User.Username };

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
                            new PurpleRoundedButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Text = "Spectate",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Action = () => performer?.PerformFromScreen(s => s.Push(new SoloSpectatorScreen(User))),
                                Enabled = { Value = User.Id != api.LocalUser.Value.Id }
                            }
                        }
                    },
                };
            }
        }
    }
}
