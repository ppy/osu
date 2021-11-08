// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Match
{
    public class DrawableMatchRoom : DrawableRoom
    {
        public readonly IBindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();
        public Action OnEdit;

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly IBindable<APIUser> host = new Bindable<APIUser>();
        private readonly bool allowEdit;

        [CanBeNull]
        private Drawable editButton;

        private BackgroundSprite background;

        public DrawableMatchRoom(Room room, bool allowEdit = true)
            : base(room)
        {
            this.allowEdit = allowEdit;

            host.BindTo(room.Host);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (allowEdit)
            {
                ButtonsContainer.Add(editButton = new PurpleTriangleButton
                {
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(100, 1),
                    Text = "Edit",
                    Action = () => OnEdit?.Invoke()
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (editButton != null)
                host.BindValueChanged(h => editButton.Alpha = h.NewValue?.Equals(api.LocalUser.Value) == true ? 1 : 0, true);

            SelectedItem.BindValueChanged(item => background.Beatmap.Value = item.NewValue?.Beatmap.Value, true);
        }

        protected override Drawable CreateBackground() => background = new BackgroundSprite();

        private class BackgroundSprite : UpdateableBeatmapBackgroundSprite
        {
            protected override double LoadDelay => 0;
        }
    }
}
