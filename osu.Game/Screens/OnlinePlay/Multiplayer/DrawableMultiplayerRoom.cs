// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class DrawableMultiplayerRoom : DrawableRoom
    {
        public Action? OnEdit;

        public Drawable ChangeSettingsButton { get; private set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        public DrawableMultiplayerRoom(Room room)
            : base(room)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ButtonsContainer.Add(ChangeSettingsButton = new PurpleRoundedButton
            {
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(120, 0.7f),
                Text = "Change settings",
                Action = () => OnEdit?.Invoke()
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;

            onRoomUpdated();
        }

        private void onRoomUpdated()
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            ChangeSettingsButton.Alpha = client.Room.Host?.Equals(client.LocalUser) == true ? 1 : 0;
            SelectedItem.Value = new PlaylistItem(client.Room.CurrentPlaylistItem);
        }

        protected override UpdateableBeatmapBackgroundSprite CreateBackground() => base.CreateBackground().With(d =>
        {
            d.BackgroundLoadDelay = 0;
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;
        }
    }
}
