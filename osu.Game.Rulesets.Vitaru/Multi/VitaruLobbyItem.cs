using osu.Framework.Graphics.Textures;
using Symcol.Rulesets.Core.Multiplayer.Screens;

namespace osu.Game.Rulesets.Vitaru.Multi
{
    public class VitaruLobbyItem : RulesetLobbyItem
    {
        public override Texture Icon => VitaruRuleset.VitaruTextures.Get("Vitaru@2x");

        public override string RulesetName => "Vitaru!";

        public override Texture Background => VitaruRuleset.VitaruTextures.Get("VitaruTouhosuModeTrue2560x1440");

        public override RulesetLobbyScreen RulesetLobbyScreen => new VitaruLobbyScreen();
    }
}
