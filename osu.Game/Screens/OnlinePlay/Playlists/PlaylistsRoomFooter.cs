// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsRoomFooter : CompositeDrawable
    {
        public Action? OnStart;
        public Action? OnClose;

        private readonly Room room;
        private DangerousRoundedButton closeButton = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public PlaylistsRoomFooter(Room room)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10),
                Children = new Drawable[]
                {
                    new PlaylistsReadyButton(room)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Size = new Vector2(600, 1),
                        Action = () => OnStart?.Invoke()
                    },
                    closeButton = new DangerousRoundedButton
                    {
                        Text = "Close",
                        Action = () => OnClose?.Invoke(),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(120, 1),
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Y,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomChanged;
            updateState();
        }

        private void hideCloseButton()
        {
            closeButton.ResizeWidthTo(0, 100, Easing.OutQuint)
                       .Then().FadeOut().Expire();
        }

        private void onRoomChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.Status):
                case nameof(Room.Host):
                case nameof(Room.StartDate):
                    updateState();
                    break;
            }
        }

        private void updateState()
        {
            TimeSpan? deletionGracePeriodRemaining = room.StartDate?.AddMinutes(5) - DateTimeOffset.Now;

            if (room.Host?.Id == api.LocalUser.Value.Id)
            {
                if (deletionGracePeriodRemaining > TimeSpan.Zero && !room.HasEnded)
                {
                    closeButton.FadeIn();
                    using (BeginDelayedSequence(deletionGracePeriodRemaining.Value.TotalMilliseconds))
                        hideCloseButton();
                }
                else if (closeButton.Alpha > 0)
                    hideCloseButton();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            room.PropertyChanged -= onRoomChanged;
        }
    }
}
