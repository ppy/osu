// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.Rooms;
using osu.Game.Screens.Footer;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsCloseButton : ShearedButton
    {
        private readonly Room room;

        private ScheduledDelegate? hideDelegate;

        public PlaylistsCloseButton(Room room)
        {
            this.room = room;

            Width = 120;
            Text = OnlinePlayStrings.FooterButtonPlaylistClose;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            DarkerColour = colours.Pink3;
            LighterColour = colours.Pink1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;

            updateState();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.StartDate):
                case nameof(Room.EndDate):
                    updateState();
                    break;
            }
        }

        private void updateState()
        {
            hideDelegate?.Cancel();
            hideDelegate = Scheduler.AddDelayed(
                () => ((IScreenFooterButton)this).DisappearToBottom(0, true),
                room.InDeletionGracePeriod
                    ? room.DeletionGracePeriodRemaining.Value.TotalMilliseconds
                    : 0
            );
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
