// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class RoomLocalUserInfo : OnlinePlayComposite
    {
        private readonly Room room;
        private OsuSpriteText attemptDisplay = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public RoomLocalUserInfo(Room room)
        {
            this.room = room;
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    attemptDisplay = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14)
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            UserScore.BindValueChanged(_ => updateAttempts());

            room.PropertyChanged += onRoomPropertyChanged;
            updateAttempts();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.MaxAttempts))
                updateAttempts();
        }

        private void updateAttempts()
        {
            if (room.MaxAttempts != null)
            {
                attemptDisplay.Text = $"Maximum attempts: {room.MaxAttempts:N0}";

                if (UserScore.Value != null)
                {
                    int remaining = room.MaxAttempts.Value - UserScore.Value.PlaylistItemAttempts.Sum(a => a.Attempts);
                    attemptDisplay.Text += $" ({remaining} remaining)";

                    if (remaining == 0)
                        attemptDisplay.Colour = colours.RedLight;
                }
            }
            else
            {
                attemptDisplay.Text = string.Empty;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
