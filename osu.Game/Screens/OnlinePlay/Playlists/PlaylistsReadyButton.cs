// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Utils;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsReadyButton : ReadyButton
    {
        [Resolved]
        private IBindable<WorkingBeatmap> gameBeatmap { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private readonly Room room;

        public PlaylistsReadyButton(Room room)
        {
            this.room = room;
            Text = "Start";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.Green;
        }

        private bool hasRemainingAttempts = true;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;
            updateRoomUserScore();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.UserScore))
                updateRoomUserScore();
        }

        private void updateRoomUserScore()
        {
            if (room.MaxAttempts == null || room.UserScore == null)
                return;

            int remaining = room.MaxAttempts.Value - room.UserScore.PlaylistItemAttempts.Sum(a => a.Attempts);

            hasRemainingAttempts = remaining > 0;
        }

        protected override void Update()
        {
            base.Update();

            Enabled.Value = hasRemainingAttempts && enoughTimeLeft();
        }

        public override LocalisableString TooltipText
        {
            get
            {
                if (!enoughTimeLeft())
                    return "No time left!";

                if (!hasRemainingAttempts)
                    return "Attempts exhausted!";

                return base.TooltipText;
            }
        }

        private bool enoughTimeLeft()
        {
            double rate = ModUtils.CalculateRateWithMods(mods.Value);

            // We want to avoid users not being able to submit scores if they chose to not skip,
            // so track length is chosen over playable length.
            double trackLength = Math.Round(gameBeatmap.Value.Track.Length / rate);

            // Additional 30 second delay added to account for load and/or submit time.
            return room.EndDate != null && DateTimeOffset.UtcNow.AddSeconds(30).AddMilliseconds(trackLength) < room.EndDate;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
