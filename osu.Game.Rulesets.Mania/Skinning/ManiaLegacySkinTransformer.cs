// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class ManiaLegacySkinTransformer : ISkin
    {
        private readonly ISkin source;
        private readonly ManiaBeatmap beatmap;

        private Lazy<bool> isLegacySkin;

        /// <summary>
        /// Whether texture for the keys exists.
        /// Used to determine if the mania ruleset is skinned.
        /// </summary>
        private Lazy<bool> hasKeyTexture;

        public ManiaLegacySkinTransformer(ISkinSource source, IBeatmap beatmap)
        {
            this.source = source;
            this.beatmap = (ManiaBeatmap)beatmap;

            source.SourceChanged += sourceChanged;
            sourceChanged();
        }

        private void sourceChanged()
        {
            isLegacySkin = new Lazy<bool>(() => source.GetConfig<LegacySkinConfiguration.LegacySetting, decimal>(LegacySkinConfiguration.LegacySetting.Version) != null);
            hasKeyTexture = new Lazy<bool>(() => source.GetAnimation(
                GetConfig<ManiaSkinConfigurationLookup, string>(
                    new ManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups.KeyImage, 0))?.Value
                ?? "mania-key1", true, true) != null);
        }

        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case GameplaySkinComponent<HitResult> resultComponent:
                    return getResult(resultComponent);

                case ManiaSkinComponent maniaComponent:
                    if (!isLegacySkin.Value || !hasKeyTexture.Value)
                        return null;

                    switch (maniaComponent.Component)
                    {
                        case ManiaSkinComponents.ColumnBackground:
                            return new LegacyColumnBackground(maniaComponent.TargetColumn == beatmap.TotalColumns - 1);

                        case ManiaSkinComponents.HitTarget:
                            return new LegacyHitTarget();

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
                    }

                    break;
            }

            return null;
        }

        private Drawable getResult(GameplaySkinComponent<HitResult> resultComponent)
        {
            switch (resultComponent.Component)
            {
                case HitResult.Miss:
                    return this.GetAnimation("mania-hit0", true, true);

                case HitResult.Meh:
                    return this.GetAnimation("mania-hit50", true, true);

                case HitResult.Ok:
                    return this.GetAnimation("mania-hit100", true, true);

                case HitResult.Good:
                    return this.GetAnimation("mania-hit200", true, true);

                case HitResult.Great:
                    return this.GetAnimation("mania-hit300", true, true);

                case HitResult.Perfect:
                    return this.GetAnimation("mania-hit300g", true, true);
            }

            return null;
        }

        public Texture GetTexture(string componentName) => source.GetTexture(componentName);

        public SampleChannel GetSample(ISampleInfo sample) => source.GetSample(sample);

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            if (lookup is ManiaSkinConfigurationLookup maniaLookup)
                return source.GetConfig<LegacyManiaSkinConfigurationLookup, TValue>(new LegacyManiaSkinConfigurationLookup(beatmap.TotalColumns, maniaLookup.Lookup, maniaLookup.TargetColumn));

            return source.GetConfig<TLookup, TValue>(lookup);
        }
    }
}
