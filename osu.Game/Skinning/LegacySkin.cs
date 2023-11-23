// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Audio;
using osu.Game.Beatmaps.Formats;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class LegacySkin : Skin
    {
        protected virtual bool AllowManiaConfigLookups => true;

        /// <summary>
        /// Whether this skin can use samples with a custom bank (custom sample set in stable terminology).
        /// Added in order to match sample lookup logic from stable (in stable, only the beatmap skin could use samples with a custom sample bank).
        /// </summary>
        protected virtual bool UseCustomSampleBanks => false;

        private readonly Dictionary<int, LegacyManiaSkinConfiguration> maniaConfigurations = new Dictionary<int, LegacyManiaSkinConfiguration>();

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public LegacySkin(SkinInfo skin, IStorageResourceProvider resources)
            : this(skin, resources, null)
        {
        }

        /// <summary>
        /// Construct a new legacy skin instance.
        /// </summary>
        /// <param name="skin">The model for this skin.</param>
        /// <param name="resources">Access to raw game resources.</param>
        /// <param name="fallbackStore">An optional fallback store which will be used for file lookups that are not serviced by realm user storage.</param>
        /// <param name="configurationFilename">The user-facing filename of the configuration file to be parsed. Can accept an .osu or skin.ini file.</param>
        protected LegacySkin(SkinInfo skin, IStorageResourceProvider? resources, IResourceStore<byte[]>? fallbackStore, string configurationFilename = @"skin.ini")
            : base(skin, resources, fallbackStore, configurationFilename)
        {
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

        [SuppressMessage("ReSharper", "RedundantAssignment")] // for `wasHit` assignments used in `finally` debug logic
        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            bool wasHit = true;

            try
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
                        return SkinUtils.As<TValue>(getCustomColour(Configuration, customColour.Lookup.ToString() ?? string.Empty));

                    case LegacyManiaSkinConfigurationLookup maniaLookup:
                        if (!AllowManiaConfigLookups)
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

                wasHit = false;
                return null;
            }
            finally
            {
                LogLookupDebug(this, lookup, wasHit ? LookupDebugType.Hit : LookupDebugType.Miss);
            }
        }

        private IBindable<TValue>? lookupForMania<TValue>(LegacyManiaSkinConfigurationLookup maniaLookup)
        {
            if (!maniaConfigurations.TryGetValue(maniaLookup.TotalColumns, out var existing))
                maniaConfigurations[maniaLookup.TotalColumns] = existing = new LegacyManiaSkinConfiguration(maniaLookup.TotalColumns);

            switch (maniaLookup.Lookup)
            {
                case LegacyManiaSkinConfigurationLookups.ColumnWidth:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnWidth[maniaLookup.ColumnIndex.Value]));

                case LegacyManiaSkinConfigurationLookups.WidthForNoteHeightScale:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.WidthForNoteHeightScale));

                case LegacyManiaSkinConfigurationLookups.ColumnSpacing:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnSpacing[maniaLookup.ColumnIndex.Value]));

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
                    Debug.Assert(maniaLookup.ColumnIndex != null);

                    if (GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value < 2.5m)
                        return SkinUtils.As<TValue>(new Bindable<float>(1));

                    if (existing.ExplosionWidth[maniaLookup.ColumnIndex.Value] != 0)
                        return SkinUtils.As<TValue>(new Bindable<float>(existing.ExplosionWidth[maniaLookup.ColumnIndex.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnWidth[maniaLookup.ColumnIndex.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                case LegacyManiaSkinConfigurationLookups.ColumnLineColour:
                    return SkinUtils.As<TValue>(getCustomColour(existing, "ColourColumnLine"));

                case LegacyManiaSkinConfigurationLookups.JudgementLineColour:
                    return SkinUtils.As<TValue>(getCustomColour(existing, "ColourJudgementLine"));

                case LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getCustomColour(existing, $"Colour{maniaLookup.ColumnIndex + 1}"));

                case LegacyManiaSkinConfigurationLookups.ColumnLightColour:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getCustomColour(existing, $"ColourLight{maniaLookup.ColumnIndex + 1}"));

                case LegacyManiaSkinConfigurationLookups.MinimumColumnWidth:
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.MinimumColumnWidth));

                case LegacyManiaSkinConfigurationLookups.NoteBodyStyle:

                    if (existing.NoteBodyStyle != null)
                        return SkinUtils.As<TValue>(new Bindable<LegacyNoteBodyStyle>(existing.NoteBodyStyle.Value));

                    if (GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value < 2.5m)
                        return SkinUtils.As<TValue>(new Bindable<LegacyNoteBodyStyle>(LegacyNoteBodyStyle.Stretch));

                    return SkinUtils.As<TValue>(new Bindable<LegacyNoteBodyStyle>(LegacyNoteBodyStyle.RepeatBottom));

                case LegacyManiaSkinConfigurationLookups.NoteImage:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{maniaLookup.ColumnIndex}"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteHeadImage:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{maniaLookup.ColumnIndex}H"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteTailImage:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{maniaLookup.ColumnIndex}T"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteBodyImage:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"NoteImage{maniaLookup.ColumnIndex}L"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteLightImage:
                    return SkinUtils.As<TValue>(getManiaImage(existing, "LightingL"));

                case LegacyManiaSkinConfigurationLookups.HoldNoteLightScale:
                    Debug.Assert(maniaLookup.ColumnIndex != null);

                    if (GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value < 2.5m)
                        return SkinUtils.As<TValue>(new Bindable<float>(1));

                    if (existing.HoldNoteLightWidth[maniaLookup.ColumnIndex.Value] != 0)
                        return SkinUtils.As<TValue>(new Bindable<float>(existing.HoldNoteLightWidth[maniaLookup.ColumnIndex.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnWidth[maniaLookup.ColumnIndex.Value] / LegacyManiaSkinConfiguration.DEFAULT_COLUMN_SIZE));

                case LegacyManiaSkinConfigurationLookups.KeyImage:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"KeyImage{maniaLookup.ColumnIndex}"));

                case LegacyManiaSkinConfigurationLookups.KeyImageDown:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(getManiaImage(existing, $"KeyImage{maniaLookup.ColumnIndex}D"));

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
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnLineWidth[maniaLookup.ColumnIndex.Value]));

                case LegacyManiaSkinConfigurationLookups.RightLineWidth:
                    Debug.Assert(maniaLookup.ColumnIndex != null);
                    return SkinUtils.As<TValue>(new Bindable<float>(existing.ColumnLineWidth[maniaLookup.ColumnIndex.Value + 1]));

                case LegacyManiaSkinConfigurationLookups.Hit0:
                case LegacyManiaSkinConfigurationLookups.Hit50:
                case LegacyManiaSkinConfigurationLookups.Hit100:
                case LegacyManiaSkinConfigurationLookups.Hit200:
                case LegacyManiaSkinConfigurationLookups.Hit300:
                case LegacyManiaSkinConfigurationLookups.Hit300g:
                    return SkinUtils.As<TValue>(getManiaImage(existing, maniaLookup.Lookup.ToString()));

                case LegacyManiaSkinConfigurationLookups.KeysUnderNotes:
                    return SkinUtils.As<TValue>(new Bindable<bool>(existing.KeysUnderNotes));

                case LegacyManiaSkinConfigurationLookups.LightFramePerSecond:
                    return SkinUtils.As<TValue>(new Bindable<int>(existing.LightFramePerSecond));
            }

            return null;
        }

        /// <summary>
        /// Retrieves the correct combo colour for a given colour index and information on the combo.
        /// </summary>
        /// <param name="source">The source to retrieve the combo colours from.</param>
        /// <param name="colourIndex">The preferred index for retrieving the combo colour with.</param>
        /// <param name="combo">Information on the combo whose using the returned colour.</param>
        protected virtual IBindable<Color4>? GetComboColour(IHasComboColours source, int colourIndex, IHasComboInformation combo)
        {
            var colour = source.ComboColours?[colourIndex % source.ComboColours.Count];
            return colour.HasValue ? new Bindable<Color4>(colour.Value) : null;
        }

        private IBindable<Color4>? getCustomColour(IHasCustomColours source, string lookup)
            => source.CustomColours.TryGetValue(lookup, out var col) ? new Bindable<Color4>(col) : null;

        private IBindable<string>? getManiaImage(LegacyManiaSkinConfiguration source, string lookup)
            => source.ImageLookups.TryGetValue(lookup, out string? image) ? new Bindable<string>(image) : null;

        private IBindable<TValue>? legacySettingLookup<TValue>(SkinConfiguration.LegacySetting legacySetting)
            where TValue : notnull
        {
            switch (legacySetting)
            {
                case SkinConfiguration.LegacySetting.Version:
                    return SkinUtils.As<TValue>(new Bindable<decimal>(Configuration.LegacyVersion ?? SkinConfiguration.LATEST_VERSION));

                default:
                    return genericLookup<SkinConfiguration.LegacySetting, TValue>(legacySetting);
            }
        }

        private IBindable<TValue>? genericLookup<TLookup, TValue>(TLookup lookup)
            where TLookup : notnull
            where TValue : notnull
        {
            try
            {
                if (Configuration.ConfigDictionary.TryGetValue(lookup.ToString() ?? string.Empty, out string? val))
                {
                    // special case for handling skins which use 1 or 0 to signify a boolean state.
                    // ..or in some cases 2 (https://github.com/ppy/osu/issues/18579).
                    if (typeof(TValue) == typeof(bool))
                    {
                        val = bool.TryParse(val, out bool boolVal)
                            ? Convert.ChangeType(boolVal, typeof(bool)).ToString()
                            : Convert.ChangeType(Convert.ToInt32(val), typeof(bool)).ToString();
                    }

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

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            if (base.GetDrawableComponent(lookup) is Drawable c)
                return c;

            switch (lookup)
            {
                case SkinComponentsContainerLookup containerLookup:
                    // Only handle global level defaults for now.
                    if (containerLookup.Ruleset != null)
                        return null;

                    switch (containerLookup.Target)
                    {
                        case SkinComponentsContainerLookup.TargetArea.MainHUDComponents:
                            return new DefaultSkinComponentsContainer(container =>
                            {
                                var score = container.OfType<LegacyScoreCounter>().FirstOrDefault();
                                var accuracy = container.OfType<GameplayAccuracyCounter>().FirstOrDefault();

                                if (score != null && accuracy != null)
                                {
                                    accuracy.Y = container.ToLocalSpace(score.ScreenSpaceDrawQuad.BottomRight).Y;
                                }

                                var songProgress = container.OfType<LegacySongProgress>().FirstOrDefault();

                                if (songProgress != null && accuracy != null)
                                {
                                    songProgress.Anchor = Anchor.TopRight;
                                    songProgress.Origin = Anchor.CentreRight;
                                    songProgress.X = -accuracy.ScreenSpaceDeltaToParentSpace(accuracy.ScreenSpaceDrawQuad.Size).X - 18;
                                    songProgress.Y = container.ToLocalSpace(accuracy.ScreenSpaceDrawQuad.TopLeft).Y + (accuracy.ScreenSpaceDeltaToParentSpace(accuracy.ScreenSpaceDrawQuad.Size).Y / 2);
                                }

                                var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();
                                var keyCounter = container.OfType<DefaultKeyCounterDisplay>().FirstOrDefault();

                                if (hitError != null)
                                {
                                    hitError.Anchor = Anchor.BottomCentre;
                                    hitError.Origin = Anchor.CentreLeft;
                                    hitError.Rotation = -90;

                                    if (keyCounter != null)
                                    {
                                        const float padding = 10;

                                        keyCounter.Anchor = Anchor.BottomRight;
                                        keyCounter.Origin = Anchor.BottomRight;
                                        keyCounter.Position = new Vector2(-padding, -(padding + hitError.Width));
                                    }
                                }
                            })
                            {
                                Children = new Drawable[]
                                {
                                    new LegacyComboCounter(),
                                    new LegacyScoreCounter(),
                                    new LegacyAccuracyCounter(),
                                    new LegacySongProgress(),
                                    new LegacyHealthDisplay(),
                                    new BarHitErrorMeter(),
                                    new DefaultKeyCounterDisplay()
                                }
                            };
                    }

                    return null;

                case GameplaySkinComponentLookup<HitResult> resultComponent:

                    // kind of wasteful that we throw this away, but should do for now.
                    if (getJudgementAnimation(resultComponent.Component) != null)
                    {
                        // TODO: this should be inside the judgement pieces.
                        Func<Drawable> createDrawable = () => getJudgementAnimation(resultComponent.Component).AsNonNull();

                        var particle = getParticleTexture(resultComponent.Component);

                        if (particle != null)
                            return new LegacyJudgementPieceNew(resultComponent.Component, createDrawable, particle);

                        return new LegacyJudgementPieceOld(resultComponent.Component, createDrawable);
                    }

                    return null;
            }

            return null;
        }

        private Texture? getParticleTexture(HitResult result)
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

        private Drawable? getJudgementAnimation(HitResult result)
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

        public override Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            switch (componentName)
            {
                case "Menu/fountain-star":
                    componentName = "star2";
                    break;
            }

            foreach (string name in getFallbackNames(componentName))
            {
                // some component names (especially user-controlled ones, like `HitX` in mania)
                // may contain `@2x` scale specifications.
                // stable happens to check for that and strip them, so do the same to match stable behaviour.
                string lookupName = name.Replace(@"@2x", string.Empty);

                float ratio = 2;
                string twoTimesFilename = $"{Path.ChangeExtension(lookupName, null)}@2x{Path.GetExtension(lookupName)}";

                var texture = Textures?.Get(twoTimesFilename, wrapModeS, wrapModeT);

                if (texture == null)
                {
                    ratio = 1;
                    texture = Textures?.Get(lookupName, wrapModeS, wrapModeT);
                }

                if (texture == null)
                    continue;

                texture.ScaleAdjust = ratio;
                return texture;
            }

            return null;
        }

        public override ISample? GetSample(ISampleInfo sampleInfo)
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
    }
}
