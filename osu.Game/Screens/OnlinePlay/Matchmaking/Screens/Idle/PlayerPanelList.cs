// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle
{
    public partial class PlayerPanelList : CompositeDrawable
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        public bool Horizontal { get; init; }

        private FillFlowContainer<PlayerPanel> panels = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = panels = new FillFlowContainer<PlayerPanel>
            {
                RelativeSizeAxes = Axes.Both,
                Spacing = new Vector2(20, 5),
                LayoutEasing = Easing.InOutQuint,
                LayoutDuration = 500
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchRoomStateChanged += onRoomStateChanged;
            client.UserJoined += onUserJoined;
            client.UserLeft += onUserLeft;

            if (client.Room != null)
            {
                onRoomStateChanged(client.Room.MatchState);
                foreach (var user in client.Room.Users)
                    onUserJoined(user);
            }
        }

        private void onUserJoined(MultiplayerRoomUser user) => Scheduler.Add(() =>
        {
            panels.Add(new PlayerPanel(user)
            {
                Horizontal = Horizontal,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        });

        private void onUserLeft(MultiplayerRoomUser user) => Scheduler.Add(() =>
        {
            panels.Single(p => p.RoomUser.Equals(user)).Expire();
        });

        private void onRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
        {
            if (state is not MatchmakingRoomState matchmakingState)
                return;

            foreach (var panel in panels)
            {
                if (matchmakingState.Users.UserDictionary.TryGetValue(panel.User.Id, out MatchmakingUser? user))
                    panels.SetLayoutPosition(panel, user.Placement);
                else
                    panels.SetLayoutPosition(panel, float.MaxValue);
            }
        });
    }
}
