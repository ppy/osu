// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;

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

        public new float CornerExponent
        {
            get => base.CornerExponent;
            set => base.CornerExponent = value;
        }

        public new EdgeEffectParameters EdgeEffect
        {
            get => base.EdgeEffect;
            set => base.EdgeEffect = value;
        }

        protected override double LoadDelay => 200;

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
}
