// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Audio;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class LegacySkin : Skin
    {
        [CanBeNull]
        protected TextureStore Textures;

        [CanBeNull]
        protected ISampleStore Samples;

        /// <summary>
        /// Whether texture for the keys exists.
        /// Used to determine if the mania ruleset is skinned.
        /// </summary>
        private readonly Lazy<bool> hasKeyTexture;

        protected virtual bool AllowManiaSkin => hasKeyTexture.Value;

        /// <summary>
        /// Whether this skin can use samples with a custom bank (custom sample set in stable terminology).
        /// Added in order to match sample lookup logic from stable (in stable, only the beatmap skin could use samples with a custom sample bank).
        /// </summary>
        protected virtual bool UseCustomSampleBanks => false;

        private readonly Dictionary<int, LegacyManiaSkinConfiguration> maniaConfigurations = new Dictionary<int, LegacyManiaSkinConfiguration>();

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public LegacySkin(SkinInfo skin, IStorageResourceProvider resources)
            : this(skin, new LegacyDatabasedSkinResourceStore(skin, resources.Files), resources, "skin.ini")
        {
        }

        /// <summary>
        /// Construct a new legacy skin instance.
        /// </summary>
        /// <param name="skin">The model for this skin.</param>
        /// <param name="storage">A storage for looking up files within this skin using user-facing filenames.</param>
        /// <param name="resources">Access to raw game resources.</param>
        /// <param name="configurationFilename">The user-facing filename of the configuration file to be parsed. Can accept an .osu or skin.ini file.</param>
        protected LegacySkin(SkinInfo skin, [CanBeNull] IResourceStore<byte[]> storage, [CanBeNull] IStorageResourceProvider resources, string configurationFilename)
            : this(skin, storage, resources, storage?.GetStream(configurationFilename))
        {
        }

        /// <summary>
        /// Construct a new legacy skin instance.
        /// </summary>
        /// <param name="skin">The model for this skin.</param>
        /// <param name="storage">A storage for looking up files within this skin using user-facing filenames.</param>
        /// <param name="resources">Access to raw game resources.</param>
        /// <param name="configurationStream">An optional stream containing the contents of a skin.ini file.</param>
        protected LegacySkin(SkinInfo skin, [CanBeNull] IResourceStore<byte[]> storage, [CanBeNull] IStorageResourceProvider resources, [CanBeNull] Stream configurationStream)
            : base(skin, resources, configurationStream)
        {
            if (storage != null)
            {
                var samples = resources?.AudioManager?.GetSampleStore(storage);
                if (samples != null)
                    samples.PlaybackConcurrency = OsuGameBase.SAMPLE_CONCURRENCY;

                Samples = samples;
                Textures = new TextureStore(resources?.CreateTextureLoaderStore(storage));

                (storage as ResourceStore<byte[]>)?.AddExtension("ogg");
            }

            // todo: this shouldn't really be duplicated here (from ManiaLegacySkinTransformer). we need to come up with a better solution.
            hasKeyTexture = new Lazy<bool>(() => this.GetAnimation(
                lookupForMania<string>(new LegacyManiaSkinConfigurationLookup(4, LegacyManiaSkinConfigurationLookups.KeyImage, 0))?.Value ?? "mania-key1", true,
                true) != null);
        }

        protected override void ParseConfigurationStream(Stream stream)
        {
            base.ParseConfigurationStream(stream);

            stream.Seek(0, SeekOrigin.Begin);

            using (LineBufferedReader reader = new LineBufferedReader(stream))
            {
                var maniaList = new LegacyManiaSkinDecoder().Decode(reader);

                foreach (var config in maniaList)
                    maniaConfigurations[config.Keys] = config;
            }
        }

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                case GlobalSkinColours colour:
                    switch (colour)
                    {
                        case GlobalSkinColours.ComboColours:
                            var comboColours = Configuration.ComboColours;
                            if (comboColours != null)
                                return SkinUtils.As<TValue>(new Bindable<IReadOnlyList<Color4>>(comboColours));

                            break;

                        default:
                            return SkinUtils.As<TValue>(getCustomColour(Configuration, colour.ToString()));
                    }

                    break;

                case SkinComboColourLookup comboColour:
                    return SkinUtils.As<TValue>(GetComboColour(Configuration, comboColour.ColourIndex, comboColour.Combo));

                case SkinCustomColourLookup customColour:
                    return SkinUtils.As<TValue>(getCustomColour(Configuration, customColour.Lookup.ToString()));

                case LegacyManiaSkinConfigurationLookup maniaLookup:
                    if (!AllowManiaSkin)
                        break;

                    var result = lookupForMania<TValue>(maniaLookup);
                    if (result != null)
                        return result;

                    break;

                case SkinConfiguration.LegacySetting legacy:
                    return legacySettingLookup<TValue>(legacy);

                default:
                    return genericLookup<TLookup, TValue>(lookup);
            }

            return null;
        }

        private IBindable<TValue> lookupForMania<TValue>(LegacyManiaSkinConfigurationLookup maniaLookup)
        {
            if (!maniaConfigurations.TryGetValue(maniaLookup.Keys, out var existing))
                maniaConfigurations[maniaLookup.Keys] = existing = new LegacyManiaSkinConfiguration(maniaLookup.Keys);

            switch (maniaLookup.Lookup)
            {
                case LegacyManiaSkinConfigurationLookups.ColumnWidth:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnWidth[maniaLookup.TargetColumn.Value]));

                case LegacyManiaSkinConfigurationLookups.ColumnSpacing:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnSpacing[maniaLookup.TargetColumn.Value]));

                case LegacyManiaSkinConfigurationLookups.HitPosition:
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.HitPosition));

                case LegacyManiaSkinConfigurationLookups.ScorePosition:
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ScorePosition));

                case LegacyManiaSkinConfigurationLookups.LightPosition:
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.LightPosition));

                case LegacyManiaSkinConfigurationLookups.ShowJudgementLine:
                    return SkinUtils.As<TValue>(new Bindable<bool>(existing.ShowJudgementLine));

                case LegacyManiaSkinConfigurationLookups.ExplosionImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "LightingN"));

                case LegacyManiaSkinConfigurationLookups.ExplosionScale:
                    Debug.Assert(maniaLookup.TargetColumn != null);

                    if (GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value < 2.5m)
                        return SkinUtils.As<TValue>(new Bindable<float>(1));

                    if (existing.ExplosionWidth[maniaLookup.TargetColumn.Value] != 0)
                        return SkinUtils.As<TValue>(new Bindable<float>(existing.ExplosionWidth[maniaLookup.TargetColumn.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnWidth[maniaLookup.TargetColumn.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                case LegacyManiaSkinConfigurationLookups.ColumnLineColour:
                    return SkinUtils.As<TValue>(getCustomColour(existing, "ColourColumnLine"));

                case LegacyManiaSkinConfigurationLookups.JudgementLineColour:
                    return SkinUtils.As<TValue>(getCustomColour(existing, "ColourJudgementLine"));

                case LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(getCustomColour(existing, $"Colour{maniaLookup.TargetColumn + 1}"));

                case LegacyManiaSkinConfigurationLookups.ColumnLightColour:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(getCustomColour(existing, $"ColourLight{maniaLookup.TargetColumn + 1}"));

                case LegacyManiaSkinConfigurationLookups.MinimumColumnWidth:
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.MinimumColumnWidth));

                case LegacyManiaSkinConfigurationLookups.NoteImage:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{maniaLookup.TargetColumn}"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteHeadImage:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{maniaLookup.TargetColumn}H"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteTailImage:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{maniaLookup.TargetColumn}T"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteBodyImage:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{maniaLookup.TargetColumn}L"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteLightImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "LightingL"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteLightScale:
                    Debug.Assert(maniaLookup.TargetColumn != null);

                    if (GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value < 2.5m)
                        return SkinUtils.As<TValue>(new Bindable<float>(1));

                    if (existing.HoldNoteLightWidth[maniaLookup.TargetColumn.Value] != 0)
                        return SkinUtils.As<TValue>(new Bindable<float>(existing.HoldNoteLightWidth[maniaLookup.TargetColumn.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnWidth[maniaLookup.TargetColumn.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                case LegacyManiaSkinConfigurationLookups.KeyImage:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"KeyImage{maniaLookup.TargetColumn}"));

                case LegacyManiaSkinConfigurationLookups.KeyImageDown:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"KeyImage{maniaLookup.TargetColumn}D"));

                case LegacyManiaSkinConfigurationLookups.LeftStageImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "StageLeft"));

                case LegacyManiaSkinConfigurationLookups.RightStageImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "StageRight"));

                case LegacyManiaSkinConfigurationLookups.BottomStageImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "StageBottom"));

                case LegacyManiaSkinConfigurationLookups.LightImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "StageLight"));

                case LegacyManiaSkinConfigurationLookups.HitTargetImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "StageHint"));

                case LegacyManiaSkinConfigurationLookups.LeftLineWidth:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnLineWidth[maniaLookup.TargetColumn.Value]));

                case LegacyManiaSkinConfigurationLookups.RightLineWidth:
                    Debug.Assert(maniaLookup.TargetColumn != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnLineWidth[maniaLookup.TargetColumn.Value + 1]));

                case LegacyManiaSkinConfigurationLookups.Hit0:
                case LegacyManiaSkinConfigurationLookups.Hit50:
                case LegacyManiaSkinConfigurationLookups.Hit100:
                case LegacyManiaSkinConfigurationLookups.Hit200:
                case LegacyManiaSkinConfigurationLookups.Hit300:
                case LegacyManiaSkinConfigurationLookups.Hit300g:
                    return SkinUtils.As<TValue>(getManiaImage(existing, maniaLookup.Lookup.ToString()));

                case LegacyManiaSkinConfigurationLookups.KeysUnderNotes:
                    return SkinUtils.As<TValue>(new Bindable<bool>(existing.KeysUnderNotes));
            }

            return null;
        }

        /// <summary>
        /// Retrieves the correct combo colour for a given colour index and information on the combo.
        /// </summary>
        /// <param name="source">The source to retrieve the combo colours from.</param>
        /// <param name="colourIndex">The preferred index for retrieving the combo colour with.</param>
        /// <param name="combo">Information on the combo whose using the returned colour.</param>
        protected virtual IBindable<Color4> GetComboColour(IHasComboColours source, int colourIndex, IHasComboInformation combo)
        {
            var colour = source.ComboColours?[colourIndex % source.ComboColours.Count];
            return colour.HasValue ? new Bindable<Color4>(colour.Value) : null;
        }

        private IBindable<Color4> getCustomColour(IHasCustomColours source, string lookup)
            => source.CustomColours.TryGetValue(lookup, out var col) ? new Bindable<Color4>(col) : null;

        private IBindable<string> getManiaImage(LegacyManiaSkinConfiguration source, string lookup)
            => source.ImageLookups.TryGetValue(lookup, out string image) ? new Bindable<string>(image) : null;

        [CanBeNull]
        private IBindable<TValue> legacySettingLookup<TValue>(SkinConfiguration.LegacySetting legacySetting)
        {
            switch (legacySetting)
            {
                case SkinConfiguration.LegacySetting.Version:
                    return SkinUtils.As<TValue>(new Bindable<decimal>(Configuration.LegacyVersion ?? SkinConfiguration.LATEST_VERSION));

                default:
                    return genericLookup<SkinConfiguration.LegacySetting, TValue>(legacySetting);
            }
        }

        [CanBeNull]
        private IBindable<TValue> genericLookup<TLookup, TValue>(TLookup lookup)
        {
            try
            {
                if (Configuration.ConfigDictionary.TryGetValue(lookup.ToString(), out string val))
                {
                    // special case for handling skins which use 1 or 0 to signify a boolean state.
                    if (typeof(TValue) == typeof(bool))
                        val = val == "1" ? "true" : "false";

                    var bindable = new Bindable<TValue>();
                    if (val != null)
                        bindable.Parse(val);
                    return bindable;
                }
            }
            catch
            {
            }

            return null;
        }

        public override Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (base.GetDrawableComponent(component) is Drawable c)
                return c;

            switch (component)
            {
                case SkinnableTargetComponent target:
                    switch (target.Target)
                    {
                        case SkinnableTarget.MainHUDComponents:
                            var skinnableTargetWrapper = new SkinnableTargetComponentsContainer(container =>
                            {
                                var score = container.OfType<LegacyScoreCounter>().FirstOrDefault();
                                var accuracy = container.OfType<GameplayAccuracyCounter>().FirstOrDefault();
                                var combo = container.OfType<LegacyComboCounter>().FirstOrDefault();

                                if (score != null && accuracy != null)
                                {
                                    accuracy.Y = container.ToLocalSpace(score.ScreenSpaceDrawQuad.BottomRight).Y;
                                }

                                var songProgress = container.OfType<SongProgress>().FirstOrDefault();

                                var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();

                                if (hitError != null)
                                {
                                    hitError.Anchor = Anchor.BottomCentre;
                                    hitError.Origin = Anchor.CentreLeft;
                                    hitError.Rotation = -90;
                                }

                                if (songProgress != null)
                                {
                                    if (hitError != null) hitError.Y -= SongProgress.MAX_HEIGHT;
                                    if (combo != null) combo.Y -= SongProgress.MAX_HEIGHT;
                                }
                            })
                            {
                                Children = this.HasFont(LegacyFont.Score)
                                    ? new Drawable[]
                                    {
                                        new LegacyComboCounter(),
                                        new LegacyScoreCounter(),
                                        new LegacyAccuracyCounter(),
                                        new LegacyHealthDisplay(),
                                        new SongProgress(),
                                        new BarHitErrorMeter(),
                                    }
                                    : new Drawable[]
                                    {
                                        // TODO: these should fallback to using osu!classic skin textures, rather than doing this.
                                        new DefaultComboCounter(),
                                        new DefaultScoreCounter(),
                                        new DefaultAccuracyCounter(),
                                        new DefaultHealthDisplay(),
                                        new SongProgress(),
                                        new BarHitErrorMeter(),
                                    }
                            };

                            return skinnableTargetWrapper;
                    }

                    return null;

                case GameplaySkinComponent<HitResult> resultComponent:
                    // TODO: this should be inside the judgement pieces.
                    Func<Drawable> createDrawable = () => getJudgementAnimation(resultComponent.Component);

                    // kind of wasteful that we throw this away, but should do for now.
                    if (createDrawable() != null)
                    {
                        var particle = getParticleTexture(resultComponent.Component);

                        if (particle != null)
                            return new LegacyJudgementPieceNew(resultComponent.Component, createDrawable, particle);
                        else
                            return new LegacyJudgementPieceOld(resultComponent.Component, createDrawable);
                    }

                    break;
            }

            return this.GetAnimation(component.LookupName, false, false);
        }

        private Texture getParticleTexture(HitResult result)
        {
            switch (result)
            {
                case HitResult.Meh:
                    return GetTexture("particle50");

                case HitResult.Ok:
                    return GetTexture("particle100");

                case HitResult.Great:
                    return GetTexture("particle300");
            }

            return null;
        }

        private Drawable getJudgementAnimation(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return this.GetAnimation("hit0", true, false);

                case HitResult.Meh:
                    return this.GetAnimation("hit50", true, false);

                case HitResult.Ok:
                    return this.GetAnimation("hit100", true, false);

                case HitResult.Great:
                    return this.GetAnimation("hit300", true, false);
            }

            return null;
        }

        public override Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            foreach (string name in getFallbackNames(componentName))
            {
                float ratio = 2;
                var texture = Textures?.Get($"{name}@2x", wrapModeS, wrapModeT);

                if (texture == null)
                {
                    ratio = 1;
                    texture = Textures?.Get(name, wrapModeS, wrapModeT);
                }

                if (texture == null)
                    continue;

                texture.ScaleAdjust = ratio;
                return texture;
            }

            return null;
        }

        public override ISample GetSample(ISampleInfo sampleInfo)
        {
            IEnumerable<string> lookupNames;

            if (sampleInfo is HitSampleInfo hitSample)
                lookupNames = getLegacyLookupNames(hitSample);
            else
            {
                lookupNames = sampleInfo.LookupNames.SelectMany(getFallbackNames);
            }

            foreach (string lookup in lookupNames)
            {
                var sample = Samples?.Get(lookup);

                if (sample != null)
                {
                    return sample;
                }
            }

            return null;
        }

        private IEnumerable<string> getLegacyLookupNames(HitSampleInfo hitSample)
        {
            var lookupNames = hitSample.LookupNames.SelectMany(getFallbackNames);

            if (!UseCustomSampleBanks && !string.IsNullOrEmpty(hitSample.Suffix))
            {
                // for compatibility with stable, exclude the lookup names with the custom sample bank suffix, if they are not valid for use in this skin.
                // using .EndsWith() is intentional as it ensures parity in all edge cases
                // (see LegacyTaikoSampleInfo for an example of one - prioritising the taiko prefix should still apply, but the sample bank should not).
                lookupNames = lookupNames.Where(name => !name.EndsWith(hitSample.Suffix, StringComparison.Ordinal));
            }

            foreach (string l in lookupNames)
                yield return l;

            // also for compatibility, try falling back to non-bank samples (so-called "universal" samples) as the last resort.
            // going forward specifying banks shall always be required, even for elements that wouldn't require it on stable,
            // which is why this is done locally here.
            yield return hitSample.Name;
        }

        private IEnumerable<string> getFallbackNames(string componentName)
        {
            // May be something like "Gameplay/osu/approachcircle" from lazer, or "Arrows/note1" from a user skin.
            yield return componentName;

            // Fall back to using the last piece for components coming from lazer (e.g. "Gameplay/osu/approachcircle" -> "approachcircle").
            yield return componentName.Split('/').Last();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Textures?.Dispose();
            Samples?.Dispose();
        }
    }
}
