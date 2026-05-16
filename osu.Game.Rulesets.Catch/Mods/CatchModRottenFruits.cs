// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModRottenFruits : Mod, IApplicableToDrawableHitObject, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToBeatmap
    {
        public override string Name => "Rotten Fruits";
        public override LocalisableString Description => "The fruit has gone bad... dodge it!";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "RF";
        public override Type[] IncompatibleMods => new[] { typeof(CatchModAutoplay) };
        public override ModType Type => ModType.Conversion;

        [SettingSource("Mute hit sounds", "Hit sounds become muted.")]
        public BindableBool AffectsHitSounds { get; } = new BindableBool();

        private readonly BindableNumber<double> hitSoundVolume = new BindableDouble(0);

        private CatcherArea catcherArea = null!;
        private BeatmapDifficulty difficulty = null!;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            difficulty = beatmap.Difficulty;

            foreach (var obj in beatmap.HitObjects)
                disableHyperDashes(obj);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            catcherArea = ((CatchPlayfield)drawableRuleset.Playfield).CatcherArea;

            // Don't show caught fruits as they aren't technically being caught.
            catcherArea.Catcher.CatchFruitOnPlate = false;

            // Don't show explosions either.
            // To a point this could be maybe made to work everywhere except legacy skins.
            // However, `LegacyHitExplosion` is sort of designed to only show explosions on the catcher and that behaviour can never make any sense here.
            catcherArea.Catcher.HitExplosionContainer.Expire();

            if (AffectsHitSounds.Value)
                drawableRuleset.Audio.AddAdjustment(AdjustableProperty.Volume, hitSoundVolume);
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is DrawableCatchHitObject catchHitObject)
            {
                catchHitObject.CheckPosition = hitObject =>
                {
                    bool caught = catcherArea.Catcher.CanCatch(hitObject);

                    if (caught)
                    {
                        var scaledCircleSize = new Vector2(1.1f - (difficulty.CircleSize - 0.1f) + 0.3f);

                        // Since hyperdashes don't exist in this mode, creatively reuse hyperdash trail afterimages as miss indicators.
                        catcherArea.CatcherTrails.Add(new CatcherTrailEntry(catcherArea.Time.Current, CatcherAnimationState.Fail, catcherArea.Catcher.X, scaledCircleSize,
                            CatcherTrailAnimation.HyperDashAfterImage));
                    }

                    return !caught;
                };
            }

            drawable.ApplyCustomUpdateState += (dho, state) =>
            {
                // Off-brown tint to all objects. To fit with the "rotten fruit" vernacular.
                dho.FadeColour(new Colour4(237, 215, 163, 255));

                if (state == ArmedState.Idle)
                    return;

                const double transition_duration = 500;

                dho.FadeIn();

                if (dho is DrawableJuiceStream || dho is DrawableBananaShower)
                {
                    dho.FadeOut(transition_duration).Expire();
                    return;
                }

                dho.ScaleTo(new Vector2(1.5f), transition_duration, Easing.OutElastic)
                   .RotateTo(RNG.NextSingle(-50, 50), transition_duration, Easing.OutElastic);

                if (state == ArmedState.Miss)
                    dho.FadeColour(Colour4.Red);

                dho.Expire();
            };
        }

        private void disableHyperDashes(HitObject hitObject)
        {
            if (hitObject is PalpableCatchHitObject palpableObject)
            {
                palpableObject.HyperDashTarget = null;
                palpableObject.DistanceToHyperDash = 0;
            }

            foreach (var nested in hitObject.NestedHitObjects)
                disableHyperDashes(nested);
        }
    }
}
