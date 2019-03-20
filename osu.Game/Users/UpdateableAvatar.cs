// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Users
{
    /// <summary>
    /// An avatar which can update to a new user when needed.
    /// </summary>
    public class UpdateableAvatar : Container
    {
        private Drawable displayedAvatar;

        private User user;

        /// <summary>
        /// Whether to show a default guest representation on null user (as opposed to nothing).
        /// </summary>
        public bool ShowGuestOnNull = true;

        public User User
        {
            get => user;
            set
            {
                if (user?.Id == value?.Id)
                    return;

                user = value;

                if (IsLoaded)
                    updateAvatar();
            }
        }

        /// <summary>
        /// Whether to open the user's profile when clicked.
        /// </summary>
        public readonly BindableBool OpenOnClick = new BindableBool(true);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateAvatar();
        }

        private void updateAvatar()
        {
            displayedAvatar?.FadeOut(300);
            displayedAvatar?.Expire();

            if (user != null || ShowGuestOnNull)
            {
                var avatar = new Avatar(user)
                {
                    RelativeSizeAxes = Axes.Both,
                };

                avatar.OnLoadComplete += d => d.FadeInFromZero(300, Easing.OutQuint);
                avatar.OpenOnClick.BindTo(OpenOnClick);

                Add(displayedAvatar = new DelayedLoadWrapper(avatar));
            }
        }
    }
}
