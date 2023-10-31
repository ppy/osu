// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public class TaikoLegacySkinTransformer : LegacySkinTransformer
    {
        public override bool IsProvidingLegacyResources => base.IsProvidingLegacyResources || hasHitCircle || hasBarLeft;

        private readonly Lazy<bool> hasExplosion;

        private bool hasHitCircle => GetTexture("taikohitcircle") != null;
        private bool hasBarLeft => GetTexture("taiko-bar-left") != null;

        public TaikoLegacySkinTransformer(ISkin skin)
            : base(skin)
        {
            hasExplosion = new Lazy<bool>(() => GetTexture(getHitName(HitResult.Great)) != null);
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            if (lookup is GameplaySkinComponentLookup<HitResult> hitResultLookup)
            {
                // if a taiko skin is providing explosion sprites, use a slightly customised judgement version.
                if (hasExplosion.Value)
                {
                    bool isMiss = hitResultLookup.Component == HitResult.Miss;
                    string hitName = getHitName(hitResultLookup.Component);
                    var hitSprite = this.GetAnimation(hitName, true, false);

                    if (hitSprite != null)
                    {
                        var strongHitSprite = isMiss ? null : this.GetAnimation($"{hitName}k", true, false);

                        return new LegacyTaikoJudgementPiece(hitResultLookup.Component, hitSprite, strongHitSprite);
                    }
                }
            }

            if (lookup is TaikoSkinComponentLookup taikoComponent)
            {
                switch (taikoComponent.Component)
                {
                    case TaikoSkinComponents.DrumRollBody:
                        if (GetTexture("taiko-roll-middle") != null)
                            return new LegacyDrumRoll();

                        return null;

                    case TaikoSkinComponents.InputDrum:
                        if (hasBarLeft)
                            return new LegacyInputDrum();

                        return null;

                    case TaikoSkinComponents.DrumSamplePlayer:
                        return null;

                    case TaikoSkinComponents.CentreHit:
                    case TaikoSkinComponents.RimHit:
                        if (hasHitCircle)
                            return new LegacyHit(taikoComponent.Component);

                        return null;

                    case TaikoSkinComponents.DrumRollTick:
                        return this.GetAnimation("sliderscorepoint", false, false);

                    case TaikoSkinComponents.Swell:
                        // todo: support taiko legacy swell (https://github.com/ppy/osu/issues/13601).
                        return null;

                    case TaikoSkinComponents.HitTarget:
                        if (GetTexture("taikobigcircle") != null)
                            return new TaikoLegacyHitTarget();

                        return null;

                    case TaikoSkinComponents.PlayfieldBackgroundRight:
                        if (GetTexture("taiko-bar-right") != null)
                            return new TaikoLegacyPlayfieldBackgroundRight();

                        return null;

                    case TaikoSkinComponents.PlayfieldBackgroundLeft:
                        // This is displayed inside LegacyInputDrum. It is required to be there for layout purposes (can be seen on legacy skins).
                        if (GetTexture("taiko-bar-right") != null)
                            return Drawable.Empty();

                        return null;

                    case TaikoSkinComponents.BarLine:
                        if (GetTexture("taiko-barline") != null)
                            return new LegacyBarLine();

                        return null;

                    case TaikoSkinComponents.TaikoExplosionMiss:
                    case TaikoSkinComponents.TaikoExplosionOk:
                    case TaikoSkinComponents.TaikoExplosionGreat:
                        return Drawable.Empty().With(d => d.Expire());

                    case TaikoSkinComponents.TaikoExplosionKiai:
                        // suppress the default kiai explosion if the skin brings its own sprites.
                        // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                        if (hasExplosion.Value)
                            return Drawable.Empty().With(d => d.Expire());

                        return null;

                    case TaikoSkinComponents.Scroller:
                        if (GetTexture("taiko-slider") != null)
                            return new LegacyTaikoScroller();

                        return null;

                    case TaikoSkinComponents.Mascot:
                        return new DrawableTaikoMascot();

                    case TaikoSkinComponents.KiaiGlow:
                        if (GetTexture("taiko-glow") != null)
                            return new LegacyKiaiGlow();

                        return null;

                    default:
                        throw new UnsupportedSkinComponentException(lookup);
                }
            }

            return base.GetDrawableComponent(lookup);
        }

        private string getHitName(HitResult component)
        {
            switch (component)
            {
                case HitResult.Miss:
                    return "taiko-hit0";

                case HitResult.Ok:
                    return "taiko-hit100";

                case HitResult.Great:
                    return "taiko-hit300";

                default:
                    return string.Empty;
            }
        }

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            if (sampleInfo is HitSampleInfo hitSampleInfo)
                return base.GetSample(new LegacyTaikoSampleInfo(hitSampleInfo));

            return base.GetSample(sampleInfo);
        }

        private class LegacyTaikoSampleInfo : HitSampleInfo
        {
            public LegacyTaikoSampleInfo(HitSampleInfo sampleInfo)
                : base(sampleInfo.Name, sampleInfo.Bank, sampleInfo.Suffix, sampleInfo.Volume)

            {
            }

            public override IEnumerable<string> LookupNames
            {
                get
                {
                    foreach (string name in base.LookupNames)
                        yield return name.Insert(name.LastIndexOf('/') + 1, "taiko-");
                }
            }
        }
    }
}
