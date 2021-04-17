using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFloatingFruits : Mod, IApplicableToPlayer
    {
        public override string Name => "Floating Fruits";
        public override string Acronym => "FF";
        public override string Description => "The fruits are... floating?";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Brands.Fly;

        public void ApplyToPlayer(Player player)
        {
            player.DrawableRuleset.Anchor = Anchor.Centre;
            player.DrawableRuleset.Origin = Anchor.Centre;
            player.DrawableRuleset.Scale = new osuTK.Vector2(1, -1);
        }
    }
}
