// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    /// <summary>
    /// A pill that displays the room's current status.
    /// </summary>
    public partial class RoomStatusPill : OnlinePlayPill
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected override FontUsage Font => base.Font.With(weight: FontWeight.SemiBold);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            EndDate.BindValueChanged(_ => updateDisplay());
            Status.BindValueChanged(_ => updateDisplay(), true);

            FinishTransforms(true);

            TextFlow.Colour = Colour4.Black;
            Pill.Background.Alpha = 1;
        }

        private void updateDisplay()
        {
            RoomStatus status = getDisplayStatus();

            Pill.Background.FadeColour(status.GetAppropriateColour(colours), 100);
            TextFlow.Text = status.Message;
        }

        private RoomStatus getDisplayStatus()
        {
            if (EndDate.Value < DateTimeOffset.Now)
                return new RoomStatusEnded();

            return Status.Value;
        }
    }
}
