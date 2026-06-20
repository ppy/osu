// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using osu.Game.Beatmaps.Formats;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class LegacySkinEncoder
    {
        private readonly LegacySkin skin;

        public LegacySkinEncoder(LegacySkin skin)
        {
            this.skin = skin;
        }

        public void Encode(TextWriter textWriter)
        {
            // https://github.com/peppy/osu-stable-reference/blob/0b8b19af621dbb282773c22b36cc0453942b98d8/osu!/Graphics/Skinning/SkinOsu.cs#L147-L192

            writeSectionHeader(textWriter, LegacyDecoder<SkinConfiguration>.Section.General);
            writeValue(textWriter, @"Name", skin.SkinInfo.PerformRead(s => s.Name));
            writeValue(textWriter, @"Author", skin.SkinInfo.PerformRead(s => s.Creator));

            // the reason why the keys are manually enumerated here rather than just iterating over `skin.Configuration.ConfigDictionary`
            // is that the skin decoder generally just dumps anything and everything that looks like a variable, from *any* section, into `ConfigDictionary`.
            // therefore, there is no way to tell what data came from which section, unless:
            // (a) the keys are manually enumerated, or
            // (b) the information about the originating section is somehow added to `ConfigDictionary`.
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"SliderBallFlip");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"CursorRotate");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"CursorExpand");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"CursorCentre");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"SliderBallFrames");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"HitCircleOverlayAboveNumber");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"HitCircleOverlayAboveNumer");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"SpinnerFrequencyModulate");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"LayeredHitSounds");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"SpinnerFadePlayfield");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"SpinnerNoBlink");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"AllowSliderBallTint");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"AnimationFramerate");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"CursorTrailRotate");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"CustomComboBurstSounds");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"ComboBurstRandom");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"SliderStyle");
            writeValue(textWriter, @"Version", skin.Configuration.IsLatestVersion ? @"latest" : skin.Configuration.LegacyVersion?.ToString(CultureInfo.InvariantCulture));

            textWriter.WriteLine();
            writeSectionHeader(textWriter, LegacyDecoder<SkinConfiguration>.Section.Colours);

            for (int i = 0; i < LegacyDecoder<SkinConfiguration>.MAX_COMBO_COLOUR_COUNT; ++i)
            {
                Color4? customColour = i < skin.Configuration.CustomComboColours.Count ? skin.Configuration.CustomComboColours[i] : null;
                writeColour(textWriter, FormattableString.Invariant($@"Combo{i + 1}"), customColour, allowTransparency: false);
            }

            foreach (string key in skin.Configuration.CustomColours.Keys)
            {
                Color4? customColour = skin.Configuration.CustomColours.GetValueOrDefault(key);
                writeColour(textWriter, key, customColour, allowTransparency: false);
            }

            textWriter.WriteLine();
            writeSectionHeader(textWriter, LegacyDecoder<SkinConfiguration>.Section.Fonts);
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"HitCirclePrefix");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"HitCircleOverlap");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"ScorePrefix");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"ComboPrefix");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"ScoreOverlap");
            writeGenericValue(textWriter, skin.Configuration.ConfigDictionary, @"ComboOverlap");

            // https://github.com/peppy/osu-stable-reference/blob/0b8b19af621dbb282773c22b36cc0453942b98d8/osu!/Graphics/Skinning/SkinFruits.cs#L41-L44
            textWriter.WriteLine();
            writeSectionHeader(textWriter, LegacyDecoder<SkinConfiguration>.Section.CatchTheBeat);
            writeGenericColour(textWriter, skin.Configuration.CustomColours, @"HyperDash");
            writeGenericColour(textWriter, skin.Configuration.CustomColours, @"HyperDashAfterImage");
            writeGenericColour(textWriter, skin.Configuration.CustomColours, @"HyperDashFruit");

            // https://github.com/peppy/osu-stable-reference/blob/0b8b19af621dbb282773c22b36cc0453942b98d8/osu!/Graphics/Skinning/SkinMania.cs#L201-L230
            foreach (var (keys, maniaConfig) in skin.ManiaConfigurations)
            {
                textWriter.WriteLine();
                writeSectionHeader(textWriter, LegacyDecoder<SkinConfiguration>.Section.Mania);
                writeValue(textWriter, @"Keys", keys.ToString(CultureInfo.InvariantCulture));
                writeValue(textWriter, @"ColumnWidth",
                    enumerableToString(maniaConfig.ColumnWidth.Select(undoPositionScaleFactor)),
                    defaultValue: enumerableToString(Enumerable.Repeat(30, keys)));
                writeValue(textWriter, @"ColumnLineWidth",
                    enumerableToString(maniaConfig.ColumnLineWidth),
                    defaultValue: enumerableToString(Enumerable.Repeat(2, keys + 1)));
                writeValue(textWriter, @"ColumnSpacing",
                    enumerableToString(maniaConfig.ColumnSpacing.Select(undoPositionScaleFactor)),
                    defaultValue: enumerableToString(Enumerable.Repeat(0, keys - 1)));
                writeValue(textWriter, @"LightingNWidth",
                    enumerableToString(maniaConfig.ExplosionWidth.Select(undoPositionScaleFactor)),
                    defaultValue: enumerableToString(Enumerable.Repeat(0, keys)));
                writeValue(textWriter, @"LightingLWidth",
                    enumerableToString(maniaConfig.HoldNoteLightWidth.Select(undoPositionScaleFactor)),
                    defaultValue: enumerableToString(Enumerable.Repeat(0, keys)));
                writeValue(textWriter, @"SpecialStyle", ((int?)maniaConfig.SpecialStyle)?.ToString());
                writeValue(textWriter, @"ColumnStart", maniaConfig.ColumnStart.ToString(CultureInfo.InvariantCulture), defaultValue: @"136");
                writeValue(textWriter, @"ColumnRight", maniaConfig.ColumnRight.ToString(CultureInfo.InvariantCulture), defaultValue: @"19");
                writeValue(textWriter, @"JudgementLine", maniaConfig.ShowJudgementLine ? @"1" : @"0");
                writeValue(textWriter, @"BarlineHeight", maniaConfig.BarLineHeight.ToString(CultureInfo.InvariantCulture), defaultValue: @"1.2");

                float hitPosition = 480 - (maniaConfig.HitPosition / LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR);
                writeValue(textWriter, @"HitPosition", hitPosition.ToString(CultureInfo.InvariantCulture), defaultValue: @"402");

                float lightPosition = 480 - (maniaConfig.LightPosition / LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR);
                writeValue(textWriter, @"LightPosition", lightPosition.ToString(CultureInfo.InvariantCulture), defaultValue: @"413");

                writeValue(textWriter, @"ComboPosition", undoPositionScaleFactor(maniaConfig.ComboPosition).ToString(CultureInfo.InvariantCulture), defaultValue: @"111");
                // NOTE: default is 325 in stable and 300 in lazer. 300 is supplied here so that all lines in the original .ini are written out as they were.
                writeValue(textWriter, @"ScorePosition", undoPositionScaleFactor(maniaConfig.ScorePosition).ToString(CultureInfo.InvariantCulture), defaultValue: @"300");
                writeValue(textWriter, @"UpsideDown", maniaConfig.UpsideDown ? @"1" : @"0", defaultValue: @"0");
                writeValue(textWriter, @"LightFramePerSecond", maniaConfig.LightFramePerSecond.ToString(CultureInfo.InvariantCulture), @"60");
                writeValue(textWriter, @"SeparateScore", maniaConfig.SeparateScore ? @"1" : @"0", defaultValue: @"1");
                writeValue(textWriter, @"KeysUnderNotes", maniaConfig.KeysUnderNotes ? @"1" : @"0", defaultValue: @"0");
                writeValue(textWriter, @"SplitStages", maniaConfig.SplitStages ? @"1" : @"0", defaultValue: @"0");
                writeValue(textWriter, @"StageSeparation", maniaConfig.StageSeparation.ToString(CultureInfo.InvariantCulture), defaultValue: @"40");
                writeValue(textWriter, @"WidthForNoteHeightScale", undoPositionScaleFactor(maniaConfig.WidthForNoteHeightScale).ToString(CultureInfo.InvariantCulture), defaultValue: @"0");
                writeValue(textWriter, @"ComboBurstStyle", ((int?)maniaConfig.ComboBurstStyle)?.ToString());

                // https://github.com/peppy/osu-stable-reference/blob/0b8b19af621dbb282773c22b36cc0453942b98d8/osu!/Graphics/Skinning/SkinMania.cs#L92-L117
                foreach (string key in maniaConfig.ImageLookups.Keys)
                    writeGenericValue(textWriter, maniaConfig.ImageLookups, key);

                // https://github.com/peppy/osu-stable-reference/blob/0b8b19af621dbb282773c22b36cc0453942b98d8/osu!/Graphics/Skinning/SkinMania.cs#L119-L132
                foreach (string key in maniaConfig.CustomColours.Keys)
                    writeGenericColour(textWriter, maniaConfig.CustomColours, key);

                // https://github.com/peppy/osu-stable-reference/blob/0b8b19af621dbb282773c22b36cc0453942b98d8/osu!/Graphics/Skinning/SkinMania.cs#L134-L142
                foreach (string key in maniaConfig.FlipSettings.Keys)
                    writeGenericValue(textWriter, maniaConfig.FlipSettings, key);

                // https://github.com/peppy/osu-stable-reference/blob/0b8b19af621dbb282773c22b36cc0453942b98d8/osu!/Graphics/Skinning/SkinMania.cs#L144-L151
                writeValue(textWriter, @"NoteBodyStyle", ((int?)maniaConfig.NoteBodyStyle)?.ToString());
                for (int i = 0; i < keys; ++i)
                    writeValue(textWriter, $@"NoteBodyStyle{i}", ((int?)maniaConfig.ColumnNoteBodyStyles[i])?.ToString());
            }
        }

        private void writeSectionHeader(TextWriter textWriter, LegacyDecoder<SkinConfiguration>.Section section)
            => textWriter.WriteLine(FormattableString.Invariant($"[{section}]"));

        private void writeValue(TextWriter textWriter, string key, string? value, string? defaultValue = null)
        {
            if (value != null && (defaultValue == null || value != defaultValue))
                textWriter.WriteLine(FormattableString.Invariant($"{key}: {value}"));
        }

        private void writeGenericValue(TextWriter textWriter, Dictionary<string, string> dictionary, string key, string? defaultValue = null)
        {
            string? value = dictionary.GetValueOrDefault(key);
            writeValue(textWriter, key, value, defaultValue);
        }

        private void writeGenericValue(TextWriter textWriter, Dictionary<string, string> dictionary, string[] keys, string? defaultValue = null)
        {
            string? value = null;
            foreach (string key in keys)
                value ??= dictionary.GetValueOrDefault(key);
            writeValue(textWriter, keys.First(), value, defaultValue);
        }

        private void writeColour(TextWriter textWriter, string key, Color4? colour, Color4? defaultColour = null, bool allowTransparency = true)
            => writeValue(textWriter, key, colourToString(colour, allowTransparency), colourToString(defaultColour, allowTransparency));

        private void writeGenericColour(TextWriter textWriter, Dictionary<string, Color4> dictionary, string key, Color4? defaultColour = null)
        {
            Color4? colour = dictionary.TryGetValue(key, out var col) ? col : null;
            writeColour(textWriter, key, colour, defaultColour);
        }

        private string? colourToString(Color4? colour, bool allowTransparency)
        {
            if (colour == null)
                return null;

            return allowTransparency
                ? FormattableString.Invariant($"{(int)(colour.Value.R * 255)},{(int)(colour.Value.G * 255)},{(int)(colour.Value.B * 255)},{(int)(colour.Value.A * 255)}")
                : FormattableString.Invariant($"{(int)(colour.Value.R * 255)},{(int)(colour.Value.G * 255)},{(int)(colour.Value.B * 255)}");
        }

        private float undoPositionScaleFactor(float f) => f / LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;

        private string enumerableToString<T>(IEnumerable<T> ts)
            => string.Join(',', ts.Select(t => t?.ToString()));
    }
}
