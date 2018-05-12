// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
            get { return user; }
            set
            {
                if (user?.Id == value?.Id)
                    return;

                user = value;

                if (IsLoaded)
                    updateAvatar();
            }
        }

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
                Add(displayedAvatar = new DelayedLoadWrapper(
                    new Avatar(user)
                    {
                        RelativeSizeAxes = Axes.Both,
                        OnLoadComplete = d => d.FadeInFromZero(300, Easing.OutQuint),
                    })
                );
            }
        }
    }
}
