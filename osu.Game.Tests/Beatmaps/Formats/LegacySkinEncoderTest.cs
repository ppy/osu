// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using osu.Framework.IO.Stores;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacySkinEncoderTest
    {
        private static readonly DllResourceStore resource_store = TestResources.GetStore();

        private static IEnumerable<string> allIniFiles = resource_store.GetAvailableResources().Where(res => res.EndsWith(".ini", StringComparison.Ordinal));

        [TestCaseSource(nameof(allIniFiles))]
        public void TestEncodeDecodeStability(string name)
        {
            using var sourceStream = resource_store.GetStream(name);

            var decoded = decode(sourceStream);
            var encoded = encode(decoded);
            var decodedAfterEncode = decode(encoded);

            Assert.Multiple(() =>
            {
                assertSame(decoded, decodedAfterEncode, skin => skin.SkinInfo.PerformRead(s => s.Name));
                assertSame(decoded, decodedAfterEncode, skin => skin.SkinInfo.PerformRead(s => s.Creator));

                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"SliderBallFlip"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"CursorRotate"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"CursorExpand"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"CursorCentre"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"SliderBallFrames"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"HitCircleOverlayAboveNumber"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"HitCircleOverlayAboveNumer"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"SpinnerFrequencyModulate"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"LayeredHitSounds"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"SpinnerFadePlayfield"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"SpinnerNoBlink"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"AllowSliderBallTint"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"AnimationFramerate"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"CursorTrailRotate"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"CustomComboBurstSounds"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"ComboBurstRandom"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"SliderStyle"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.LegacyVersion);
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.IsLatestVersion);

                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.CustomComboColours);
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.CustomColours);

                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"HitCirclePrefix"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"HitCircleOverlap"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"ScorePrefix"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"ComboPrefix"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"ScoreOverlap"));
                assertSame(decoded, decodedAfterEncode, skin => skin.Configuration.ConfigDictionary.GetValueOrDefault(@"ComboOverlap"));

                foreach (var (keys, decodedMania) in decoded.ManiaConfigurations)
                {
                    var decodedAfterEncodeMania = decodedAfterEncode.ManiaConfigurations.GetValueOrDefault(keys);

                    Assert.That(decodedAfterEncodeMania, Is.Not.Null);

                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ColumnWidth);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ColumnLineWidth);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ColumnSpacing);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ExplosionWidth);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.HoldNoteLightWidth);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.SpecialStyle);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ColumnStart);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ColumnRight);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ShowJudgementLine);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.BarLineHeight);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.HitPosition);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.LightPosition);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ComboPosition);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ScorePosition);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.UpsideDown);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.LightFramePerSecond);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.SeparateScore);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.KeysUnderNotes);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.SplitStages);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.StageSeparation);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.WidthForNoteHeightScale);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ComboBurstStyle);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ImageLookups);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.CustomColours);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.FlipSettings);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.NoteBodyStyle);
                    assertSame(decodedMania, decodedAfterEncodeMania!, mania => mania.ColumnNoteBodyStyles);
                }
            });
        }

        private LegacySkin decode(Stream stream)
        {
            using var sourceReader = new LineBufferedReader(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var mainConfig = new LegacySkinDecoder().Decode(sourceReader);
            stream.Seek(0, SeekOrigin.Begin);
            var maniaConfigs = new LegacyManiaSkinDecoder().Decode(sourceReader);

            var skin = new LegacySkin(mainConfig.SkinInfo, null!);
            skin.Configuration = mainConfig;
            skin.ManiaConfigurations.AddRange(maniaConfigs.ToDictionary(cfg => cfg.Keys));

            return skin;
        }

        private MemoryStream encode(LegacySkin skin)
        {
            var stream = new MemoryStream();
            using var destinationWriter = new StreamWriter(stream, leaveOpen: true);
            new LegacySkinEncoder(skin).Encode(destinationWriter);
            return stream;
        }

        private void assertSame<TContainer, TValue>(TContainer expected, TContainer actual, Expression<Func<TContainer, TValue>> accessorExpression)
            where TContainer : notnull
        {
            var accessor = accessorExpression.Compile();
            Assert.That(accessor(actual), Is.EqualTo(accessor(expected)), $"Mismatch in {accessorExpression}");
        }
    }
}
