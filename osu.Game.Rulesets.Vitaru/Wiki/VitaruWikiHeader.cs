using osu.Framework.Graphics.Textures;
using osu.Game.Users;
using Symcol.Rulesets.Core.Wiki;

namespace osu.Game.Rulesets.Vitaru.Wiki
{
    public class VitaruWikiHeader : WikiHeader
    {
        protected override Texture RulesetIcon => VitaruRuleset.VitaruTextures.Get("Vitaru@2x");

        protected override string RulesetName => "vitaru";

        protected override string RulesetDescription => "vitaru! is a 3rd party ruleset developed for osu!lazer. It is a \"Dodge the Beat\" style ruleset where projectiles will be flying towards you while you must avoid them.";

        protected override string RulesetUrl => $@"https://github.com/Symcol/osu/tree/symcol/osu.Game.Rulesets.Vitaru";

        protected override User Creator => new User
        {
            Username = "Shawdooow",
            Id = 7726082
        };

        protected override string DiscordInvite => $@"https://discord.gg/GqFstZF";

        protected override Texture HeaderBackground => VitaruRuleset.VitaruTextures.Get("VitaruTouhosuModeTrue2560x1440");
    }
}
