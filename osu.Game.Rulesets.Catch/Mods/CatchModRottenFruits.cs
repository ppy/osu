// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Audio;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModRottenFruits : Mod, IApplicableToDrawableHitObject, IApplicableToDrawableRuleset<CatchHitObject>, IUpdatableByPlayfield
    {
        public override string Name => "Rotten Fruits";
        public override LocalisableString Description => "The fruit has gone bad... dodge it!";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "RF";
        public override Type[] IncompatibleMods => new[] { typeof(CatchModAutoplay) };
        public override ModType Type => ModType.Conversion;

        [SettingSource("Mute hit sounds", "Hit sounds become muted.")]
        public BindableBool AffectsHitSounds { get; } = new BindableBool(true);

        private Catcher catcher = null!;
        private readonly BindableNumber<double> hitSoundVolume = new BindableDouble(0);

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            catcher = ((CatchPlayfield)drawableRuleset.Playfield).Catcher;

            // Don't show caught fruits as they aren't technically being caught.
            catcher.CatchFruitOnPlate = false;

            if (AffectsHitSounds.Value)
            {
                drawableRuleset.Audio.AddAdjustment(AdjustableProperty.Volume, hitSoundVolume);
            }
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableCatchHitObject catchHitObject)
            {
                catchHitObject.CheckPosition = hitObject => !catcher.CanCatch(hitObject);
            }

            drawable.ApplyCustomUpdateState += (dho, state) =>
            {
                // Keep the existing transforms when hit.
                if (state is not ArmedState.Hit)
                    return;

                // When "hit", the DHO is faded out, so to let fruits fall after being caught we fade them back in.
                dho.FadeIn();
            };
        }
        public void Update(Playfield playfield)
        {
            // Block hyperdashing to avoid hyperdashes when two objects appear at the same time.
            catcher.SetHyperDashState(1, -1);
        }
    }
}
