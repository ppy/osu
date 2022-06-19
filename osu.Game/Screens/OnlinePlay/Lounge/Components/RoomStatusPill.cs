// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    /// <summary>
    /// A pill that displays the room's current status.
    /// </summary>
    public class RoomStatusPill : OnlinePlayComposite
    {
        [Resolved]
        private OsuColour colours { get; set; }

        private PillContainer pill;
        private SpriteText statusText;

        public RoomStatusPill()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = pill = new PillContainer
            {
                Child = statusText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 12),
                    Colour = Color4.Black
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            EndDate.BindValueChanged(_ => updateDisplay());
            Status.BindValueChanged(_ => updateDisplay(), true);

            FinishTransforms(true);
        }

        private void updateDisplay()
        {
            RoomStatus status = getDisplayStatus();

            pill.Background.Alpha = 1;
            pill.Background.FadeColour(status.GetAppropriateColour(colours), 100);
            statusText.Text = status.Message;
        }

        private RoomStatus getDisplayStatus()
        {
            if (EndDate.Value < DateTimeOffset.Now)
                return new RoomStatusEnded();

            return Status.Value;
        }
    }
}
