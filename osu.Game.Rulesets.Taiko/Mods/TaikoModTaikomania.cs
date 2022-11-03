using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModTaikomania : Mod, IApplicableToDrawableHitObject
    {
        public override string Name => "Taikomania";
        public override string Acronym => "TM";
        public override LocalisableString Description => @"Color confused? Moves dons and kats apart.";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 0.5;

        private Vector2 CentreShift = new Vector2(0, -40);
        private Vector2 RimShift = new Vector2(0, 40);

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableHit)
            {
                drawable.ApplyCustomUpdateState += (o, state) =>
                {
                    if (o is DrawableHit)
                    {
                        switch (((DrawableHit)o).HitObject.Type)
                        {
                            case HitType.Centre:
                                drawable.MoveToOffset(CentreShift);
                                break;
                            case HitType.Rim:
                                drawable.MoveToOffset(RimShift);
                                break;
                        }
                    }
                };
            }
        }
    }
}
