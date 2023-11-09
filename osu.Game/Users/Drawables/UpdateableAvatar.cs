// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users.Drawables
{
    /// <summary>
    /// An avatar which can update to a new user when needed.
    /// </summary>
    public partial class UpdateableAvatar : ModelBackedDrawable<APIUser?>
    {
        public APIUser? User
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

        private readonly bool isInteractive;
        private readonly bool showGuestOnNull;
        private readonly bool showUsernameOnly;

        /// <summary>
        /// Construct a new UpdateableAvatar.
        /// </summary>
        /// <param name="user">The initial user to display.</param>
        /// <param name="isInteractive">If set to true, hover/click sounds will play and clicking the avatar will open the user's profile.</param>
        /// <param name="showUsernameOnly">If set to true, the user status panel will be displayed in the tooltip.</param>
        /// <param name="showGuestOnNull">Whether to show a default guest representation on null user (as opposed to nothing).</param>
        public UpdateableAvatar(APIUser? user = null, bool isInteractive = true, bool showUsernameOnly = false, bool showGuestOnNull = true)
        {
            this.isInteractive = isInteractive;
            this.showGuestOnNull = showGuestOnNull;
            this.showUsernameOnly = showUsernameOnly;

            User = user;
        }

        protected override Drawable? CreateDrawable(APIUser? user)
        {
            if (user == null && !showGuestOnNull)
                return null;

            if (isInteractive)
            {
                return new ClickableAvatar(user)
                {
                    RelativeSizeAxes = Axes.Both,
                };
            }
            else
            {
                return new DrawableAvatar(user)
                {
                    RelativeSizeAxes = Axes.Both,
                };
            }
        }
    }
}
