using osu.Game.Users;

namespace Symcol.Rulesets.Core.Containers
{
    /// <summary>
    /// TODO: make this more generic
    /// </summary>
    public class ProfileLink : LinkText
    {
        public override string Tooltip => "View profile in browser";

        public ProfileLink(User user, bool maintainer = false)
        {
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
