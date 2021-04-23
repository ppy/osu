using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFloatingFruits : Mod, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Floating Fruits";
        public override string Acronym => "FF";
        public override string Description => "The fruits are... floating?";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Cloud;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            drawableRuleset.Anchor = Anchor.Centre;
            drawableRuleset.Origin = Anchor.Centre;
            drawableRuleset.Scale = new osuTK.Vector2(1, -1);
        }
    }
}
