// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Match
{
    public partial class DrawableMatchRoom : DrawableRoom
    {
        public Action? OnEdit;

        public new required Bindable<PlaylistItem?> SelectedItem
        {
            get => selectedItem;
            set => selectedItem.Current = value;
        }

        public Drawable? ChangeSettingsButton { get; private set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly BindableWithCurrent<PlaylistItem?> selectedItem = new BindableWithCurrent<PlaylistItem?>();
        private readonly bool allowEdit;

        public DrawableMatchRoom(Room room, bool allowEdit = true)
            : base(room)
        {
            this.allowEdit = allowEdit;

            base.SelectedItem.BindTo(SelectedItem);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (allowEdit)
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Room.PropertyChanged += onRoomPropertyChanged;
            updateRoomHost();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.Host))
                updateRoomHost();
        }

        private void updateRoomHost()
        {
            if (ChangeSettingsButton != null)
                ChangeSettingsButton.Alpha = Room.Host?.Equals(api.LocalUser.Value) == true ? 1 : 0;
        }

        protected override UpdateableBeatmapBackgroundSprite CreateBackground() => base.CreateBackground().With(d =>
        {
            d.BackgroundLoadDelay = 0;
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
