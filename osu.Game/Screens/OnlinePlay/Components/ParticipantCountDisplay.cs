// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class ParticipantCountDisplay : CompositeDrawable
    {
        private const float text_size = 30;
        private const float transition_duration = 100;

        private readonly Room room;

        private OsuSpriteText slash = null!;
        private OsuSpriteText maxText = null!;
        private OsuSpriteText count = null!;

        public ParticipantCountDisplay(Room room)
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
                Direction = FillDirection.Horizontal,
                LayoutDuration = transition_duration,
                Children = new[]
                {
                    count = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: text_size)
                    },
                    slash = new OsuSpriteText
                    {
                        Text = @"/",
                        Font = OsuFont.GetFont(weight: FontWeight.Light, size: text_size)
                    },
                    maxText = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(weight: FontWeight.Light, size: text_size)
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;
            updateRoomParticipantCount();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.MaxParticipants):
                    updateRoomMaxParticipants();
                    break;

                case nameof(Room.ParticipantCount):
                    updateRoomParticipantCount();
                    break;
            }
        }

        private void updateRoomMaxParticipants()
        {
            if (room.MaxParticipants == null)
            {
                slash.FadeOut(transition_duration);
                maxText.FadeOut(transition_duration);
            }
            else
            {
                slash.FadeIn(transition_duration);
                maxText.Text = room.MaxParticipants.ToString()!;
                maxText.FadeIn(transition_duration);
            }
        }

        private void updateRoomParticipantCount()
            => count.Text = room.ParticipantCount.ToString("#,0");

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
