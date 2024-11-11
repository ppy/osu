// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Match
{
    public partial class DrawableMatchRoom : DrawableRoom
    {
        public Action? OnEdit;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly BindableWithCurrent<PlaylistItem?> current = new BindableWithCurrent<PlaylistItem?>();
        private readonly IBindable<APIUser?> host = new Bindable<APIUser?>();
        private readonly bool allowEdit;
        private Drawable? editButton;

        public DrawableMatchRoom(Room room, bool allowEdit = true)
            : base(room)
        {
            this.allowEdit = allowEdit;

            base.SelectedItem.BindTo(SelectedItem);
            host.BindTo(room.Host);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (allowEdit)
            {
                ButtonsContainer.Add(editButton = new PurpleRoundedButton
                {
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(100, 1),
                    Text = CommonStrings.ButtonsEdit,
                    Action = () => OnEdit?.Invoke()
                });
            }
        }

        public new required Bindable<PlaylistItem?> SelectedItem
        {
            get => current;
            set => current.Current = value;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (editButton != null)
                host.BindValueChanged(h => editButton.Alpha = h.NewValue?.Equals(api.LocalUser.Value) == true ? 1 : 0, true);
        }

        protected override UpdateableBeatmapBackgroundSprite CreateBackground() => base.CreateBackground().With(d =>
        {
            d.BackgroundLoadDelay = 0;
        });
    }
}
