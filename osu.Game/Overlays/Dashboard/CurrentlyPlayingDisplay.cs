// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Spectator;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard
{
    internal class CurrentlyPlayingDisplay : CompositeDrawable
    {
        private IBindableList<int> playingUsers;

        private FillFlowContainer<UserPanel> userFlow;

        [Resolved]
        private SpectatorStreamingClient spectatorStreaming { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = userFlow = new FillFlowContainer<UserPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10),
            };
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        // temporary, should be game-global but i don't want to add more manager classes for now.
        private static readonly Dictionary<int, User> user_cache = new Dictionary<int, User>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playingUsers = spectatorStreaming.PlayingUsers.GetBoundCopy();
            playingUsers.BindCollectionChanged((sender, e) => Schedule(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (int userId in e.NewItems.OfType<int>())
                        {
                            if (user_cache.TryGetValue(userId, out var user))
                            {
                                addUser(user);
                                continue;
                            }

                            var request = new GetUserRequest(userId);
                            request.Success += u => Schedule(() => addUser(u));
                            api.Queue(request);

                            void addUser(User u)
                            {
                                user_cache[userId] = u;
                                if (playingUsers.Contains(userId))
                                    userFlow.Add(createUserPanel(u));
                            }
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var u in e.OldItems.OfType<int>())
                            userFlow.FirstOrDefault(card => card.User.Id == u)?.Expire();
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        userFlow.Clear();
                        break;
                }
            }), true);
        }

        [Resolved]
        private OsuGame game { get; set; }

        private UserPanel createUserPanel(User user)
        {
            return new UserGridPanel(user).With(panel =>
            {
                panel.Anchor = Anchor.TopCentre;
                panel.Origin = Anchor.TopCentre;
                panel.Width = 290;
                panel.ShowProfileOnClick = false;
                panel.Action = () => game.PerformFromScreen(s => s.Push(new Spectator(user)));
            });
        }
    }
}
