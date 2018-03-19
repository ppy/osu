using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Users;
using Symcol.Rulesets.Core.Wiki;

namespace osu.Game.Rulesets.Shape.Wiki
{
    public class ShapeWikiHeader : WikiHeader
    {
        protected override Texture RulesetIcon => ShapeRuleset.ShapeTextures.Get("icon");

        protected override string RulesetName => "shape";

        protected override string RulesetDescription => "shape! is a 3rd party ruleset developed for osu!lazer.";

        protected override string RulesetUrl => $@"https://github.com/Symcol/osu/tree/symcol/osu.Game.Rulesets.Shape";

        protected override User Creator => new User
        {
            Username = "Shawdooow",
            Id = 7726082
        };

        protected override string DiscordInvite => $@"https://discord.gg/JvS5cxA";
    }
}
