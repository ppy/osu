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
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Database;
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
        private const float searchbox_height = 40f;
        private const float container_padding = 10f;

        private readonly IBindableList<int> playingUsers = new BindableList<int>();

        private SearchContainer<PlayingUserPanel> userFlow;
        private BasicSearchTextBox userFlowSearchTextBox;

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
                    Height = container_padding * 2 + searchbox_height,
                    Colour = colourProvider.Background4,
                },

                new Container<BasicSearchTextBox>
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding(container_padding),

                    Child = userFlowSearchTextBox = new BasicSearchTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Height = searchbox_height,
                        PlaceholderText = Resources.Localisation.Web.HomeStrings.SearchPlaceholder,
                    },
                },

                userFlow = new SearchContainer<PlayingUserPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding
                    {
                        Top = container_padding * 3 + searchbox_height,
                        Bottom = container_padding,
                        Right = container_padding,
                        Left = container_padding,
                    },
                    Spacing = new Vector2(10),
                },
            };

            userFlowSearchTextBox.Current.ValueChanged += onSearchBarValueChanged;
        }

        [Resolved]
        private UserLookupCache users { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playingUsers.BindTo(spectatorClient.PlayingUsers);
            playingUsers.BindCollectionChanged(onPlayingUsersChanged, true);
        }

        private void onSearchBarValueChanged(ValueChangedEvent<string> change) => userFlow.SearchTerm = userFlowSearchTextBox.Text;

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
                            var user = task.GetResultSafely();

                            if (user == null)
                                return;

                            Schedule(() =>
                            {
                                // user may no longer be playing.
                                if (!playingUsers.Contains(user.Id))
                                    return;

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

        public class PlayingUserPanel : CompositeDrawable, IFilterable
        {
            public readonly APIUser User;
            public IEnumerable<LocalisableString> FilterTerms { get; set; }

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
