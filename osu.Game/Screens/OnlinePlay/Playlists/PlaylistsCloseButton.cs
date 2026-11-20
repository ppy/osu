// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsCloseButton : ShearedButton
    {
        private readonly Room room;

        private ScheduledDelegate? disableDelegate;

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
            disableDelegate?.Cancel();
            disableDelegate = Scheduler.AddDelayed(
                () => Enabled.Value = false,
                room.InDeletionGracePeriod
                    ? room.DeletionGracePeriodRemaining.Value.TotalMilliseconds
                    : 0
            );
        }

        public override LocalisableString TooltipText
        {
            get
            {
                if (Enabled.Value)
                    return string.Empty;

                if (room.DeletedEarly)
                    return OnlinePlayStrings.FooterButtonPlaylistCloseAlreadyClosed;

                return OnlinePlayStrings.FooterButtonPlaylistClosePastGracePeriod;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
