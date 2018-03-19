using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Shape;
using Symcol.Rulesets.Core.Multiplayer.Screens;

namespace osu.Game.Rulesets.Shape.Multi
{
    public class ShapeLobbyItem : RulesetLobbyItem
    {
        public override Texture Icon => ShapeRuleset.ShapeTextures.Get("icon");

        public override string RulesetName => "Shape!";

        //public override Texture Background => ShapeRuleset.ShapeTextures.Get("VitaruTouhosuModeTrue2560x1440");

        public override RulesetLobbyScreen RulesetLobbyScreen => new ShapeLobbyScreen();
    }
}
