using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModTaikomania : Mod, IApplicableToDrawableHitObject, IApplicableToBeatmap
    {
        public override string Name => "Taikomania";
        public override string Acronym => "TM";
        public override LocalisableString Description => @"Colour confused? Split dons and kats apart vertically.";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 0.5;

        [SettingSource("Split amount", "How far notes are split apart", 0)]
        public BindableFloat SplitAmount { get; } = new BindableFloat(40f)
        {
            Precision = 1f,
            MinValue = 10f,
            MaxValue = 40f,
        };
        
        [SettingSource("Dons on top?", "Or on the bottom?", 1)]
        public BindableBool CentreOnTop { get; } = new BindableBool(true);

        private Vector2 CentreShift;
        private Vector2 RimShift;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            CentreShift = new Vector2(0, SplitAmount.Value * (CentreOnTop.Value ? -1 : 1));
            RimShift = new Vector2(0, SplitAmount.Value * (CentreOnTop.Value ? 1 : -1));
        }

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
