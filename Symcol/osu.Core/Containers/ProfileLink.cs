using osu.Core.Containers.Text;
using osu.Game.Users;

namespace osu.Core.Containers
{
    /// <summary>
    /// TODO: make this more generic
    /// </summary>
    public class ProfileLink : LinkOsuSpriteText
    {
        public ProfileLink(User user, bool maintainer = false)
        {
            Tooltip = "View profile in browser";

            if (!maintainer)
                Text = "Ruleset Creator: " + user.Username;
            else
                Text = "Ruleset Maintainer: " + user.Username;

            Url = $@"https://osu.ppy.sh/users/{user.Id}";
            Font = @"Exo2.0-RegularItalic";
            TextSize = 20;
        }
    }
}
