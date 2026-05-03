// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;

namespace osu.Game.Overlays.Notifications
{
    public abstract partial class UserAvatarNotification : SimpleNotification
    {
        protected readonly APIUser User;

        protected DrawableAvatar Avatar { get; private set; } = null!;

        protected UserAvatarNotification(APIUser user, LocalisableString text = default)
        {
            User = user;
            Icon = default;
            Text = text;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            IconContent.Masking = true;
            IconContent.CornerRadius = CORNER_RADIUS;
            IconContent.ChangeChildDepth(IconDrawable, float.MinValue);

            LoadComponentAsync(Avatar = new DrawableAvatar(User)
            {
                FillMode = FillMode.Fill,
            }, IconContent.Add);
        }

        protected override void Update()
        {
            base.Update();
            IconContent.Width = Math.Min(78, IconContent.DrawHeight);
        }
    }
}
