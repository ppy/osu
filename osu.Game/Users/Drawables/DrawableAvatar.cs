// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;

namespace osu.Game.Users.Drawables
{
    /// <summary>
    /// An avatar which can update to a new user when needed.
    /// </summary>
    public class UpdateableAvatar : ModelBackedDrawable<User>
    {
        public User User
        {
            get => Model;
            set => Model = value;
        }

        public new bool Masking
        {
            get => base.Masking;
            set => base.Masking = value;
        }

        public new float CornerRadius
        {
            get => base.CornerRadius;
            set => base.CornerRadius = value;
        }

        public new EdgeEffectParameters EdgeEffect
        {
            get => base.EdgeEffect;
            set => base.EdgeEffect = value;
        }

        /// <summary>
        /// Whether to show a default guest representation on null user (as opposed to nothing).
        /// </summary>
        public bool ShowGuestOnNull = true;

        /// <summary>
        /// Whether to open the user's profile when clicked.
        /// </summary>
        public readonly BindableBool OpenOnClick = new BindableBool(true);

        public UpdateableAvatar(User user = null)
        {
            User = user;
        }

        protected override Drawable CreateDrawable(User user)
        {
            if (user == null && !ShowGuestOnNull)
                return null;

            var avatar = new DrawableAvatar(user)
            {
                RelativeSizeAxes = Axes.Both,
            };

            avatar.OnLoadComplete += d => d.FadeInFromZero(300, Easing.OutQuint);
            avatar.OpenOnClick.BindTo(OpenOnClick);

            return avatar;
        }
    }

    public class DrawableAvatar : Container
    {
        /// <summary>
        /// Whether to open the user's profile when clicked.
        /// </summary>
        public readonly BindableBool OpenOnClick = new BindableBool(true);

        private readonly User user;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        /// <summary>
        /// An avatar for specified user.
        /// </summary>
        /// <param name="user">The user. A null value will get a placeholder avatar.</param>
        public DrawableAvatar(User user = null)
        {
            this.user = user;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            if (textures == null)
                throw new ArgumentNullException(nameof(textures));

            Texture texture = null;
            if (user != null && user.Id > 1) texture = textures.Get($@"https://a.ppy.sh/{user.Id}");
            if (texture == null) texture = textures.Get(@"Online/avatar-guest");

            ClickableArea clickableArea;
            Add(clickableArea = new ClickableArea
            {
                RelativeSizeAxes = Axes.Both,
                Child = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = texture,
                    FillMode = FillMode.Fit,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                Action = openProfile
            });

            clickableArea.Enabled.BindTo(OpenOnClick);
        }

        private void openProfile()
        {
            if (!OpenOnClick.Value)
                return;

            if (user != null)
                game?.ShowUser(user.Id);
        }

        private class ClickableArea : OsuClickableContainer
        {
            public override string TooltipText => Enabled.Value ? @"View Profile" : null;

            protected override bool OnClick(ClickEvent e)
            {
                if (!Enabled.Value)
                    return false;

                return base.OnClick(e);
            }
        }
    }
}
