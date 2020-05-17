﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO;
using osu.Game.Rulesets.Objects.Legacy;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class LegacyDecoder<T> : Decoder<T>
        where T : new()
    {
        protected readonly int FormatVersion;

        protected LegacyDecoder(int version)
        {
            FormatVersion = version;
        }

        protected override void ParseStreamInto(LineBufferedReader stream, T output)
        {
            Section section = Section.None;

            string line;

            while ((line = stream.ReadLine()) != null)
            {
                if (ShouldSkipLine(line))
                    continue;

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    if (!Enum.TryParse(line[1..^1], out section))
                    {
                        Logger.Log($"Unknown section \"{line}\" in \"{output}\"");
                        section = Section.None;
                    }

                    OnBeginNewSection(section);
                    continue;
                }

                try
                {
                    ParseLine(output, section, line);
                }
                catch (Exception e)
                {
                    Logger.Log($"Failed to process line \"{line}\" into \"{output}\": {e.Message}", LoggingTarget.Runtime, LogLevel.Important);
                }
            }
        }

        protected virtual bool ShouldSkipLine(string line) => string.IsNullOrWhiteSpace(line) || line.AsSpan().TrimStart().StartsWith("//".AsSpan(), StringComparison.Ordinal);

        /// <summary>
        /// Invoked when a new <see cref="Section"/> has been entered.
        /// </summary>
        /// <param name="section">The entered <see cref="Section"/>.</param>
        protected virtual void OnBeginNewSection(Section section)
        {
        }

        protected virtual void ParseLine(T output, Section section, string line)
        {
            line = StripComments(line);

            switch (section)
            {
                case Section.Colours:
                    HandleColours(output, line);
                    return;
            }
        }

        protected string StripComments(string line)
        {
            var index = line.AsSpan().IndexOf("//".AsSpan());
            if (index > 0)
                return line.Substring(0, index);

            return line;
        }

        protected void HandleColours<TModel>(TModel output, string line)
        {
            var pair = SplitKeyVal(line);

            bool isCombo = pair.Key.StartsWith(@"Combo");

            string[] split = pair.Value.Split(',');

            if (split.Length != 3 && split.Length != 4)
                throw new InvalidOperationException($@"Color specified in incorrect format (should be R,G,B or R,G,B,A): {pair.Value}");

            Color4 colour;

            try
            {
                colour = new Color4(byte.Parse(split[0]), byte.Parse(split[1]), byte.Parse(split[2]), split.Length == 4 ? byte.Parse(split[3]) : (byte)255);
            }
            catch
            {
                throw new InvalidOperationException(@"Color must be specified with 8-bit integer components");
            }

            if (isCombo)
            {
                if (!(output is IHasComboColours tHasComboColours)) return;

                tHasComboColours.AddComboColours(colour);
            }
            else
            {
                if (!(output is IHasCustomColours tHasCustomColours)) return;

                tHasCustomColours.CustomColours[pair.Key] = colour;
            }
        }

        protected KeyValuePair<string, string> SplitKeyVal(string line, char separator = ':')
        {
            var split = line.Split(separator, 2);

            return new KeyValuePair<string, string>
            (
                split[0].Trim(),
                split.Length > 1 ? split[1].Trim() : string.Empty
            );
        }

        protected string CleanFilename(string path) => path.Trim('"').ToStandardisedPath();

        protected enum Section
        {
            None,
            General,
            Editor,
            Metadata,
            Difficulty,
            Events,
            TimingPoints,
            Colours,
            HitObjects,
            Variables,
            Fonts,
            CatchTheBeat,
            Mania,
        }

        internal class LegacyDifficultyControlPoint : DifficultyControlPoint
        {
            public LegacyDifficultyControlPoint()
            {
                SpeedMultiplierBindable.Precision = double.Epsilon;
            }
        }

        internal class LegacySampleControlPoint : SampleControlPoint
        {
            public int CustomSampleBank;

            public override HitSampleInfo ApplyTo(HitSampleInfo hitSampleInfo)
            {
                var baseInfo = base.ApplyTo(hitSampleInfo);

                if (baseInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacy
                    && legacy.CustomSampleBank == 0)
                {
                    legacy.CustomSampleBank = CustomSampleBank;
                }

                return baseInfo;
            }

            public override bool IsRedundant(ControlPoint existing)
                => base.IsRedundant(existing)
                   && existing is LegacySampleControlPoint existingSample
                   && CustomSampleBank == existingSample.CustomSampleBank;
        }
    }
}
