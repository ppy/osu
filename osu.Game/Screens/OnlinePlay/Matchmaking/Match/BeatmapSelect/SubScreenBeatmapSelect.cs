// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class SubScreenBeatmapSelect : MatchmakingSubScreen
    {
        public override PanelDisplayStyle PlayersDisplayStyle => PanelDisplayStyle.Split;
        public override Drawable PlayersDisplayArea { get; }

        private readonly BeatmapSelectGrid beatmapSelectGrid;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        public SubScreenBeatmapSelect()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 200 },
                    Child = beatmapSelectGrid = new BeatmapSelectGrid
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = PlayersDisplayArea = new Container().With(d =>
                    {
                        d.RelativeSizeAxes = Axes.Both;
                    })
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.ItemAdded += onItemAdded;

            foreach (var item in client.Room!.Playlist)
                onItemAdded(item);

            beatmapSelectGrid.ItemSelected += item => client.MatchmakingToggleSelection(item.ID);

            client.MatchmakingItemSelected += onItemSelected;
            client.MatchmakingItemDeselected += onItemDeselected;
        }

        private void onItemAdded(MultiplayerPlaylistItem item) => Scheduler.Add(() =>
        {
            if (item.Expired)
                return;

            beatmapSelectGrid.AddItem(item);
        });

        private void onItemSelected(int userId, long itemId)
        {
            var user = client.Room!.Users.First(it => it.UserID == userId).User!;
            beatmapSelectGrid.SetUserSelection(user, itemId, true);
        }

        private void onItemDeselected(int userId, long itemId)
        {
            var user = client.Room!.Users.First(it => it.UserID == userId).User!;
            beatmapSelectGrid.SetUserSelection(user, itemId, false);
        }

        public void RollFinalBeatmap(long[] candidateItems, long finalItem) => beatmapSelectGrid.RollAndDisplayFinalBeatmap(candidateItems, finalItem);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.ItemAdded -= onItemAdded;
                client.MatchmakingItemSelected -= onItemSelected;
                client.MatchmakingItemDeselected -= onItemDeselected;
            }
        }
    }
}
