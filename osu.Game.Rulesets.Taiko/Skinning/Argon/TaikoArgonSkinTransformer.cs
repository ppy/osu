// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public class TaikoArgonSkinTransformer : SkinTransformer
    {
        public TaikoArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup component)
        {
            switch (component)
            {
                case GameplaySkinComponentLookup<HitResult> resultComponent:
                    // This should eventually be moved to a skin setting, when supported.
                    if (Skin is ArgonProSkin && resultComponent.Component >= HitResult.Great)
                        return Drawable.Empty();

                    return new ArgonJudgementPiece(resultComponent.Component);

                case TaikoSkinComponentLookup taikoComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (taikoComponent.Component)
                    {
                        case TaikoSkinComponents.CentreHit:
                            return new ArgonCentreCirclePiece();

                        case TaikoSkinComponents.RimHit:
                            return new ArgonRimCirclePiece();

                        case TaikoSkinComponents.PlayfieldBackgroundLeft:
                            return new ArgonPlayfieldBackgroundLeft();

                        case TaikoSkinComponents.PlayfieldBackgroundRight:
                            return new ArgonPlayfieldBackgroundRight();

                        case TaikoSkinComponents.InputDrum:
                            return new ArgonInputDrum();

                        case TaikoSkinComponents.HitTarget:
                            return new ArgonHitTarget();

                        case TaikoSkinComponents.BarLine:
                            return new ArgonBarLine();

                        case TaikoSkinComponents.DrumRollBody:
                            return new ArgonElongatedCirclePiece();

                        case TaikoSkinComponents.DrumRollTick:
                            return new ArgonTickPiece();

                        case TaikoSkinComponents.TaikoExplosionKiai:
                            // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                            return Drawable.Empty().With(d => d.Expire());

                        case TaikoSkinComponents.TaikoExplosionGreat:
                        case TaikoSkinComponents.TaikoExplosionMiss:
                        case TaikoSkinComponents.TaikoExplosionOk:
                            return new ArgonHitExplosion(taikoComponent.Component);

                        case TaikoSkinComponents.Swell:
                            return new ArgonSwellCirclePiece();
                    }

                    break;
            }

            return base.GetDrawableComponent(component);
        }

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            if (sampleInfo is HitSampleInfo hitSampleInfo)
                return base.GetSample(new AdjustedHitSampleInfo(hitSampleInfo));

            return base.GetSample(sampleInfo);
        }

        private class AdjustedHitSampleInfo : HitSampleInfo
        {
            // public const int SAMPLE_VOLUME_THRESHOLD_HARD = 90;
            // public const int SAMPLE_VOLUME_THRESHOLD_MEDIUM = 60;
            //
            public AdjustedHitSampleInfo(HitSampleInfo sampleInfo)
                : base(sampleInfo.Name, sampleInfo.Bank, sampleInfo.Suffix, getAdjustedVolume(sampleInfo.Name, sampleInfo.Volume))
            {
            }

            private static int getAdjustedVolume(string name, int volume)
            {
                switch (name)
                {
                    // These samples are to be ignored for argon.
                    case HIT_FINISH:
                    case HIT_WHISTLE:
                        return 0;
                }

                return volume;
            }

            public override IEnumerable<string> LookupNames
            {
                get
                {
                    foreach (string name in base.LookupNames)
                        yield return name.Insert(name.LastIndexOf('/') + 1, "taiko-");
                }
            }

            // private static string getBank(string originalBank, string sampleName, int volume)
            // {
            //     // So basically we're overwriting mapper's bank intentions here.
            //     // The rationale is that most taiko beatmaps only use a single bank, but regularly adjust volume.
            //
            //     switch (sampleName)
            //     {
            //         case HIT_NORMAL:
            //         case HIT_CLAP:
            //         {
            //             if (volume >= SAMPLE_VOLUME_THRESHOLD_HARD)
            //                 return BANK_DRUM;
            //
            //             if (volume >= SAMPLE_VOLUME_THRESHOLD_MEDIUM)
            //                 return BANK_NORMAL;
            //
            //             return BANK_SOFT;
            //         }
            //
            //         default:
            //             return originalBank;
            //     }
            // }
        }
    }
}
