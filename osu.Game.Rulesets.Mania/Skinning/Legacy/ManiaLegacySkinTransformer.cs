// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public class ManiaLegacySkinTransformer : LegacySkinTransformer
    {
        public override bool IsProvidingLegacyResources => base.IsProvidingLegacyResources || hasKeyTexture.Value;

        /// <summary>
        /// Mapping of <see cref="HitResult"/> to their corresponding
        /// <see cref="LegacyManiaSkinConfigurationLookups"/> value.
        /// </summary>
        private static readonly IReadOnlyDictionary<HitResult, LegacyManiaSkinConfigurationLookups> hit_result_mapping
            = new Dictionary<HitResult, LegacyManiaSkinConfigurationLookups>
            {
                { HitResult.Perfect, LegacyManiaSkinConfigurationLookups.Hit300g },
                { HitResult.Great, LegacyManiaSkinConfigurationLookups.Hit300 },
                { HitResult.Good, LegacyManiaSkinConfigurationLookups.Hit200 },
                { HitResult.Ok, LegacyManiaSkinConfigurationLookups.Hit100 },
                { HitResult.Meh, LegacyManiaSkinConfigurationLookups.Hit50 },
                { HitResult.Miss, LegacyManiaSkinConfigurationLookups.Hit0 }
            };

        /// <summary>
        /// Mapping of <see cref="HitResult"/> to their corresponding
        /// default filenames.
        /// </summary>
        private static readonly IReadOnlyDictionary<HitResult, string> default_hit_result_skin_filenames
            = new Dictionary<HitResult, string>
            {
                { HitResult.Perfect, "mania-hit300g" },
                { HitResult.Great, "mania-hit300" },
                { HitResult.Good, "mania-hit200" },
                { HitResult.Ok, "mania-hit100" },
                { HitResult.Meh, "mania-hit50" },
                { HitResult.Miss, "mania-hit0" }
            };

        private readonly Lazy<bool> isLegacySkin;

        /// <summary>
        /// Whether texture for the keys exists.
        /// Used to determine if the mania ruleset is skinned.
        /// </summary>
        private readonly Lazy<bool> hasKeyTexture;

        private readonly ManiaBeatmap beatmap;

        public ManiaLegacySkinTransformer(ISkin skin, IBeatmap beatmap)
            : base(skin)
        {
            this.beatmap = (ManiaBeatmap)beatmap;

            isLegacySkin = new Lazy<bool>(() => GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version) != null);
            hasKeyTexture = new Lazy<bool>(() =>
            {
                string keyImage = this.GetManiaSkinConfig<string>(LegacyManiaSkinConfigurationLookups.KeyImage, 0)?.Value ?? "mania-key1";
                return this.GetAnimation(keyImage, true, true) != null;
            });
        }

        public override Drawable GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GameplaySkinComponentLookup<HitResult> resultComponent:
                    return getResult(resultComponent.Component);

                case ManiaSkinComponentLookup maniaComponent:
                    if (!isLegacySkin.Value || !hasKeyTexture.Value)
                        return null;

                    switch (maniaComponent.Component)
                    {
                        case ManiaSkinComponents.ColumnBackground:
                            return new LegacyColumnBackground();

                        case ManiaSkinComponents.HitTarget:
                            // Legacy skins sandwich the hit target between the column background and the column light.
                            // To preserve this ordering, it's created manually inside LegacyStageBackground.
                            return Drawable.Empty();

                        case ManiaSkinComponents.KeyArea:
                            return new LegacyKeyArea();

                        case ManiaSkinComponents.Note:
                            return new LegacyNotePiece();

                        case ManiaSkinComponents.HoldNoteHead:
                            return new LegacyHoldNoteHeadPiece();

                        case ManiaSkinComponents.HoldNoteTail:
                            return new LegacyHoldNoteTailPiece();

                        case ManiaSkinComponents.HoldNoteBody:
                            return new LegacyBodyPiece();

                        case ManiaSkinComponents.HitExplosion:
                            return new LegacyHitExplosion();

                        case ManiaSkinComponents.StageBackground:
                            return new LegacyStageBackground();

                        case ManiaSkinComponents.StageForeground:
                            return new LegacyStageForeground();

                        case ManiaSkinComponents.BarLine:
                            return null; // Not yet implemented.

                        default:
                            throw new UnsupportedSkinComponentException(lookup);
                    }
            }

            return base.GetDrawableComponent(lookup);
        }

        private Drawable getResult(HitResult result)
        {
            if (!hit_result_mapping.ContainsKey(result))
                return null;

            string filename = this.GetManiaSkinConfig<string>(hit_result_mapping[result])?.Value
                              ?? default_hit_result_skin_filenames[result];

            var animation = this.GetAnimation(filename, true, true);
            return animation == null ? null : new LegacyManiaJudgementPiece(result, animation);
        }

        public override ISample GetSample(ISampleInfo sampleInfo)
        {
            // layered hit sounds never play in mania
            if (sampleInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacySample && legacySample.IsLayered)
                return new SampleVirtual();

            return base.GetSample(sampleInfo);
        }

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            if (lookup is ManiaSkinConfigurationLookup maniaLookup)
            {
                return base.GetConfig<LegacyManiaSkinConfigurationLookup, TValue>(new LegacyManiaSkinConfigurationLookup(beatmap.TotalColumns, maniaLookup.Lookup, maniaLookup.ColumnIndex));
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }
    }
}
