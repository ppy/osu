// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class ParticipantsList : CompositeDrawable
    {
        public const float TILE_SIZE = 35;

        public override Axes RelativeSizeAxes
        {
            get => base.RelativeSizeAxes;
            set
            {
                base.RelativeSizeAxes = value;

                if (tiles != null)
                    tiles.RelativeSizeAxes = value;
            }
        }

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set
            {
                base.AutoSizeAxes = value;

                if (tiles != null)
                    tiles.AutoSizeAxes = value;
            }
        }

        private FillDirection direction = FillDirection.Full;

        public FillDirection Direction
        {
            get => direction;
            set
            {
                direction = value;

                if (tiles != null)
                    tiles.Direction = value;
            }
        }

        private readonly Room room;

        public ParticipantsList(Room room)
        {
            this.room = room;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;
            updateParticipants();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.RecentParticipants))
                updateParticipants();
        }

        private ScheduledDelegate? scheduledUpdate;
        private FillFlowContainer<UserTile>? tiles;

        private void updateParticipants()
        {
            scheduledUpdate?.Cancel();
            scheduledUpdate = Schedule(() =>
            {
                tiles?.FadeOut(250, Easing.Out).Expire();

                tiles = new FillFlowContainer<UserTile>
                {
                    Alpha = 0,
                    Direction = Direction,
                    AutoSizeAxes = AutoSizeAxes,
                    RelativeSizeAxes = RelativeSizeAxes,
                    Spacing = Vector2.One
                };

                for (int i = 0; i < room.RecentParticipants.Count; i++)
                    tiles.Add(new UserTile { User = room.RecentParticipants[i] });

                AddInternal(tiles);

                tiles.Delay(250).FadeIn(250, Easing.OutQuint);
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }

        private partial class UserTile : CompositeDrawable
        {
            public APIUser? User
            {
                get => avatar.User;
                set => avatar.User = value;
            }

            private readonly UpdateableAvatar avatar;

            public UserTile()
            {
                Size = new Vector2(TILE_SIZE);
                CornerRadius = 5f;
                Masking = true;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex(@"27252d"),
                    },
                    avatar = new UpdateableAvatar(showUserPanelOnHover: true) { RelativeSizeAxes = Axes.Both },
                };
            }
        }
    }
}
