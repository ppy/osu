using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Users
{
    /// <summary>
    /// An avatar which can update to a new user when needed.
    /// </summary>
    public class UpdateableAvatar : Container
    {
        private Avatar displayedAvatar;

        private User user;

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
            Add(displayedAvatar = new Avatar(user, false) { RelativeSizeAxes = Axes.Both });
        }
    }
}