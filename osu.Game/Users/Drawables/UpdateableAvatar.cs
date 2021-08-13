// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        private readonly bool openOnClick;
        private readonly bool showUsernameTooltip;
        private readonly bool showGuestOnNull;

        /// <summary>
        /// Construct a new UpdateableAvatar.
        /// </summary>
        /// <param name="user">The initial user to display.</param>
        /// <param name="openOnClick">Whether to open the user's profile when clicked.</param>
        /// <param name="showUsernameTooltip">Whether to show the username rather than "view profile" on the tooltip.</param>
        /// <param name="showGuestOnNull">Whether to show a default guest representation on null user (as opposed to nothing).</param>
        public UpdateableAvatar(User user = null, bool openOnClick = true, bool showUsernameTooltip = false, bool showGuestOnNull = true)
        {
            this.openOnClick = openOnClick;
            this.showUsernameTooltip = showUsernameTooltip;
            this.showGuestOnNull = showGuestOnNull;

            User = user;
        }

        protected override Drawable CreateDrawable(User user)
        {
            if (user == null && !showGuestOnNull)
                return null;

            var avatar = new ClickableAvatar(user)
            {
                OpenOnClick = openOnClick,
                ShowUsernameTooltip = showUsernameTooltip,
                RelativeSizeAxes = Axes.Both,
            };

            return avatar;
        }
    }
}
